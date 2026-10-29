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
                ? new SessionTokenReading(SessionTokenStatus.Invalid, 0, string.Empty)
                : _sessionTokenIssuer.Read(sessionToken);
            OpenSessionResult opening = tokenReading.Status switch
            {
                SessionTokenStatus.Valid => OpenFor(new SessionOwner(tokenReading.PlayerId, tokenReading.Username)),
                SessionTokenStatus.Invalid => new OpenSessionResult { Status = OpenSessionStatus.InvalidSessionToken },
                SessionTokenStatus.Expired => new OpenSessionResult { Status = OpenSessionStatus.SessionTokenExpired },
                _ => throw new ArgumentOutOfRangeException(nameof(sessionToken)),
            };

            return opening;
        }

        public HeartbeatResponse IsAlive()
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            if (caller is not null)
            {
                _sessionRegistry.Touch(caller);
            }

            var heartbeat = new HeartbeatResponse { ServerTimeUtc = _timeProvider.GetUtcNow().UtcDateTime };

            return heartbeat;
        }

        public async Task<CloseSessionResult> CloseSessionAsync()
        {
            PlayerSession? caller = _sessionRegistry.FindCaller();
            if (caller is not null)
            {
                _sessionRegistry.Close(caller);
                await _roomService.LeaveForAsync(caller.Owner.PlayerId);
            }

            return CloseSessionResult.SessionClosed;
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
                    IsGuest = false,
                    PlayerId = owner.PlayerId,
                },
                CurrentRoom = _roomService.Reattach(owner.PlayerId),
                HeartbeatPeriodSeconds = (int)_sessionTimings.HeartbeatPeriod.TotalSeconds,
            };

            return opening;
        }
    }
}
