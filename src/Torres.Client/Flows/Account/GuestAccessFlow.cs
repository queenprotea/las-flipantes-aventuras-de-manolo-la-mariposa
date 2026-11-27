using System;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class GuestAccessFlow
    {
        private readonly IAccountConnection _accountConnection;
        private readonly IGameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly PendingRequest<GuestAccessResult> _guestAccessRequest = new PendingRequest<GuestAccessResult>();
        private readonly PendingRequest<OpenSessionResult> _sessionOpening = new PendingRequest<OpenSessionResult>();

        private string? _enteringAlias;
        private GuestAccessOutcome? _outcome;

        internal GuestAccessFlow(IAccountConnection accountConnection, IGameSessionConnection gameSessionConnection, PlayerSession playerSession)
        {
            _accountConnection = accountConnection;
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal bool IsWaiting => _guestAccessRequest.IsWaiting || _sessionOpening.IsWaiting;

        internal static ScreenId FindScreenToCancel()
        {
            ScreenId nextScreen = ScreenId.Login;

            return nextScreen;
        }

        internal string? StartGuestAccess(string alias)
        {
            string guestAlias = alias.Trim();
            string? errorKey = null;
            if (guestAlias.Length == 0)
            {
                errorKey = TextKeys.Common.RequiredField;
            }
            else if (!IsWaiting)
            {
                _enteringAlias = guestAlias;
                _guestAccessRequest.Start(_accountConnection.EnterAsGuestAsync(guestAlias));
            }

            return errorKey;
        }

        internal GuestAccessOutcome? TakeOutcome()
        {
            FollowGuestAccess();
            FollowSessionOpening();
            GuestAccessOutcome? outcome = _outcome;
            _outcome = null;

            return outcome;
        }

        private void FollowGuestAccess()
        {
            FinishedRequest<GuestAccessResult>? finishedGuestAccess = _guestAccessRequest.TakeFinished();
            if (finishedGuestAccess is null)
            {
                return;
            }

            _accountConnection.ReleaseChannel();
            if (finishedGuestAccess.IsSuccessful && (finishedGuestAccess.Reply is GuestAccessResult guestAccessResult))
            {
                FollowGuestAccessResult(guestAccessResult);
            }
            else
            {
                ReportServerUnreachable();
            }
        }

        private void FollowGuestAccessResult(GuestAccessResult guestAccessResult)
        {
            switch (guestAccessResult.Status)
            {
                case GuestAccessStatus.Entered:
                    OpenSession(guestAccessResult.SessionToken);
                    break;

                case GuestAccessStatus.AliasRequired:
                case GuestAccessStatus.AliasTooLong:
                    _enteringAlias = null;
                    _outcome = new GuestAccessOutcome(TextKeys.Common.RequiredField, null, false);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(guestAccessResult), guestAccessResult.Status, null);
            }
        }

        private void OpenSession(string? sessionToken)
        {
            if (string.IsNullOrEmpty(sessionToken))
            {
                ReportServerUnreachable();
            }
            else
            {
                _sessionOpening.Start(_gameSessionConnection.OpenAsync(sessionToken));
            }
        }

        private void FollowSessionOpening()
        {
            FinishedRequest<OpenSessionResult>? finishedOpening = _sessionOpening.TakeFinished();
            if (finishedOpening is null)
            {
                return;
            }

            string? enteringAlias = _enteringAlias;
            _enteringAlias = null;
            OpenSessionResult? openingResult = finishedOpening.IsSuccessful ? finishedOpening.Reply : null;
            if ((enteringAlias is not null) && (openingResult is not null) && (openingResult.Status == OpenSessionStatus.SessionOpened))
            {
                EnterAs(enteringAlias, openingResult);
            }
            else
            {
                _gameSessionConnection.AbandonSession();
                ReportServerUnreachable();
            }
        }

        private void EnterAs(string guestAlias, OpenSessionResult openingResult)
        {
            _gameSessionConnection.MarkSessionOpen();
            _playerSession.StartAsGuest(openingResult.Identity?.DisplayName ?? guestAlias);
            ScreenId nextScreen = ScreenId.MainMenu;
            if (openingResult.CurrentRoom is not null)
            {
                _playerSession.EnterRoom(openingResult.CurrentRoom);
                nextScreen = ScreenId.Room;
            }

            _outcome = new GuestAccessOutcome(null, nextScreen, false);
        }

        private void ReportServerUnreachable()
        {
            _enteringAlias = null;
            _outcome = new GuestAccessOutcome(null, null, true);
        }
    }
}
