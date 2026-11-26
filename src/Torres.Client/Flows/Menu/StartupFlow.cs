using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class StartupFlow
    {
        private readonly ServerStatusConnection _statusConnection;
        private readonly PendingRequest<ServerStatus> _statusCheck = new PendingRequest<ServerStatus>();

        private ServerCheckState _state = ServerCheckState.Checking;

        internal StartupFlow(ServerStatusConnection statusConnection)
        {
            _statusConnection = statusConnection;
        }

        internal ServerCheckState FollowCheck()
        {
            if ((_state == ServerCheckState.Checking) && !_statusCheck.IsWaiting)
            {
                _statusCheck.Start(_statusConnection.GetStatusAsync());
            }

            FinishedRequest<ServerStatus>? finishedCheck = _statusCheck.TakeFinished();
            if (finishedCheck is not null)
            {
                _statusConnection.ReleaseChannel();
                bool isServerReady = finishedCheck.IsSuccessful && (finishedCheck.Reply == ServerStatus.Ready);
                _state = isServerReady ? ServerCheckState.Ready : ServerCheckState.Unreachable;
            }

            return _state;
        }

        internal void Retry()
        {
            _statusConnection.ReleaseChannel();
            _statusCheck.Forget();
            _state = ServerCheckState.Checking;
        }
    }
}
