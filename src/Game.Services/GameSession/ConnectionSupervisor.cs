using System;
using System.Threading;
using System.Threading.Tasks;

namespace Game.Services
{
    public sealed class ConnectionSupervisor
    {
        private readonly SessionRegistry _sessionRegistry;
        private readonly RoomService _roomService;
        private readonly SessionTimings _sessionTimings;

        private PeriodicTimer? _heartbeatTimer;
        private Task _supervisionTask = Task.CompletedTask;
        private TimeSpan _roomInactivityLimit;

        public ConnectionSupervisor(SessionRegistry sessionRegistry, RoomService roomService, SessionTimings sessionTimings)
        {
            ArgumentNullException.ThrowIfNull(sessionRegistry);
            ArgumentNullException.ThrowIfNull(roomService);
            ArgumentNullException.ThrowIfNull(sessionTimings);

            _sessionRegistry = sessionRegistry;
            _roomService = roomService;
            _sessionTimings = sessionTimings;
        }

        public void Start(TimeSpan roomInactivityLimit)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(roomInactivityLimit, TimeSpan.Zero);

            _roomInactivityLimit = roomInactivityLimit;
            _heartbeatTimer = new PeriodicTimer(_sessionTimings.HeartbeatPeriod);
            _supervisionTask = Task.Run(SuperviseAsync);
        }

        public async Task StopAsync()
        {
            _heartbeatTimer?.Dispose();
            await _supervisionTask;
        }

        private async Task SuperviseAsync()
        {
            while ((_heartbeatTimer is not null) && await _heartbeatTimer.WaitForNextTickAsync())
            {
                DropSilentSessions();
                await _roomService.RemoveExpiredMembersAsync(_roomInactivityLimit);
            }
        }

        private void DropSilentSessions()
        {
            foreach (PlayerSession silentSession in _sessionRegistry.FindSilentSessions())
            {
                _sessionRegistry.DeclareLost(silentSession);
            }
        }
    }
}
