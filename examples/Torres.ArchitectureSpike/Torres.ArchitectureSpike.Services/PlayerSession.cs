using System;
using System.Globalization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using CoreWCF;

using log4net;

using Torres.ArchitectureSpike.Contracts;

namespace Torres.ArchitectureSpike.Services
{
    internal sealed class PlayerSession
    {
        private const int SessionActive = 0;
        private const int SessionLost = 1;
        private const string UnwatchableChannelMessage = "The callback channel of {0} is not an ICommunicationObject; only failed deliveries can reveal its loss.";
        private const string DeliveryFailedMessage = "A delivery to {0} failed; the session is declared lost.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(PlayerSession));

        private readonly Channel<SessionDelivery> _outbox = Channel.CreateUnbounded<SessionDelivery>(new UnboundedChannelOptions { SingleReader = true });
        private readonly IGameSessionCallback _callbackChannel;
        private readonly SessionRegistry _sessionRegistry;

        private int _sessionState = SessionActive;
        private Task _senderTask = Task.CompletedTask;

        internal PlayerSession(string playerName, IGameSessionCallback callbackChannel, SessionRegistry sessionRegistry)
        {
            PlayerName = playerName;
            _callbackChannel = callbackChannel;
            _sessionRegistry = sessionRegistry;
        }

        internal string PlayerName { get; }

        internal void Watch()
        {
            if (_callbackChannel is ICommunicationObject callbackObject)
            {
                callbackObject.Closed += CallbackChannelOnClosed;
                callbackObject.Faulted += CallbackChannelOnFaulted;
            }
            else
            {
                _log.WarnFormat(UnwatchableChannelMessage, PlayerName);
            }

            _senderTask = Task.Run(SendPendingAsync);
        }

        internal void Enqueue(SessionDelivery delivery)
        {
            _outbox.Writer.TryWrite(delivery);
        }

        internal void StopSending()
        {
            _outbox.Writer.TryComplete();
        }

        private async Task SendPendingAsync()
        {
            try
            {
                await foreach (SessionDelivery delivery in _outbox.Reader.ReadAllAsync())
                {
                    delivery.SendTo(_callbackChannel);
                }
            }
            catch (Exception exception) when ((exception is CommunicationException) || (exception is TimeoutException) || (exception is ObjectDisposedException))
            {
                _log.Warn(string.Format(CultureInfo.InvariantCulture, DeliveryFailedMessage, PlayerName), exception);
                DeclareLost(PlayerConnectionChange.Faulted);
            }
        }

        private void DeclareLost(PlayerConnectionChange change)
        {
            if (Interlocked.Exchange(ref _sessionState, SessionLost) == SessionLost)
            {
                return;
            }

            _sessionRegistry.DeclareLost(this, change);
        }

        private void CallbackChannelOnClosed(object? sender, EventArgs e)
        {
            DeclareLost(PlayerConnectionChange.Closed);
        }

        private void CallbackChannelOnFaulted(object? sender, EventArgs e)
        {
            DeclareLost(PlayerConnectionChange.Faulted);
        }
    }
}
