using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class MaintenanceSupervisorTests
    {
        private static readonly TimeSpan _maintenancePeriod = TimeSpan.FromMinutes(1);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly MaintenanceSupervisor _supervisor;

        public MaintenanceSupervisorTests()
        {
            RoomService roomService = SampleServices.CreateRoomService(_callerSessions, new RoomRegistry(), _fakeTimeProvider);
            AvatarUploadRegistry avatarUploadRegistry = SampleServices.CreateAvatarUploadRegistry(_callerSessions, _fakeTimeProvider);
            _supervisor = new MaintenanceSupervisor(roomService, avatarUploadRegistry, new MaintenanceTiming(_maintenancePeriod));
        }

        [Fact]
        public void Test_Start_WithoutInactivityLimit_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _supervisor.Start(TimeSpan.Zero));
        }

        [Fact]
        public async Task Test_StopAsync_BeforeStarting_FinishesWithoutError()
        {
            Exception? stopFailure = await Record.ExceptionAsync(() => _supervisor.StopAsync());

            Assert.Null(stopFailure);
        }
    }
}
