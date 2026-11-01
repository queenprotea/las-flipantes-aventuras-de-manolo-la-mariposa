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
        private readonly SessionTimings _sessionTimings;
        private readonly TimeProvider _timeProvider;

        public SessionService(SessionRegistry sessionRegistry, SessionTokenIssuer sessionTokenIssuer, RoomService roomService, SessionTimings sessionTimings, TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(sessionRegistry);
            ArgumentNullException.ThrowIfNull(sessionTokenIssuer);
            ArgumentNullException.ThrowIfNull(roomService);
            ArgumentNullException.ThrowIfNull(sessionTimings);
            ArgumentNullException.ThrowIfNull(timeProvider);

            _sessionRegistry = sessionRegistry;
            _sessionTokenIssuer = sessionTokenIssuer;
            _roomService = roomService;
            _sessionTimings = sessionTimings;
            _timeProvider = timeProvider;
        }

        public OpenSessionResult OpenSession(string sessionToken)
        {
            SessionTokenReading tokenReading = string.IsNullOrWhiteSpace(sessionToken)
                ? new SessionTokenReading(SessionTokenStatus.Invalid, 0, string.Empty, false)
                : _sessionTokenIssuer.Read(sessionToken);
            OpenSessionResult opening = tokenReading.Status switch
            {
                SessionTokenStatus.Valid => OpenFor(ToOwner(tokenReading)),
                SessionTokenStatus.Invalid => new OpenSessionResult { Status = OpenSessionStatus.InvalidSessionToken },
                SessionTokenStatus.Expired => new OpenSessionResult { Status = OpenSessionStatus.SessionTokenExpired },
                _ => throw new ArgumentOutOfRangeException(nameof(sessionToken)),
            };

            return opening;
        }

        public ConnectionCheckResponse IsAlive()
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            if (caller is not null)
            {
                _sessionRegistry.Touch(caller);
            }

            var connectionCheck = new ConnectionCheckResponse { ServerTimeUtc = _timeProvider.GetUtcNow().UtcDateTime };

            return connectionCheck;
        }

        public async Task<CloseSessionResult> CloseSessionAsync()
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            if (caller is not null)
            {
                _sessionRegistry.Close(caller);
                await _roomService.LeaveForAsync(caller.Owner.OwnerId);
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
                ConnectionCheckPeriodSeconds = (int)_sessionTimings.ConnectionCheckPeriod.TotalSeconds,
            };

            return opening;
        }
    }
}
