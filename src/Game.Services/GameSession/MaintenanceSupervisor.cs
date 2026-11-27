using System;
using System.Threading;
using System.Threading.Tasks;

namespace Game.Services
{
    public sealed class MaintenanceSupervisor
    {
        private readonly RoomService _roomService;
        private readonly AvatarUploadRegistry _avatarUploadRegistry;
        private readonly MaintenanceTiming _maintenanceTiming;

        private PeriodicTimer? _maintenanceTimer;
        private Task _maintenanceTask = Task.CompletedTask;
        private TimeSpan _roomInactivityLimit;

        public MaintenanceSupervisor(RoomService roomService, AvatarUploadRegistry avatarUploadRegistry, MaintenanceTiming maintenanceTiming)
        {
            ArgumentNullException.ThrowIfNull(roomService);
            ArgumentNullException.ThrowIfNull(avatarUploadRegistry);
            ArgumentNullException.ThrowIfNull(maintenanceTiming);

            _roomService = roomService;
            _avatarUploadRegistry = avatarUploadRegistry;
            _maintenanceTiming = maintenanceTiming;
        }

        public void Start(TimeSpan roomInactivityLimit)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(roomInactivityLimit, TimeSpan.Zero);

            _roomInactivityLimit = roomInactivityLimit;
            _maintenanceTimer = new PeriodicTimer(_maintenanceTiming.Period);
            _maintenanceTask = Task.Run(MaintainAsync);
        }

        public async Task StopAsync()
        {
            _maintenanceTimer?.Dispose();
            await _maintenanceTask;
        }

        private async Task MaintainAsync()
        {
            while ((_maintenanceTimer is not null) && await _maintenanceTimer.WaitForNextTickAsync())
            {
                _avatarUploadRegistry.DiscardIdle();
                await _roomService.RemoveExpiredMembersAsync(_roomInactivityLimit);
            }
        }
    }
}
