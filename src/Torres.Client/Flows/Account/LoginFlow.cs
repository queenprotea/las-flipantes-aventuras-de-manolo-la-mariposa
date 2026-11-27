using System;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class LoginFlow
    {
        private readonly AccountConnection _accountConnection;
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<LoginResult> _loginRequest = new PendingRequest<LoginResult>();
        private readonly PendingRequest<OpenSessionResult> _sessionOpening = new PendingRequest<OpenSessionResult>();

        private PlayerIdentity? _loggedInPlayer;
        private LoginOutcome? _outcome;

        internal LoginFlow(AccountConnection accountConnection, GameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _accountConnection = accountConnection;
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal bool IsWaiting => _loginRequest.IsWaiting || _sessionOpening.IsWaiting;

        internal bool CanEnterAsGuest => !_playerSession.IsGuest;

        internal string? StartLogin(string username, string password)
        {
            string? errorKey = null;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                errorKey = TextKeys.Common.RequiredField;
            }
            else if (!IsWaiting)
            {
                _loginRequest.Start(_accountConnection.LoginAsync(username.Trim(), password));
            }

            return errorKey;
        }

        internal LoginOutcome? TakeOutcome()
        {
            FollowLogin();
            FollowSessionOpening();
            LoginOutcome? outcome = _outcome;
            _outcome = null;

            return outcome;
        }

        private static ScreenId FindScreenAfterLogin(OpenSessionResult opening)
        {
            ScreenId nextScreen = ScreenId.MainMenu;
            if ((opening.CurrentMatch is not null) && opening.CurrentMatch.IsAwaitingPlayers)
            {
                nextScreen = ScreenId.MatchInProgress;
            }
            else if (opening.CurrentRoom is not null)
            {
                nextScreen = ScreenId.Room;
            }

            return nextScreen;
        }

        private static LoginOutcome CreateFailure(string errorKey)
        {
            var failure = new LoginOutcome(errorKey, null, false);

            return failure;
        }

        private void FollowLogin()
        {
            FinishedRequest<LoginResult>? finishedLogin = _loginRequest.TakeFinished();
            if (finishedLogin is null)
            {
                return;
            }

            _accountConnection.ReleaseChannel();
            if (finishedLogin.IsSuccessful && (finishedLogin.Reply is LoginResult loginResult))
            {
                FollowLoginResult(loginResult);
            }
            else
            {
                _outcome = CreateFailure(TextKeys.Common.ServerErrorTitle);
            }
        }

        private void FollowLoginResult(LoginResult loginResult)
        {
            switch (loginResult.Status)
            {
                case LoginStatus.LoggedIn:
                    OpenSession(loginResult);
                    break;

                case LoginStatus.InvalidCredentials:
                    _outcome = CreateFailure(TextKeys.Login.InvalidCredentials);
                    break;

                case LoginStatus.AccountBanned:
                    _outcome = new LoginOutcome(null, null, true);
                    break;

                case LoginStatus.DatabaseUnavailable:
                    _outcome = CreateFailure(TextKeys.Common.ServerErrorTitle);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(loginResult), loginResult.Status, null);
            }
        }

        private void OpenSession(LoginResult loginResult)
        {
            if ((loginResult.Player is not PlayerIdentity player) || string.IsNullOrEmpty(loginResult.SessionToken))
            {
                _outcome = CreateFailure(TextKeys.Common.ServerErrorTitle);
                return;
            }

            if (_playerSession.IsGuest)
            {
                _gameSessionConnection.CloseSession();
                _playerSession.End();
            }

            _loggedInPlayer = player;
            _sessionOpening.Start(_gameSessionConnection.OpenAsync(loginResult.SessionToken));
        }

        private void FollowSessionOpening()
        {
            FinishedRequest<OpenSessionResult>? finishedOpening = _sessionOpening.TakeFinished();
            if (finishedOpening is null)
            {
                return;
            }

            PlayerIdentity? loggedInPlayer = _loggedInPlayer;
            _loggedInPlayer = null;
            OpenSessionResult? opening = finishedOpening.IsSuccessful ? finishedOpening.Reply : null;
            if ((loggedInPlayer is not null) && (opening is not null) && (opening.Status == OpenSessionStatus.SessionOpened))
            {
                EnterAs(loggedInPlayer, opening);
            }
            else
            {
                _gameSessionConnection.AbandonSession();
                _outcome = CreateFailure(TextKeys.Common.ServerErrorTitle);
            }
        }

        private void EnterAs(PlayerIdentity player, OpenSessionResult opening)
        {
            _gameSessionConnection.MarkSessionOpen();
            _playerSession.Start(player);
            if (opening.CurrentRoom is not null)
            {
                _playerSession.EnterRoom(opening.CurrentRoom);
            }
            else if (opening.CurrentMatch is not null)
            {
                _playerSession.EnterMatchWithoutRoom(opening.CurrentMatch.RoomCode);
            }

            _outcome = new LoginOutcome(null, FindScreenAfterLogin(opening), false);
        }
    }
}
