using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class MatchInProgressFlowTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly MatchInProgressFlow _matchInProgressFlow;

        public MatchInProgressFlowTests()
        {
            _matchInProgressFlow = new MatchInProgressFlow(_gameSessionConnection, SampleClients.CreateSessionOfAna());
        }

        [Fact]
        public void Test_TakeOutcome_AfterAbandoning_GoesToTheMainMenu()
        {
            _gameSessionConnection.ForfeitReply = Task.FromResult(ForfeitResult.Forfeited);
            _matchInProgressFlow.Abandon();

            ScreenOutcome? outcome = _matchInProgressFlow.TakeOutcome();

            Assert.Equal(ScreenId.MainMenu, outcome?.NextScreen);
        }
    }
}
