using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class InvitePlayersFlowTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly InvitePlayersFlow _invitePlayersFlow;

        public InvitePlayersFlowTests()
        {
            _invitePlayersFlow = new InvitePlayersFlow(_gameSessionConnection, SampleClients.CreateSessionOfAna(), SampleClients.RefreshPeriod);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheInvitationFails_ShowsTheServerError()
        {
            _gameSessionConnection.SendInvitationReply = SampleClients.CreateFailedCallAsync<SendInvitationResult>();
            _invitePlayersFlow.Invite(SampleClients.BetoId, false);

            InvitePlayersOutcome? outcome = _invitePlayersFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.ErrorKey);
        }

        [Fact]
        public void Test_FindScreenToGoBack_FromTheInvitations_ReturnsRoom()
        {
            ScreenId nextScreen = InvitePlayersFlow.FindScreenToGoBack();

            Assert.Equal(ScreenId.Room, nextScreen);
        }
    }
}
