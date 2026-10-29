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

        private long _lastHeartbeatTicks;
        private Task _senderTask = Task.CompletedTask;

        internal PlayerSession(SessionOwner owner, string sessionId, IGameSessionNoticeReceiver noticeReceiver, SessionRegistry sessionRegistry, DateTime openedAtUtc)
        {
            Owner = owner;
            SessionId = sessionId;
            _noticeReceiver = noticeReceiver;
            _sessionRegistry = sessionRegistry;
            _lastHeartbeatTicks = openedAtUtc.Ticks;
        }

        internal SessionOwner Owner { get; }

        internal string SessionId { get; }

        internal bool WasTerminated { get; private set; }

        internal void Watch()
        {
            if (_noticeReceiver is ICommunicationObject watchedChannel)
            {
                watchedChannel.Closed += NoticeReceiverOnClosed;
                watchedChannel.Faulted += NoticeReceiverOnFaulted;
            }
            else
            {
                _log.WarnFormat(UnwatchableChannelMessage, Owner.PlayerId);
            }

            _senderTask = Task.Run(SendPendingAsync);
        }

        internal void Enqueue(Notice notice)
        {
            _outbox.Writer.TryWrite(notice);
        }

        internal void Touch(DateTime nowUtc)
        {
            Interlocked.Exchange(ref _lastHeartbeatTicks, nowUtc.Ticks);
        }

        internal bool IsSilentSince(DateTime limitUtc)
        {
            bool isSilent = Interlocked.Read(ref _lastHeartbeatTicks) < limitUtc.Ticks;

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
            string failureMessage = string.Format(CultureInfo.InvariantCulture, DeliveryFailedMessage, Owner.PlayerId);
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
                _log.Warn(string.Format(CultureInfo.InvariantCulture, ReplacedCloseFailedMessage, Owner.PlayerId), exception);
                terminatedChannel.Abort();
            }
            catch (TimeoutException exception)
            {
                _log.Warn(string.Format(CultureInfo.InvariantCulture, ReplacedCloseFailedMessage, Owner.PlayerId), exception);
                terminatedChannel.Abort();
            }
        }

        private void NoticeReceiverOnClosed(object? sender, EventArgs e)
        {
            _sessionRegistry.DeclareLost(this);
        }

        private void NoticeReceiverOnFaulted(object? sender, EventArgs e)
        {
            _sessionRegistry.DeclareLost(this);
        }
    }
}
