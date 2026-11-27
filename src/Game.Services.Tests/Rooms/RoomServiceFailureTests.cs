using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class RoomServiceFailureTests
    {
        private const int SomeRoomId = 1;
        private const int UndefinedVisibility = 99;

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly RoomService _roomService;

        public RoomServiceFailureTests()
        {
            _roomService = new RoomService(new RoomRegistry(), _callerSessions.Registry, new RecordingRoomRecordRepository(), new FailingSanctionRepository(), new RecordingPlayerRepository(), _fakeTimeProvider);
            _callerSessions.Open(SamplePlayers.CaroGuest);
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public async Task Test_CreateRoomAsync_WithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            CreateRoomResult creation = await _roomService.CreateRoomAsync(RoomVisibility.Public);

            Assert.Equal(CreateRoomStatus.DatabaseUnavailable, creation.Status);
        }

        [Fact]
        public async Task Test_JoinRoomAsync_WithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            JoinRoomResult joining = await _roomService.JoinRoomAsync(SomeRoomId);

            Assert.Equal(JoinRoomStatus.DatabaseUnavailable, joining.Status);
        }

        [Fact]
        public async Task Test_CreateRoomAsync_AsAGuestWithTheDatabaseDown_SkipsTheBanCheck()
        {
            _callerSessions.ActAs(SamplePlayers.CaroGuest);

            CreateRoomResult creation = await _roomService.CreateRoomAsync(RoomVisibility.Public);

            Assert.Equal(CreateRoomStatus.Created, creation.Status);
        }

        [Fact]
        public async Task Test_CreateRoomAsync_WithAnUndefinedVisibility_ReturnsInvalidVisibility()
        {
            CreateRoomResult creation = await _roomService.CreateRoomAsync((RoomVisibility)UndefinedVisibility);

            Assert.Equal(CreateRoomStatus.InvalidVisibility, creation.Status);
        }

        [Fact]
        public async Task Test_CreateRoomAsync_WithoutSession_ReturnsNotIdentified()
        {
            _callerSessions.Lose(SamplePlayers.Ana);

            CreateRoomResult creation = await _roomService.CreateRoomAsync(RoomVisibility.Public);

            Assert.Equal(CreateRoomStatus.NotIdentified, creation.Status);
        }
    }
}
