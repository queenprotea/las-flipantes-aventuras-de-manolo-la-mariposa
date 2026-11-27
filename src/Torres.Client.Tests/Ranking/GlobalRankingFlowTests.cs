using Game.Contracts;

using Torres.Client.Flows;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class GlobalRankingFlowTests
    {
        private readonly RecordingRankingConnection _rankingConnection = new RecordingRankingConnection();
        private readonly GlobalRankingFlow _globalRankingFlow;

        public GlobalRankingFlowTests()
        {
            _globalRankingFlow = new GlobalRankingFlow(_rankingConnection, SampleClients.CreateSessionOfAna());
        }

        [Fact]
        public void Test_IsCurrentPlayer_WithAnotherName_ReturnsFalse()
        {
            bool isCurrentPlayer = _globalRankingFlow.IsCurrentPlayer(SampleClients.BetoUsername);

            Assert.False(isCurrentPlayer);
        }

        [Fact]
        public void Test_IsCurrentPlayer_WithOwnName_ReturnsTrue()
        {
            bool isCurrentPlayer = _globalRankingFlow.IsCurrentPlayer(SampleClients.AnaUsername);

            Assert.True(isCurrentPlayer);
        }

        [Fact]
        public void Test_TakeOutcome_WithServerUnreachable_ReturnsNoEntries()
        {
            _rankingConnection.GetGlobalRankingReply = SampleClients.CreateFailedCallAsync<GlobalRankingResponse>();

            GlobalRankingOutcome? outcome = _globalRankingFlow.TakeOutcome();

            Assert.Equal(new GlobalRankingOutcome(null), outcome);
        }
    }
}
