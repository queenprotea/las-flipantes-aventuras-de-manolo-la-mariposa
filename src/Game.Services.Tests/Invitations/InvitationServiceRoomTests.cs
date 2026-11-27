using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class InvitationServiceRoomTests
    {
        private const int NoRoomId = 0;

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly RoomService _roomService;
        private readonly InvitationService _invitationService;

        public InvitationServiceRoomTests()
        {
            var roomRegistry = new RoomRegistry();
            var playerRepository = new RecordingPlayerRepository();
            _roomService = new RoomService(roomRegistry, _callerSessions.Registry, new RecordingRoomRecordRepository(), new RecordingSanctionRepository(), playerRepository, _fakeTimeProvider);
            _invitationService = new InvitationService(_callerSessions.Registry, roomRegistry, _roomService, new RecordingRoomInvitationRepository(), playerRepository, new RecordingEmailSender(), _fakeTimeProvider);
            _callerSessions.Open(SamplePlayers.Beto);
            _callerSessions.Open(SamplePlayers.CaroGuest);
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public async Task Test_SendInvitationAsync_WithoutRoom_ReturnsNotInRoom()
        {
            SendInvitationResult sendResult = await _invitationService.SendInvitationAsync(SamplePlayers.BetoId);

            Assert.Equal(SendInvitationResult.NotInRoom, sendResult);
        }

        [Fact]
        public async Task Test_SendInvitationByEmailAsync_WithoutRoom_ReturnsNotInRoom()
        {
            SendInvitationResult sendResult = await _invitationService.SendInvitationByEmailAsync(SamplePlayers.BetoId);

            Assert.Equal(SendInvitationResult.NotInRoom, sendResult);
        }

        [Fact]
        public async Task Test_SendInvitationAsync_ToItself_ReturnsPlayerNotFound()
        {
            await CreateRoomAsAnaAsync();

            SendInvitationResult sendResult = await _invitationService.SendInvitationAsync(SamplePlayers.AnaId);

            Assert.Equal(SendInvitationResult.PlayerNotFound, sendResult);
        }

        [Fact]
        public async Task Test_SendInvitationAsync_ToAMember_ReturnsAlreadyInThisRoom()
        {
            int roomId = await CreateRoomAsAnaAsync();
            _callerSessions.ActAs(SamplePlayers.Beto);
            await _roomService.JoinRoomAsync(roomId);
            _callerSessions.ActAs(SamplePlayers.Ana);

            SendInvitationResult sendResult = await _invitationService.SendInvitationAsync(SamplePlayers.BetoId);

            Assert.Equal(SendInvitationResult.AlreadyInThisRoom, sendResult);
        }

        [Fact]
        public async Task Test_GetRoomInviteesAsync_WithoutRoom_ReturnsNone()
        {
            RoomInviteesResponse invitees = await _invitationService.GetRoomInviteesAsync();

            Assert.Empty(invitees.InvitedPlayerIds);
        }

        [Fact]
        public async Task Test_GetRoomInviteesAsync_AfterAnInvitation_ListsTheInvitee()
        {
            await CreateRoomAsAnaAsync();
            await _invitationService.SendInvitationAsync(SamplePlayers.BetoId);

            RoomInviteesResponse invitees = await _invitationService.GetRoomInviteesAsync();

            Assert.Contains(SamplePlayers.BetoId, invitees.InvitedPlayerIds);
        }

        [Fact]
        public async Task Test_RespondInvitationAsync_WithoutRoomId_ReturnsInvitationNoLongerExists()
        {
            RespondInvitationResult answer = await _invitationService.RespondInvitationAsync(NoRoomId, true);

            Assert.Equal(RespondInvitationStatus.InvitationNoLongerExists, answer.Status);
        }

        [Fact]
        public async Task Test_GetPendingInvitationsAsync_AsAGuest_ThrowsFaultException()
        {
            _callerSessions.ActAs(SamplePlayers.CaroGuest);

            await Assert.ThrowsAsync<FaultException>(() => _invitationService.GetPendingInvitationsAsync());
        }

        private async Task<int> CreateRoomAsAnaAsync()
        {
            _callerSessions.ActAs(SamplePlayers.Ana);
            CreateRoomResult creation = await _roomService.CreateRoomAsync(RoomVisibility.Private);
            int roomId = creation.Room?.RoomId ?? NoRoomId;

            return roomId;
        }
    }
}
