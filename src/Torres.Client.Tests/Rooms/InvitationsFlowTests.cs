using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class InvitationsFlowTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly InvitationsFlow _invitationsFlow;

        public InvitationsFlowTests()
        {
            _invitationsFlow = new InvitationsFlow(_gameSessionConnection, SampleClients.CreateSessionOfAna());
        }

        [Fact]
        public void Test_TakeOutcome_AfterAccepting_GoesToTheRoom()
        {
            _gameSessionConnection.AcceptInvitationReply = Task.FromResult(new RespondInvitationResult { Status = RespondInvitationStatus.Accepted, Room = SampleClients.CreateRoom() });
            _invitationsFlow.Accept(SampleClients.RoomId);

            InvitationsOutcome? outcome = _invitationsFlow.TakeOutcome();

            Assert.Equal(ScreenId.Room, outcome?.NextScreen);
        }

        [Fact]
        public void Test_TakeOutcome_ToAFullRoom_AsksWhatToDo()
        {
            _gameSessionConnection.AcceptInvitationReply = Task.FromResult(new RespondInvitationResult { Status = RespondInvitationStatus.RoomFull });
            _invitationsFlow.Accept(SampleClients.RoomId);

            InvitationsOutcome? outcome = _invitationsFlow.TakeOutcome();

            Assert.True(outcome?.AsksRoomFullChoice);
        }

        [Fact]
        public void Test_TakeOutcome_WhenAcceptingFails_ShowsTheServerError()
        {
            _gameSessionConnection.AcceptInvitationReply = SampleClients.CreateFailedCallAsync<RespondInvitationResult>();
            _invitationsFlow.Accept(SampleClients.RoomId);

            InvitationsOutcome? outcome = _invitationsFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        [Fact]
        public void Test_FindScreenToGoBack_FromTheInvitations_ReturnsMainMenu()
        {
            ScreenId nextScreen = InvitationsFlow.FindScreenToGoBack();

            Assert.Equal(ScreenId.MainMenu, nextScreen);
        }
    }
}
