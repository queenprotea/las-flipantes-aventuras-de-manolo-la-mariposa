using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class MatchFlowTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly MatchFlow _matchFlow;

        public MatchFlowTests()
        {
            _matchFlow = new MatchFlow(_gameSessionConnection, SampleClients.CreateSessionOfAna());
        }

        [Fact]
        public void Test_TakeOutcome_WhenEndingTheTurnFails_ShowsTheServerError()
        {
            _gameSessionConnection.EndTurnReply = SampleClients.CreateFailedCallAsync<MatchActionResult>();
            _matchFlow.EndTurn();

            MatchActionOutcome? outcome = _matchFlow.TakeOutcome();

            Assert.Equal(Notice.CreateServerError(), outcome?.Notice);
        }

        [Fact]
        public void Test_TakeOutcome_WithNotEnoughPoints_ShowsItsNotice()
        {
            _gameSessionConnection.DrawCardReply = Task.FromResult(MatchActionResult.NotEnoughActionPoints);
            _matchFlow.DrawCard();

            MatchActionOutcome? outcome = _matchFlow.TakeOutcome();

            Assert.Equal(TextKeys.Match.NotEnoughPointsNotice, outcome?.Notice?.TitleKey);
        }
    }
}
