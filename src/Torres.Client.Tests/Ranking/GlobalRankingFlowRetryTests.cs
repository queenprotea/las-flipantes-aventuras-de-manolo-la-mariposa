using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class GlobalRankingFlowRetryTests
    {
        private const string GetGlobalRankingCall = nameof(RecordingRankingConnection.GetGlobalRankingAsync);
        private const int TwoRequests = 2;

        private readonly RecordingRankingConnection _rankingConnection = new RecordingRankingConnection();
        private readonly GlobalRankingFlow _rankingFlow;

        public GlobalRankingFlowRetryTests()
        {
            _rankingFlow = new GlobalRankingFlow(_rankingConnection, SampleClients.CreateSessionOfAna());
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheRankingArrives_ShowsItsEntries()
        {
            var entries = new List<RankingEntry>();
            _rankingConnection.GetGlobalRankingReply = Task.FromResult(new GlobalRankingResponse { Entries = entries });

            GlobalRankingOutcome? outcome = _rankingFlow.TakeOutcome();

            Assert.Same(entries, outcome?.Entries);
        }

        [Fact]
        public void Test_Retry_AfterAFailure_AsksForTheRankingAgain()
        {
            _rankingConnection.GetGlobalRankingReply = SampleClients.CreateFailedCallAsync<GlobalRankingResponse>();
            _rankingFlow.TakeOutcome();
            _rankingFlow.Retry();

            _rankingFlow.TakeOutcome();

            int rankingRequests = _rankingConnection.CalledMethods.Count(calledMethod => calledMethod == GetGlobalRankingCall);
            Assert.Equal(TwoRequests, rankingRequests);
        }
    }
}
