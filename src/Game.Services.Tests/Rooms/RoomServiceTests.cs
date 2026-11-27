using System;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Persistence;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class RoomServiceTests
    {
        private const int HostMemberId = 1;
        private const int SecondMemberId = 2;
        private const int MissingRoomId = 404;
        private const int NoRoomId = 0;
        private const string UnknownRoomCode = "ZZZZ";
        private const int OneRefusedCode = 1;
        private const int TwoCodeDraws = 2;
        private const string NotInRoomMessage = "The caller is not in any room.";

        private static readonly TimeSpan _temporaryBanLength = TimeSpan.FromDays(1);
        private static readonly TimeSpan _noticeWaitLimit = TimeSpan.FromSeconds(5);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly RoomRegistry _roomRegistry = new RoomRegistry();
        private readonly RecordingRoomRecordRepository _roomRecordRepository = new RecordingRoomRecordRepository();
        private readonly RecordingSanctionRepository _sanctionRepository = new RecordingSanctionRepository();
        private readonly RoomService _roomService;

        public RoomServiceTests()
        {
            _roomService = new RoomService(_roomRegistry, _callerSessions.Registry, _roomRecordRepository, _sanctionRepository, new RecordingPlayerRepository(), _fakeTimeProvider);
            _callerSessions.Open(SamplePlayers.Beto);
            _callerSessions.Open(SamplePlayers.Dani);
            _callerSessions.Open(SamplePlayers.Eva);
            _callerSessions.Open(SamplePlayers.CaroGuest);
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public async Task Test_CreateRoomAsync_WithFreeCode_ReturnsCreated()
        {
            CreateRoomResult creation = await _roomService.CreateRoomAsync(RoomVisibility.Public);

            Assert.Equal(CreateRoomStatus.Created, creation.Status);
        }

        [Fact]
        public async Task Test_CreateRoomAsync_WithStoredCodeTaken_ReturnsCreated()
        {
            _roomRecordRepository.RefusedCreations = OneRefusedCode;

            CreateRoomResult creation = await _roomService.CreateRoomAsync(RoomVisibility.Public);

            Assert.Equal(CreateRoomStatus.Created, creation.Status);
        }

        [Fact]
        public async Task Test_CreateRoomAsync_WithStoredCodeTaken_DrawsAnotherCode()
        {
            _roomRecordRepository.RefusedCreations = OneRefusedCode;

            await _roomService.CreateRoomAsync(RoomVisibility.Public);

            Assert.Equal(TwoCodeDraws, _roomRecordRepository.CreationAttempts);
        }

        [Fact]
        public async Task Test_CreateRoomAsync_ByAGuest_ReturnsCreated()
        {
            _callerSessions.ActAs(SamplePlayers.CaroGuest);

            CreateRoomResult creation = await _roomService.CreateRoomAsync(RoomVisibility.Public);

            Assert.Equal(CreateRoomStatus.Created, creation.Status);
        }

        [Fact]
        public async Task Test_CreateRoomAsync_ByAPlayer_MakesItTheHost()
        {
            CreateRoomResult creation = await _roomService.CreateRoomAsync(RoomVisibility.Public);

            Assert.Equal(HostMemberId, creation.Room?.HostMemberId);
        }

        [Fact]
        public async Task Test_JoinRoomAsync_ToAnOpenRoom_ReturnsJoined()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);
            _callerSessions.ActAs(SamplePlayers.Beto);

            JoinRoomResult joining = await _roomService.JoinRoomAsync(roomId);

            Assert.Equal(JoinRoomStatus.Joined, joining.Status);
        }

        [Fact]
        public void Test_GetRooms_WithoutOpenRooms_ReturnsNoRooms()
        {
            RoomListResponse roomList = _roomService.GetRooms();

            Assert.Empty(roomList.Rooms);
        }

        [Fact]
        public async Task Test_GetRooms_WithAPublicRoom_ListsIt()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);

            RoomListResponse roomList = _roomService.GetRooms();

            Assert.Equal(new int[] { roomId }, roomList.Rooms.Select(room => room.RoomId));
        }

        [Fact]
        public void Test_FindRoom_WithUnknownCode_ReturnsNoRooms()
        {
            RoomListResponse roomList = _roomService.FindRoom(UnknownRoomCode);

            Assert.Empty(roomList.Rooms);
        }

        [Fact]
        public async Task Test_JoinRoomAsync_ToAMissingRoom_ReturnsRoomNotFound()
        {
            JoinRoomResult joining = await _roomService.JoinRoomAsync(MissingRoomId);

            Assert.Equal(JoinRoomStatus.RoomNotFound, joining.Status);
        }

        [Fact]
        public async Task Test_JoinRoomAsync_ToAFullRoom_ReturnsRoomFull()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);
            await JoinAsAsync(SamplePlayers.Beto, roomId);
            await JoinAsAsync(SamplePlayers.Dani, roomId);
            await JoinAsAsync(SamplePlayers.CaroGuest, roomId);
            _callerSessions.ActAs(SamplePlayers.Eva);

            JoinRoomResult joining = await _roomService.JoinRoomAsync(roomId);

            Assert.Equal(JoinRoomStatus.RoomFull, joining.Status);
        }

        [Fact]
        public async Task Test_JoinRoomAsync_WithTemporaryBan_ReturnsBanned()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);
            DateTime nowUtc = _fakeTimeProvider.GetUtcNow().UtcDateTime;
            _sanctionRepository.Sanctions.Add(new Sanction { PlayerId = SamplePlayers.BetoId, EffectiveFrom = nowUtc, EffectiveUntil = nowUtc + _temporaryBanLength });
            _callerSessions.ActAs(SamplePlayers.Beto);

            JoinRoomResult joining = await _roomService.JoinRoomAsync(roomId);

            Assert.Equal(JoinRoomStatus.Banned, joining.Status);
        }

        [Fact]
        public async Task Test_GetCurrentRoom_AfterJoining_ReturnsInRoom()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);
            await JoinAsAsync(SamplePlayers.Beto, roomId);

            RoomLookupResponse lookup = _roomService.GetCurrentRoom();

            Assert.Equal(RoomLookupStatus.InRoom, lookup.Status);
        }

        [Fact]
        public async Task Test_GetCurrentRoom_OfTheOnlyMember_ShowsOneMember()
        {
            await CreateRoomAsAsync(SamplePlayers.Ana);

            Room currentRoom = ReadCurrentRoom();

            Assert.Single(currentRoom.Members);
        }

        [Fact]
        public async Task Test_GetCurrentRoom_WithoutFinishedMatches_ShowsAnEmptyRanking()
        {
            await CreateRoomAsAsync(SamplePlayers.Ana);

            Room currentRoom = ReadCurrentRoom();

            Assert.Empty(currentRoom.Ranking);
        }

        [Fact]
        public async Task Test_GetCurrentRoom_AfterAMemberLosesItsSession_ShowsItInactive()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);
            await JoinAsAsync(SamplePlayers.Beto, roomId);
            _callerSessions.Lose(SamplePlayers.Beto);
            _callerSessions.ActAs(SamplePlayers.Ana);

            Room currentRoom = ReadCurrentRoom();

            Assert.False(currentRoom.Members[1].IsActive);
        }

        [Fact]
        public async Task Test_LeaveRoomAsync_AsAMember_ReturnsLeft()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);
            await JoinAsAsync(SamplePlayers.Beto, roomId);

            LeaveRoomResult leaveResult = await _roomService.LeaveRoomAsync();

            Assert.Equal(LeaveRoomResult.Left, leaveResult);
        }

        [Fact]
        public async Task Test_LeaveRoomAsync_AsTheLastMember_ClosesTheRoomRecord()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);

            await _roomService.LeaveRoomAsync();

            Assert.Equal(new int[] { roomId }, _roomRecordRepository.ClosedRoomIds);
        }

        [Fact]
        public async Task Test_LeaveRoomAsync_AsTheHost_PassesTheHostToTheNextMember()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);
            await JoinAsAsync(SamplePlayers.Beto, roomId);
            _callerSessions.ActAs(SamplePlayers.Ana);

            await _roomService.LeaveRoomAsync();

            Room betoRoom = ReadCurrentRoomOf(SamplePlayers.Beto);
            Assert.Equal(SecondMemberId, betoRoom.HostMemberId);
        }

        [Fact]
        public async Task Test_LeaveRoomAsync_LeavingOneMember_KeepsTheRoomForTheOther()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);
            await JoinAsAsync(SamplePlayers.Beto, roomId);

            await _roomService.LeaveRoomAsync();

            RoomLookupStatus anaRoomStatus = ReadRoomStatusOf(SamplePlayers.Ana);
            Assert.Equal(RoomLookupStatus.InRoom, anaRoomStatus);
        }

        [Fact]
        public async Task Test_KickMember_AsTheHost_ReturnsKicked()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);
            await JoinAsAsync(SamplePlayers.Beto, roomId);
            _callerSessions.ActAs(SamplePlayers.Ana);

            KickMemberResult kickResult = _roomService.KickMember(SecondMemberId);

            Assert.Equal(KickMemberResult.Kicked, kickResult);
        }

        [Fact]
        public async Task Test_KickMember_AsTheHost_TellsTheKickedMember()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);
            await JoinAsAsync(SamplePlayers.Beto, roomId);
            _callerSessions.ActAs(SamplePlayers.Ana);

            _roomService.KickMember(SecondMemberId);

            Room kickedView = await _callerSessions.FindReceiverOf(SamplePlayers.Beto).WaitForRoomChangeAsync(RoomChangeReason.YouWereKicked).WaitAsync(_noticeWaitLimit);
            Assert.Equal(roomId, kickedView.RoomId);
        }

        [Fact]
        public async Task Test_KickMember_OfItself_ReturnsCannotKickSelf()
        {
            await CreateRoomAsAsync(SamplePlayers.Ana);

            KickMemberResult kickResult = _roomService.KickMember(HostMemberId);

            Assert.Equal(KickMemberResult.CannotKickSelf, kickResult);
        }

        [Fact]
        public async Task Test_KickMember_DuringAMatch_ReturnsRoomInMatch()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);
            await JoinAsAsync(SamplePlayers.Beto, roomId);
            _roomRegistry.FindById(roomId)?.StartMatch(SamplePlayers.AnaId);
            _callerSessions.ActAs(SamplePlayers.Ana);

            KickMemberResult kickResult = _roomService.KickMember(SecondMemberId);

            Assert.Equal(KickMemberResult.RoomInMatch, kickResult);
        }

        [Fact]
        public async Task Test_KickMember_OfAnInactiveMember_ReturnsKicked()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);
            await JoinAsAsync(SamplePlayers.Beto, roomId);
            _callerSessions.Lose(SamplePlayers.Beto);
            _callerSessions.ActAs(SamplePlayers.Ana);

            KickMemberResult kickResult = _roomService.KickMember(SecondMemberId);

            Assert.Equal(KickMemberResult.Kicked, kickResult);
        }

        [Fact]
        public async Task Test_KickMember_AfterLosingTheHost_ReturnsNotHost()
        {
            int roomId = await CreateRoomAsAsync(SamplePlayers.Ana);
            await JoinAsAsync(SamplePlayers.Beto, roomId);
            _callerSessions.Lose(SamplePlayers.Ana);
            _callerSessions.Open(SamplePlayers.Ana);

            KickMemberResult kickResult = _roomService.KickMember(SecondMemberId);

            Assert.Equal(KickMemberResult.NotHost, kickResult);
        }

        private async Task<int> CreateRoomAsAsync(SessionOwner host)
        {
            _callerSessions.ActAs(host);
            CreateRoomResult creation = await _roomService.CreateRoomAsync(RoomVisibility.Public);
            int roomId = creation.Room?.RoomId ?? NoRoomId;

            return roomId;
        }

        private Room ReadCurrentRoom()
        {
            Room currentRoom = _roomService.GetCurrentRoom().Room ?? throw new InvalidOperationException(NotInRoomMessage);

            return currentRoom;
        }

        private Room ReadCurrentRoomOf(SessionOwner member)
        {
            _callerSessions.ActAs(member);
            Room currentRoom = ReadCurrentRoom();

            return currentRoom;
        }

        private RoomLookupStatus ReadRoomStatusOf(SessionOwner member)
        {
            _callerSessions.ActAs(member);
            RoomLookupStatus roomStatus = _roomService.GetCurrentRoom().Status;

            return roomStatus;
        }

        private async Task JoinAsAsync(SessionOwner member, int roomId)
        {
            _callerSessions.ActAs(member);
            await _roomService.JoinRoomAsync(roomId);
        }
    }
}
