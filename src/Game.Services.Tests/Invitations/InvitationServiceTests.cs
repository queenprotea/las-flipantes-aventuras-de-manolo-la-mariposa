using System;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class InvitationServiceTests
    {
        private const int NoRoomId = 0;

        private static readonly TimeSpan _noticeWaitLimit = TimeSpan.FromSeconds(5);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly RecordingRoomInvitationRepository _invitationRepository = new RecordingRoomInvitationRepository();
        private readonly RecordingEmailSender _emailSender = new RecordingEmailSender();
        private readonly RoomService _roomService;
        private readonly InvitationService _invitationService;

        public InvitationServiceTests()
        {
            var roomRegistry = new RoomRegistry();
            var playerRepository = new RecordingPlayerRepository();
            _roomService = new RoomService(roomRegistry, _callerSessions.Registry, new RecordingRoomRecordRepository(), new RecordingSanctionRepository(), playerRepository, _fakeTimeProvider);
            _invitationService = new InvitationService(_callerSessions.Registry, roomRegistry, _roomService, _invitationRepository, playerRepository, _emailSender, _fakeTimeProvider);
            _callerSessions.Open(SamplePlayers.Beto);
            _callerSessions.Open(SamplePlayers.Dani);
            _callerSessions.Open(SamplePlayers.Eva);
            _callerSessions.Open(SamplePlayers.CaroGuest);
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public async Task Test_SendInvitationAsync_ToAPlayer_ReturnsSent()
        {
            await CreateRoomAsAnaAsync();

            SendInvitationResult sendResult = await _invitationService.SendInvitationAsync(SamplePlayers.BetoId);

            Assert.Equal(SendInvitationResult.Sent, sendResult);
        }

        [Fact]
        public async Task Test_SendInvitationAsync_ToAConnectedPlayer_DeliversTheInvitation()
        {
            int roomId = await CreateRoomAsAnaAsync();

            await _invitationService.SendInvitationAsync(SamplePlayers.BetoId);

            RoomInvitation deliveredInvitation = await _callerSessions.FindReceiverOf(SamplePlayers.Beto).FirstInvitation.WaitAsync(_noticeWaitLimit);
            Assert.Equal(roomId, deliveredInvitation.RoomId);
        }

        [Fact]
        public async Task Test_SendInvitationAsync_ToAPlayerWhoIsNotAFriend_ReturnsSent()
        {
            await CreateRoomAsAnaAsync();

            SendInvitationResult sendResult = await _invitationService.SendInvitationAsync(SamplePlayers.DaniId);

            Assert.Equal(SendInvitationResult.Sent, sendResult);
        }

        [Fact]
        public async Task Test_SendInvitationAsync_Twice_ReturnsInvitationAlreadyPending()
        {
            await CreateRoomAsAnaAsync();
            await _invitationService.SendInvitationAsync(SamplePlayers.BetoId);

            SendInvitationResult secondResult = await _invitationService.SendInvitationAsync(SamplePlayers.BetoId);

            Assert.Equal(SendInvitationResult.InvitationAlreadyPending, secondResult);
        }

        [Fact]
        public async Task Test_SendInvitationAsync_ToAMissingAccount_ReturnsPlayerNotFound()
        {
            await CreateRoomAsAnaAsync();
            _invitationRepository.MissingPlayerIds.Add(SamplePlayers.UnknownPlayerId);

            SendInvitationResult sendResult = await _invitationService.SendInvitationAsync(SamplePlayers.UnknownPlayerId);

            Assert.Equal(SendInvitationResult.PlayerNotFound, sendResult);
        }

        [Fact]
        public async Task Test_SendInvitationByEmailAsync_ToAPlayer_ReturnsSent()
        {
            await CreateRoomAsAnaAsync();

            SendInvitationResult sendResult = await _invitationService.SendInvitationByEmailAsync(SamplePlayers.BetoId);

            Assert.Equal(SendInvitationResult.Sent, sendResult);
        }

        [Fact]
        public async Task Test_SendInvitationByEmailAsync_ToAPlayer_EmailsTheInvitee()
        {
            await CreateRoomAsAnaAsync();

            await _invitationService.SendInvitationByEmailAsync(SamplePlayers.BetoId);

            Assert.Equal(new string[] { SamplePlayers.BetoEmail }, _emailSender.SentMessages.Select(sentMessage => sentMessage.RecipientAddress));
        }

        [Fact]
        public async Task Test_SendInvitationByEmailAsync_WithFailedEmail_ReturnsEmailNotSent()
        {
            await CreateRoomAsAnaAsync();
            _emailSender.IsDelivering = false;

            SendInvitationResult sendResult = await _invitationService.SendInvitationByEmailAsync(SamplePlayers.BetoId);

            Assert.Equal(SendInvitationResult.EmailNotSent, sendResult);
        }

        [Fact]
        public async Task Test_RespondInvitationAsync_Accepting_ReturnsAccepted()
        {
            int roomId = await InviteBetoAsync();

            RespondInvitationResult answer = await _invitationService.RespondInvitationAsync(roomId, true);

            Assert.Equal(RespondInvitationStatus.Accepted, answer.Status);
        }

        [Fact]
        public async Task Test_RespondInvitationAsync_Accepting_PutsTheInviteeInTheRoom()
        {
            int roomId = await InviteBetoAsync();

            await _invitationService.RespondInvitationAsync(roomId, true);

            Assert.Equal(RoomLookupStatus.InRoom, _roomService.GetCurrentRoom().Status);
        }

        [Fact]
        public async Task Test_GetPendingInvitationsAsync_WithoutInvitations_ReturnsNone()
        {
            PendingInvitationsResponse pendingInvitations = await _invitationService.GetPendingInvitationsAsync();

            Assert.Empty(pendingInvitations.Invitations);
        }

        [Fact]
        public async Task Test_GetPendingInvitationsAsync_WithAnInvitation_ListsItsRoom()
        {
            int roomId = await InviteBetoAsync();

            PendingInvitationsResponse pendingInvitations = await _invitationService.GetPendingInvitationsAsync();

            Assert.Equal(new int[] { roomId }, pendingInvitations.Invitations.Select(invitation => invitation.RoomId));
        }

        [Fact]
        public async Task Test_RespondInvitationAsync_Rejecting_ReturnsRejected()
        {
            int roomId = await InviteBetoAsync();

            RespondInvitationResult answer = await _invitationService.RespondInvitationAsync(roomId, false);

            Assert.Equal(RespondInvitationStatus.Rejected, answer.Status);
        }

        [Fact]
        public async Task Test_RespondInvitationAsync_ToAClosedRoom_ReturnsInvitationNoLongerExists()
        {
            int roomId = await InviteBetoAsync();
            _callerSessions.ActAs(SamplePlayers.Ana);
            await _roomService.LeaveRoomAsync();
            _callerSessions.ActAs(SamplePlayers.Beto);

            RespondInvitationResult answer = await _invitationService.RespondInvitationAsync(roomId, true);

            Assert.Equal(RespondInvitationStatus.InvitationNoLongerExists, answer.Status);
        }

        [Fact]
        public async Task Test_RespondInvitationAsync_AfterTheRoomFilledUp_ReturnsRoomFull()
        {
            int roomId = await CreateRoomAsAnaAsync();
            await _invitationService.SendInvitationAsync(SamplePlayers.EvaId);
            await JoinAsAsync(SamplePlayers.Beto, roomId);
            await JoinAsAsync(SamplePlayers.Dani, roomId);
            await JoinAsAsync(SamplePlayers.CaroGuest, roomId);
            _callerSessions.ActAs(SamplePlayers.Eva);

            RespondInvitationResult answer = await _invitationService.RespondInvitationAsync(roomId, true);

            Assert.Equal(RespondInvitationStatus.RoomFull, answer.Status);
        }

        private async Task<int> CreateRoomAsAnaAsync()
        {
            _callerSessions.ActAs(SamplePlayers.Ana);
            CreateRoomResult creation = await _roomService.CreateRoomAsync(RoomVisibility.Private);
            int roomId = creation.Room?.RoomId ?? NoRoomId;

            return roomId;
        }

        private async Task<int> InviteBetoAsync()
        {
            int roomId = await CreateRoomAsAnaAsync();
            await _invitationService.SendInvitationAsync(SamplePlayers.BetoId);
            _callerSessions.ActAs(SamplePlayers.Beto);

            return roomId;
        }

        private async Task JoinAsAsync(SessionOwner member, int roomId)
        {
            _callerSessions.ActAs(member);
            await _roomService.JoinRoomAsync(roomId);
        }
    }
}
