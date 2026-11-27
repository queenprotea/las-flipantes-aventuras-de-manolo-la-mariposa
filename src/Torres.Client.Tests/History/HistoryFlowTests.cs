using System.Linq;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class HistoryFlowTests
    {
        private const string GetMatchHistoryCall = nameof(RecordingGameSessionConnection.GetMatchHistoryAsync);
        private const int TwoRequests = 2;

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly HistoryFlow _historyFlow;

        public HistoryFlowTests()
        {
            _historyFlow = new HistoryFlow(_gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_SelectMatch_AfterTheHistoryLoaded_ReturnsTrue()
        {
            _historyFlow.Open();
            _historyFlow.TakeOutcome();

            bool isSelected = _historyFlow.SelectMatch(SampleClients.MatchId);

            Assert.True(isSelected);
        }

        [Fact]
        public void Test_SelectMatch_AfterTheHistoryLoaded_RemembersTheMatch()
        {
            _historyFlow.Open();
            _historyFlow.TakeOutcome();

            _historyFlow.SelectMatch(SampleClients.MatchId);

            Assert.Equal(SampleClients.MatchId, _playerSession.MatchHistory.SelectedMatchId);
        }

        [Fact]
        public void Test_TakeOutcome_WithServerUnreachable_ReturnsNoPage()
        {
            _gameSessionConnection.GetMatchHistoryReply = SampleClients.CreateFailedCallAsync<MatchHistoryResponse>();
            _historyFlow.Open();

            HistoryOutcome? outcome = _historyFlow.TakeOutcome();

            Assert.Equal(new HistoryOutcome(null), outcome);
        }

        [Fact]
        public void Test_Retry_AfterAFailure_AsksForThePageAgain()
        {
            _gameSessionConnection.GetMatchHistoryReply = SampleClients.CreateFailedCallAsync<MatchHistoryResponse>();
            _historyFlow.Open();
            _historyFlow.TakeOutcome();

            _historyFlow.Retry();

            int historyRequestCount = _gameSessionConnection.CalledMethods.Count(calledMethod => calledMethod == GetMatchHistoryCall);
            Assert.Equal(TwoRequests, historyRequestCount);
        }
    }
}
