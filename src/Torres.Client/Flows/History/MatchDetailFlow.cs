using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class MatchDetailFlow
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<MatchDetailResponse> _detailRequest = new PendingRequest<MatchDetailResponse>();

        private int? _requestedMatchId;

        internal MatchDetailFlow(GameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal bool Open()
        {
            _detailRequest.Forget();
            _requestedMatchId = _playerSession.MatchHistory.SelectedMatchId;
            bool canShowMatch = _playerSession.HasAccount && _gameSessionConnection.IsSessionOpen && _requestedMatchId.HasValue;
            if (canShowMatch)
            {
                StartDetailRequest();
            }

            return canShowMatch;
        }

        internal bool Retry()
        {
            bool isStarted = _requestedMatchId.HasValue && StartDetailRequest();

            return isStarted;
        }

        internal MatchDetailOutcome? TakeOutcome()
        {
            MatchDetailOutcome? outcome = null;
            FinishedRequest<MatchDetailResponse>? finishedRequest = _detailRequest.TakeFinished();
            if (finishedRequest is not null)
            {
                MatchDetailResponse? detail = null;
                if (finishedRequest.IsSuccessful && (finishedRequest.Reply is MatchDetailResponse foundDetail))
                {
                    detail = foundDetail;
                }

                outcome = new MatchDetailOutcome(detail);
            }

            return outcome;
        }

        private bool StartDetailRequest()
        {
            bool isSessionOpen = _gameSessionConnection.IsSessionOpen;
            if (isSessionOpen && (_requestedMatchId is int matchId) && !_detailRequest.IsWaiting)
            {
                _detailRequest.Start(_gameSessionConnection.GetMatchDetailAsync(matchId));
            }

            return isSessionOpen;
        }
    }
}
