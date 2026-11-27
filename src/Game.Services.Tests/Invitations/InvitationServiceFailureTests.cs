using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class InvitationServiceFailureTests
    {
        private const int NoRoomId = 0;

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly RoomService _roomService;
        private readonly InvitationService _invitationService;

        public InvitationServiceFailureTests()
        {
            var roomRegistry = new RoomRegistry();
            var playerRepository = new RecordingPlayerRepository();
            _roomService = new RoomService(roomRegistry, _callerSessions.Registry, new RecordingRoomRecordRepository(), new RecordingSanctionRepository(), playerRepository, _fakeTimeProvider);
            _invitationService = new InvitationService(_callerSessions.Registry, roomRegistry, _roomService, new FailingRoomInvitationRepository(), playerRepository, new RecordingEmailSender(), _fakeTimeProvider);
            _callerSessions.Open(SamplePlayers.Beto);
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public async Task Test_SendInvitationAsync_WithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            await CreateRoomAsAnaAsync();

            SendInvitationResult sendResult = await _invitationService.SendInvitationAsync(SamplePlayers.BetoId);

            Assert.Equal(SendInvitationResult.DatabaseUnavailable, sendResult);
        }

        [Fact]
        public async Task Test_GetPendingInvitationsAsync_WithTheDatabaseDown_ThrowsFaultException()
        {
            await Assert.ThrowsAsync<FaultException>(() => _invitationService.GetPendingInvitationsAsync());
        }

        [Fact]
        public async Task Test_GetRoomInviteesAsync_WithTheDatabaseDown_ThrowsFaultException()
        {
            await CreateRoomAsAnaAsync();

            await Assert.ThrowsAsync<FaultException>(() => _invitationService.GetRoomInviteesAsync());
        }

        [Fact]
        public async Task Test_RespondInvitationAsync_WithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            int roomId = await CreateRoomAsAnaAsync();
            _callerSessions.ActAs(SamplePlayers.Beto);

            RespondInvitationResult answer = await _invitationService.RespondInvitationAsync(roomId, false);

            Assert.Equal(RespondInvitationStatus.DatabaseUnavailable, answer.Status);
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
