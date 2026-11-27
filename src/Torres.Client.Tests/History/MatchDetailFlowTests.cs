using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class MatchDetailFlowTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly MatchDetailFlow _matchDetailFlow;

        public MatchDetailFlowTests()
        {
            PlayerSession playerSession = SampleClients.CreateSessionOfAna();
            playerSession.MatchHistory.Select(SampleClients.MatchId, SampleClients.FirstPage);
            _matchDetailFlow = new MatchDetailFlow(_gameSessionConnection, playerSession);
        }

        [Fact]
        public void Test_TakeOutcome_WithServerUnreachable_ReturnsNoDetail()
        {
            _gameSessionConnection.GetMatchDetailReply = SampleClients.CreateFailedCallAsync<MatchDetailResponse>();
            _matchDetailFlow.Open();

            MatchDetailOutcome? outcome = _matchDetailFlow.TakeOutcome();

            Assert.Equal(new MatchDetailOutcome(null), outcome);
        }
    }
}
