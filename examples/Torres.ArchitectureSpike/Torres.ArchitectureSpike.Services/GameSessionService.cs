using System;
using System.Threading.Tasks;

using CoreWCF;

using Torres.ArchitectureSpike.Contracts;
using Torres.ArchitectureSpike.Match;

namespace Torres.ArchitectureSpike.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public sealed class GameSessionService : IGameSessionService
    {
        private readonly SessionRegistry _sessionRegistry;
        private readonly MatchCoordinator _matchCoordinator;

        public GameSessionService(SessionRegistry sessionRegistry, MatchCoordinator matchCoordinator)
        {
            ArgumentNullException.ThrowIfNull(sessionRegistry);
            ArgumentNullException.ThrowIfNull(matchCoordinator);

            _sessionRegistry = sessionRegistry;
            _matchCoordinator = matchCoordinator;
        }

        public Task<ConnectStatus> ConnectAsync(string playerName)
        {
            ConnectStatus connectStatus = ConnectStatus.InvalidName;
            if (!string.IsNullOrWhiteSpace(playerName))
            {
                OperationContext operationContext = OperationContext.Current;
                IGameSessionCallback callbackChannel = operationContext.GetCallbackChannel<IGameSessionCallback>();
                connectStatus = _sessionRegistry.Register(playerName, callbackChannel, operationContext.SessionId);
            }

            Task<ConnectStatus> connectResult = Task.FromResult(connectStatus);

            return connectResult;
        }

        public Task<HeartbeatReply> HeartbeatAsync()
        {
            var heartbeatReply = new HeartbeatReply { ServerTimeUtc = DateTime.UtcNow };
            Task<HeartbeatReply> heartbeatResult = Task.FromResult(heartbeatReply);

            return heartbeatResult;
        }

        public Task<MatchEntryStatus> JoinMatchAsync()
        {
            MatchEntryStatus entryStatus = MatchEntryStatus.NotConnected;
            string? callerName = FindCallerName();
            if (callerName is not null)
            {
                entryStatus = _matchCoordinator.Join(callerName);
            }

            Task<MatchEntryStatus> entryResult = Task.FromResult(entryStatus);

            return entryResult;
        }

        public Task<ActionResult> AdvanceTokenAsync(int steps)
        {
            ActionResult actionResult = ActionResult.NotInMatch;
            string? callerName = FindCallerName();
            if (callerName is not null)
            {
                actionResult = _matchCoordinator.Advance(callerName, steps);
            }

            Task<ActionResult> advanceResult = Task.FromResult(actionResult);

            return advanceResult;
        }

        private string? FindCallerName()
        {
            string sessionId = OperationContext.Current.SessionId;
            string? callerName = _sessionRegistry.FindPlayerName(sessionId);

            return callerName;
        }
    }
}
