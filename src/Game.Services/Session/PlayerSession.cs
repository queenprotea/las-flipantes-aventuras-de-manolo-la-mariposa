using System;
using System.Globalization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;

using log4net;

namespace Game.Services
{
    internal sealed class PlayerSession
    {
        private const string UnwatchableChannelMessage = "The callback channel of player {0} cannot be watched; only a failed notice reveals its loss.";
        private const string DeliveryFailedMessage = "A notice to player {0} could not be delivered; the session is declared lost.";
        private const string ReplacedCloseFailedMessage = "The replaced session of player {0} could not be closed in order and was cut.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(PlayerSession));

        private readonly Channel<Notice> _outbox = Channel.CreateUnbounded<Notice>(new UnboundedChannelOptions { SingleReader = true });
        private readonly IGameSessionNoticeReceiver _noticeReceiver;
        private readonly SessionRegistry _sessionRegistry;

        private long _lastConnectionCheckTicks;
        private Task _senderTask = Task.CompletedTask;

        internal PlayerSession(SessionOwner owner, string sessionId, IGameSessionNoticeReceiver noticeReceiver, SessionRegistry sessionRegistry, DateTime openedAt)
        {
            Owner = owner;
            SessionId = sessionId;
            _noticeReceiver = noticeReceiver;
            _sessionRegistry = sessionRegistry;
            _lastConnectionCheckTicks = openedAt.Ticks;
        }

        internal SessionOwner Owner { get; private set; }

        internal string SessionId { get; }

        internal bool WasTerminated { get; private set; }

        internal void Watch()
        {
            if (_noticeReceiver is ICommunicationObject watchedChannel)
            {
                watchedChannel.Closed += WatchedChannelOnClosed;
                watchedChannel.Faulted += WatchedChannelOnFaulted;
            }
            else
            {
                _log.WarnFormat(CultureInfo.InvariantCulture, UnwatchableChannelMessage, Owner.OwnerId);
            }

            _senderTask = Task.Run(SendPendingAsync);
        }

        internal void ApplyUsername(string username)
        {
            Owner = Owner.WithUsername(username);
        }

        internal void Enqueue(Notice notice)
        {
            _outbox.Writer.TryWrite(notice);
        }

        internal void Touch(DateTime now)
        {
            Interlocked.Exchange(ref _lastConnectionCheckTicks, now.Ticks);
        }

        internal bool IsSilentSince(DateTime limitAt)
        {
            bool isSilent = Interlocked.Read(ref _lastConnectionCheckTicks) < limitAt.Ticks;

            return isSilent;
        }

        internal void Terminate(SessionTerminationReason reason)
        {
            WasTerminated = true;
            _outbox.Writer.TryWrite(new SessionTerminationNotice(reason));
            _outbox.Writer.TryComplete();
        }

        internal void Abandon()
        {
            _outbox.Writer.TryComplete();
        }

        internal void Drop()
        {
            _outbox.Writer.TryComplete();
            if (_noticeReceiver is ICommunicationObject droppedChannel)
            {
                droppedChannel.Abort();
            }
        }

        private async Task SendPendingAsync()
        {
            try
            {
                await foreach (Notice pendingNotice in _outbox.Reader.ReadAllAsync())
                {
                    pendingNotice.SendTo(_noticeReceiver);
                }
            }
            catch (CommunicationException exception)
            {
                ReportFailedDelivery(exception);
            }
            catch (TimeoutException exception)
            {
                ReportFailedDelivery(exception);
            }
            catch (ObjectDisposedException exception)
            {
                ReportFailedDelivery(exception);
            }

            await CloseTerminatedChannelAsync();
        }

        private void ReportFailedDelivery(Exception exception)
        {
            string failureMessage = string.Format(CultureInfo.InvariantCulture, DeliveryFailedMessage, Owner.OwnerId);
            _log.Warn(failureMessage, exception);
            _sessionRegistry.DeclareLost(this);
        }

        private async Task CloseTerminatedChannelAsync()
        {
            if (!WasTerminated || (_noticeReceiver is not ICommunicationObject terminatedChannel))
            {
                return;
            }

            try
            {
                await terminatedChannel.CloseAsync();
            }
            catch (CommunicationException exception)
            {
                _log.Warn(string.Format(CultureInfo.InvariantCulture, ReplacedCloseFailedMessage, Owner.OwnerId), exception);
                terminatedChannel.Abort();
            }
            catch (TimeoutException exception)
            {
                _log.Warn(string.Format(CultureInfo.InvariantCulture, ReplacedCloseFailedMessage, Owner.OwnerId), exception);
                terminatedChannel.Abort();
            }
        }

        private void WatchedChannelOnClosed(object? sender, EventArgs e)
        {
            _sessionRegistry.DeclareLost(this);
        }

        private void WatchedChannelOnFaulted(object? sender, EventArgs e)
        {
            _sessionRegistry.DeclareLost(this);
        }
    }
}
