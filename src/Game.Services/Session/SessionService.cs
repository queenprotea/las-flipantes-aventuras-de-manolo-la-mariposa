using System;
using System.Threading.Tasks;

using Game.Contracts;

namespace Game.Services
{
    public sealed class SessionService : ISessionService
    {
        private readonly SessionRegistry _sessionRegistry;
        private readonly SessionTokenIssuer _sessionTokenIssuer;
        private readonly RoomService _roomService;
        private readonly MatchService _matchService;
        private readonly AvatarUploadRegistry _avatarUploadRegistry;

        public SessionService(SessionRegistry sessionRegistry, SessionTokenIssuer sessionTokenIssuer, RoomService roomService, MatchService matchService, AvatarUploadRegistry avatarUploadRegistry)
        {
            _sessionRegistry = sessionRegistry;
            _sessionTokenIssuer = sessionTokenIssuer;
            _roomService = roomService;
            _matchService = matchService;
            _avatarUploadRegistry = avatarUploadRegistry;
        }

        public OpenSessionResult OpenSession(string sessionToken)
        {
            SessionTokenReading tokenReading = string.IsNullOrWhiteSpace(sessionToken)
                ? new SessionTokenReading(SessionTokenStatus.Invalid, 0, string.Empty, false)
                : CheckMatchSeat(_sessionTokenIssuer.Read(sessionToken));
            OpenSessionResult opening = tokenReading.Status switch
            {
                SessionTokenStatus.Valid => OpenFor(ToOwner(tokenReading)),
                SessionTokenStatus.Invalid => new OpenSessionResult { Status = OpenSessionStatus.InvalidSessionToken },
                SessionTokenStatus.Expired => new OpenSessionResult { Status = OpenSessionStatus.SessionTokenExpired },
                _ => throw new ArgumentOutOfRangeException(nameof(sessionToken)),
            };

            return opening;
        }

        public async Task<CloseSessionResult> CloseSessionAsync()
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            if (caller is not null)
            {
                _sessionRegistry.Close(caller);
                _avatarUploadRegistry.DiscardForClosedSession(caller.Owner.OwnerId);
                await _roomService.LeaveForClosedSessionAsync(caller.Owner.OwnerId);
            }

            return CloseSessionResult.SessionClosed;
        }

        private static SessionOwner ToOwner(SessionTokenReading tokenReading)
        {
            SessionOwner owner = tokenReading.IsGuest
                ? SessionOwner.ForGuest(tokenReading.OwnerId, tokenReading.DisplayName)
                : SessionOwner.ForPlayer(tokenReading.OwnerId, tokenReading.DisplayName);

            return owner;
        }

        private SessionTokenReading CheckMatchSeat(SessionTokenReading tokenReading)
        {
            bool isSeatGone = (tokenReading.Status == SessionTokenStatus.Valid)
                && (tokenReading.MatchId is int matchId)
                && !_matchService.HoldsSeatIn(tokenReading.OwnerId, matchId);
            SessionTokenReading checkedReading = isSeatGone ? tokenReading with { Status = SessionTokenStatus.Expired } : tokenReading;

            return checkedReading;
        }

        private OpenSessionResult OpenFor(SessionOwner owner)
        {
            _sessionRegistry.Open(owner);
            var opening = new OpenSessionResult
            {
                Status = OpenSessionStatus.SessionOpened,
                Identity = new SessionIdentity
                {
                    DisplayName = owner.DisplayName,
                    IsGuest = owner.IsGuest,
                    PlayerId = owner.IsGuest ? null : owner.OwnerId,
                },
                CurrentRoom = _roomService.Reattach(owner.OwnerId),
                CurrentMatch = _matchService.FindSeating(owner.OwnerId),
            };

            return opening;
        }
    }
}
