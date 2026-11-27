using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Match;
using Game.Persistence;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class RoomServiceRestoreTests
    {
        private const string RoomCode = "K7Q2PX";
        private const int BetoMemberId = 1;
        private const int AnaMemberId = 2;
        private const int CaroMemberId = 3;
        private const int OneRefusedCreation = 1;

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly RoomRegistry _roomRegistry = new RoomRegistry();
        private readonly RecordingRoomRecordRepository _roomRecordRepository = new RecordingRoomRecordRepository();
        private readonly MatchEntrant _betoEntrant = new MatchEntrant(SamplePlayers.BetoId, BetoMemberId, SamplePlayers.BetoUsername, false, true, SeatColor.Green);
        private readonly MatchEntrant _anaEntrant = new MatchEntrant(SamplePlayers.AnaId, AnaMemberId, SamplePlayers.AnaUsername, false, true, SeatColor.Blue);
        private readonly MatchEntrant _caroEntrant = new MatchEntrant(SamplePlayers.CaroGuestId, CaroMemberId, SamplePlayers.CaroAlias, true, true, SeatColor.Green);

        public RoomServiceRestoreTests()
        {
            _callerSessions.Open(SamplePlayers.Beto);
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public async Task Test_RestoreRoomsAsync_WithEntrants_OpensTheRoomAgain()
        {
            RoomService roomService = CreateRoomService(_roomRecordRepository);

            await roomService.RestoreRoomsAsync(CreateInterruptedRooms(_betoEntrant, _anaEntrant));

            LiveRoom? restoredRoom = _roomRegistry.FindByCode(RoomCode);
            Assert.NotNull(restoredRoom);
        }

        [Fact]
        public async Task Test_RestoreRoomsAsync_WithEntrants_LeavesTheMatchInProgress()
        {
            RoomService roomService = CreateRoomService(_roomRecordRepository);

            await roomService.RestoreRoomsAsync(CreateInterruptedRooms(_betoEntrant, _anaEntrant));

            bool hasMatchInProgress = roomService.HasMatchInProgress(SamplePlayers.AnaId);
            Assert.True(hasMatchInProgress);
        }

        [Fact]
        public async Task Test_RestoreRoomsAsync_WithAGuestEntrant_SeatsTheGuest()
        {
            RoomService roomService = CreateRoomService(_roomRecordRepository);

            await roomService.RestoreRoomsAsync(CreateInterruptedRooms(_betoEntrant, _caroEntrant));

            LiveRoom? guestRoom = _roomRegistry.FindRoomOf(SamplePlayers.CaroGuestId);
            Assert.NotNull(guestRoom);
        }

        [Fact]
        public async Task Test_RestoreRoomsAsync_WithoutEntrants_OpensNoRoom()
        {
            RoomService roomService = CreateRoomService(_roomRecordRepository);

            await roomService.RestoreRoomsAsync(CreateInterruptedRooms());

            IReadOnlyList<LiveRoom> openRooms = _roomRegistry.ListRooms();
            Assert.Empty(openRooms);
        }

        [Fact]
        public async Task Test_RestoreRoomsAsync_WithTheCodeInUse_OpensOnlyOneRoom()
        {
            RoomService roomService = CreateRoomService(_roomRecordRepository);
            await roomService.RestoreRoomsAsync(CreateInterruptedRooms(_betoEntrant));

            await roomService.RestoreRoomsAsync(CreateInterruptedRooms(_anaEntrant));

            IReadOnlyList<LiveRoom> openRooms = _roomRegistry.ListRooms();
            Assert.Single(openRooms);
        }

        [Fact]
        public async Task Test_RestoreRoomsAsync_WithARefusedRecord_OpensNoRoom()
        {
            _roomRecordRepository.RefusedCreations = OneRefusedCreation;
            RoomService roomService = CreateRoomService(_roomRecordRepository);

            await roomService.RestoreRoomsAsync(CreateInterruptedRooms(_betoEntrant));

            IReadOnlyList<LiveRoom> openRooms = _roomRegistry.ListRooms();
            Assert.Empty(openRooms);
        }

        [Fact]
        public async Task Test_RestoreRoomsAsync_WithTheDatabaseDown_OpensNoRoom()
        {
            RoomService roomService = CreateRoomService(new FailingRoomRecordRepository());

            await roomService.RestoreRoomsAsync(CreateInterruptedRooms(_betoEntrant));

            IReadOnlyList<LiveRoom> openRooms = _roomRegistry.ListRooms();
            Assert.Empty(openRooms);
        }

        [Fact]
        public async Task Test_Reattach_WhileTheHostIsAway_MakesTheReturningPlayerHost()
        {
            RoomService roomService = CreateRoomService(_roomRecordRepository);
            await roomService.RestoreRoomsAsync(CreateInterruptedRooms(_betoEntrant, _anaEntrant));

            Room? anaRoom = roomService.Reattach(SamplePlayers.AnaId);

            Assert.Equal(AnaMemberId, anaRoom?.HostMemberId);
        }

        [Fact]
        public async Task Test_Reattach_WhileTheHostIsBack_KeepsTheHost()
        {
            RoomService roomService = CreateRoomService(_roomRecordRepository);
            await roomService.RestoreRoomsAsync(CreateInterruptedRooms(_betoEntrant, _anaEntrant));
            roomService.Reattach(SamplePlayers.BetoId);

            Room? anaRoom = roomService.Reattach(SamplePlayers.AnaId);

            Assert.Equal(BetoMemberId, anaRoom?.HostMemberId);
        }

        private static List<InterruptedRoom> CreateInterruptedRooms(params MatchEntrant[] entrants)
        {
            var interruptedRooms = new List<InterruptedRoom> { new InterruptedRoom(RoomCode, entrants) };

            return interruptedRooms;
        }

        private RoomService CreateRoomService(IRoomRecordRepository roomRecordRepository)
        {
            var roomService = new RoomService(_roomRegistry, _callerSessions.Registry, roomRecordRepository, new RecordingSanctionRepository(), new RecordingPlayerRepository(), _fakeTimeProvider);

            return roomService;
        }
    }
}
