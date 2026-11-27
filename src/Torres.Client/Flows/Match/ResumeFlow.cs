using System;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class ResumeFlow
    {
        private readonly IGameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<ReturnToMatchResult> _returnRequest = new PendingRequest<ReturnToMatchResult>();

        internal ResumeFlow(IGameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal MatchView? CurrentMatchView => _playerSession.CurrentMatchView;

        internal void Open()
        {
            bool hasReturned = (_playerSession.CurrentMatchView is MatchView matchView) && PlayerSession.HasViewerReturned(matchView);
            if (!hasReturned && !_returnRequest.IsWaiting)
            {
                _returnRequest.Start(_gameSessionConnection.ReturnToMatchAsync());
            }
        }

        internal TimeSpan FindResumeTimeLeft()
        {
            TimeSpan resumeTimeLeft = TimeSpan.Zero;
            bool isWaiting = _playerSession.CurrentMatchView?.Phase == MatchPhase.AwaitingPlayers;
            if (isWaiting)
            {
                resumeTimeLeft = _playerSession.StepTimeLeft;
            }

            return resumeTimeLeft;
        }

        internal ResumeOutcome? TakeOutcome()
        {
            FinishedRequest<ReturnToMatchResult>? finishedReturn = _returnRequest.TakeFinished();
            ResumeOutcome? outcome = null;
            if ((finishedReturn is not null) && !finishedReturn.IsSuccessful)
            {
                outcome = new ResumeOutcome(Notice.CreateServerError(), false);
            }
            else if ((finishedReturn is not null) && (finishedReturn.Reply != ReturnToMatchResult.Returned))
            {
                outcome = new ResumeOutcome(null, true);
            }

            return outcome;
        }
    }
}
