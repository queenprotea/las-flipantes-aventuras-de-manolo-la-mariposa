using System;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class DisconnectionFlow
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<OpenSessionResult> _reopenRequest = new PendingRequest<OpenSessionResult>();

        private TimeSpan _sinceLastAttempt;
        private bool _hasResumed;
        private bool _isWaitingForMatch;

        internal DisconnectionFlow(GameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal MatchView? CurrentMatchView => _playerSession.CurrentMatchView;

        internal bool CanRetry => !_reopenRequest.IsWaiting && !_hasResumed;

        internal void Open()
        {
            _reopenRequest.Forget();
            _hasResumed = false;
            _isWaitingForMatch = false;
            StartReopening();
        }

        internal void Advance(TimeSpan elapsedTime)
        {
            bool isWaitingToRetry = !_reopenRequest.IsWaiting && !_isWaitingForMatch;
            if (isWaitingToRetry)
            {
                _sinceLastAttempt += elapsedTime;
            }

            if (isWaitingToRetry && (_sinceLastAttempt >= _gameSessionConnection.ConnectionCheckPeriod))
            {
                StartReopening();
            }
        }

        internal void Retry()
        {
            if (!_reopenRequest.IsWaiting)
            {
                StartReopening();
            }
        }

        internal TimeSpan FindReconnectionTimeLeft()
        {
            TimeSpan reconnectionTimeLeft = TimeSpan.Zero;
            if (_playerSession.ReconnectionEndsAtUtc is DateTime reconnectionEndsAt)
            {
                reconnectionTimeLeft = reconnectionEndsAt - DateTime.UtcNow;
            }

            return reconnectionTimeLeft;
        }

        internal DisconnectionOutcome? TakeOutcome()
        {
            FinishedRequest<OpenSessionResult>? finishedReopening = _reopenRequest.TakeFinished();
            DisconnectionOutcome? outcome = null;
            if ((finishedReopening is not null) && finishedReopening.IsSuccessful)
            {
                outcome = FollowReopening(finishedReopening.Reply);
            }
            else if (finishedReopening is not null)
            {
                _gameSessionConnection.AbortSessionChannel();
                outcome = new DisconnectionOutcome(Notice.CreateServerError(), false, false, false, false);
            }

            return outcome;
        }

        private void StartReopening()
        {
            _sinceLastAttempt = TimeSpan.Zero;
            _reopenRequest.Start(_gameSessionConnection.ReopenAsync());
        }

        private DisconnectionOutcome FollowReopening(OpenSessionResult? opening)
        {
            DisconnectionOutcome outcome;
            if ((opening is not null) && (opening.Status == OpenSessionStatus.SessionOpened))
            {
                outcome = ResumeSession(opening);
            }
            else
            {
                _gameSessionConnection.AbortSessionChannel();
                outcome = new DisconnectionOutcome(null, false, true, false, false);
            }

            return outcome;
        }

        private DisconnectionOutcome ResumeSession(OpenSessionResult opening)
        {
            _gameSessionConnection.BeginConnectionChecks(opening);
            _playerSession.RecoverConnection();
            _hasResumed = true;
            if (opening.CurrentRoom is not null)
            {
                _playerSession.EnterRoom(opening.CurrentRoom);
            }

            bool hasMatchEnded = opening.CurrentMatch is null;
            bool isMatchAwaitingPlayers = false;
            if (opening.CurrentMatch is MatchSeating seatedMatch)
            {
                isMatchAwaitingPlayers = FollowSeatedMatch(opening.CurrentRoom, seatedMatch);
            }
            else
            {
                LeaveEndedMatch(opening.CurrentRoom);
            }

            var resumedOutcome = new DisconnectionOutcome(null, true, false, hasMatchEnded, isMatchAwaitingPlayers);

            return resumedOutcome;
        }

        private void LeaveEndedMatch(Room? currentRoom)
        {
            if (currentRoom is null)
            {
                _playerSession.LeaveRoom();
            }
            else
            {
                _playerSession.LeaveMatch();
            }
        }

        private bool FollowSeatedMatch(Room? currentRoom, MatchSeating seatedMatch)
        {
            if (currentRoom is null)
            {
                _playerSession.EnterMatchWithoutRoom(seatedMatch.RoomCode);
            }

            _isWaitingForMatch = !seatedMatch.IsAwaitingPlayers;
            bool isMatchAwaitingPlayers = seatedMatch.IsAwaitingPlayers;

            return isMatchAwaitingPlayers;
        }
    }
}
