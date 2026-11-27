using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class InvitationsFlowAnswerTests
    {
        private const string GetPendingInvitationsCall = nameof(RecordingGameSessionConnection.GetPendingInvitationsAsync);

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly RoomInvitation _invitation = new RoomInvitation { RoomId = SampleClients.RoomId, RoomCode = SampleClients.RoomCode };
        private readonly InvitationsFlow _invitationsFlow;

        public InvitationsFlowAnswerTests()
        {
            _playerSession.Invitations.Add(_invitation);
            _invitationsFlow = new InvitationsFlow(_gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_Open_AsAGuest_RedirectsToTheLogin()
        {
            var guestSession = new PlayerSession();
            guestSession.StartAsGuest(SampleClients.GuestAlias);
            var guestFlow = new InvitationsFlow(_gameSessionConnection, guestSession);

            ScreenId? redirect = guestFlow.Open();

            Assert.Equal(ScreenId.Login, redirect);
        }

        [Fact]
        public void Test_Open_InsideARoom_RedirectsToTheRoom()
        {
            _playerSession.EnterRoom(SampleClients.CreateRoom());

            ScreenId? redirect = _invitationsFlow.Open();

            Assert.Equal(ScreenId.Room, redirect);
        }

        [Fact]
        public void Test_Open_OutsideARoom_AsksForTheInvitations()
        {
            _invitationsFlow.Open();

            Assert.Contains(GetPendingInvitationsCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheInvitationsArrive_LoadsThem()
        {
            _gameSessionConnection.GetPendingInvitationsReply = Task.FromResult(new PendingInvitationsResponse { Invitations = new List<RoomInvitation>() });
            _invitationsFlow.Open();

            _invitationsFlow.TakeOutcome();

            Assert.Empty(_invitationsFlow.Invitations);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheInvitationsFail_ShowsTheServerError()
        {
            _gameSessionConnection.GetPendingInvitationsReply = SampleClients.CreateFailedCallAsync<PendingInvitationsResponse>();
            _invitationsFlow.Open();

            InvitationsOutcome? outcome = _invitationsFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        [Fact]
        public void Test_Accept_WithTheSessionClosed_ReturnsFalse()
        {
            _gameSessionConnection.IsSessionOpen = false;

            bool canAnswer = _invitationsFlow.Accept(SampleClients.RoomId);

            Assert.False(canAnswer);
        }

        [Fact]
        public void Test_TakeOutcome_AfterRejecting_RemovesTheInvitation()
        {
            AnswerWith(new RespondInvitationResult { Status = RespondInvitationStatus.Rejected });

            _invitationsFlow.TakeOutcome();

            Assert.Empty(_invitationsFlow.Invitations);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheRoomIsGone_ShowsTheRoomGoneNotice()
        {
            AnswerWith(new RespondInvitationResult { Status = RespondInvitationStatus.InvitationNoLongerExists });

            InvitationsOutcome? outcome = _invitationsFlow.TakeOutcome();

            Assert.Equal(TextKeys.Alerts.InvitationRoomGoneTitle, outcome?.Notice?.TitleKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheRoomIsGone_RemovesTheInvitation()
        {
            AnswerWith(new RespondInvitationResult { Status = RespondInvitationStatus.InvitationNoLongerExists });

            _invitationsFlow.TakeOutcome();

            Assert.Empty(_invitationsFlow.Invitations);
        }

        [Fact]
        public void Test_TakeOutcome_DuringAMatchOfTheRoom_ShowsRoomInMatch()
        {
            AnswerWith(new RespondInvitationResult { Status = RespondInvitationStatus.RoomInMatch });

            InvitationsOutcome? outcome = _invitationsFlow.TakeOutcome();

            Assert.Equal(TextKeys.Rooms.RoomInMatchError, outcome?.Notice?.TitleKey);
        }

        [Fact]
        public void Test_TakeOutcome_WithABannedPlayer_ShowsTheBan()
        {
            AnswerWith(new RespondInvitationResult { Status = RespondInvitationStatus.Banned });

            InvitationsOutcome? outcome = _invitationsFlow.TakeOutcome();

            bool? isBan = outcome?.Notice?.IsBan;
            Assert.True(isBan);
        }

        [Fact]
        public void Test_TakeOutcome_AlreadyInsideARoom_ShowsAlreadyInRoom()
        {
            AnswerWith(new RespondInvitationResult { Status = RespondInvitationStatus.AlreadyInRoom });

            InvitationsOutcome? outcome = _invitationsFlow.TakeOutcome();

            Assert.Equal(TextKeys.Rooms.AlreadyInRoomError, outcome?.Notice?.TitleKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheAnswerFindsTheDatabaseUnavailable_ShowsTheServerError()
        {
            AnswerWith(new RespondInvitationResult { Status = RespondInvitationStatus.DatabaseUnavailable });

            InvitationsOutcome? outcome = _invitationsFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheAcceptanceComesWithoutRoom_ShowsTheServerError()
        {
            _gameSessionConnection.AcceptInvitationReply = Task.FromResult(new RespondInvitationResult { Status = RespondInvitationStatus.Accepted });
            _invitationsFlow.Accept(SampleClients.RoomId);

            InvitationsOutcome? outcome = _invitationsFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        [Fact]
        public void Test_Revision_AfterAddingAnInvitation_ChangesFromTheStart()
        {
            int revision = _invitationsFlow.Revision;

            Assert.NotEqual(default, revision);
        }

        private void AnswerWith(RespondInvitationResult answer)
        {
            _gameSessionConnection.RejectInvitationReply = Task.FromResult(answer);
            _invitationsFlow.Reject(SampleClients.RoomId);
        }
    }
}
