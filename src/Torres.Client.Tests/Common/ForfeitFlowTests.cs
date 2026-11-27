using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class ForfeitFlowTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly ForfeitFlow _forfeitFlow;

        public ForfeitFlowTests()
        {
            _forfeitFlow = new ForfeitFlow(_gameSessionConnection, SampleClients.CreateSessionOfAna());
        }

        [Fact]
        public void Test_TakeOutcome_AfterForfeitingWithoutARoom_GoesToTheMainMenu()
        {
            _gameSessionConnection.ForfeitReply = Task.FromResult(ForfeitResult.Forfeited);
            _forfeitFlow.Forfeit();

            ScreenOutcome? outcome = _forfeitFlow.TakeOutcome();

            Assert.Equal(ScreenId.MainMenu, outcome?.NextScreen);
        }

        [Fact]
        public void Test_TakeOutcome_WhenForfeitingFails_ShowsTheServerError()
        {
            _gameSessionConnection.ForfeitReply = SampleClients.CreateFailedCallAsync<ForfeitResult>();
            _forfeitFlow.Forfeit();

            ScreenOutcome? outcome = _forfeitFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }
    }
}
