using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class MatchSummaryFlowTests
    {
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly MatchSummaryFlow _summaryFlow;

        public MatchSummaryFlowTests()
        {
            _playerSession.ApplyMatchChange(SampleClients.CreatePlacementChange());
            _summaryFlow = new MatchSummaryFlow(_playerSession);
        }

        [Fact]
        public void Test_LeaveMatch_WithoutRoom_GoesToTheMainMenu()
        {
            ScreenId nextScreen = _summaryFlow.LeaveMatch();

            Assert.Equal(ScreenId.MainMenu, nextScreen);
        }

        [Fact]
        public void Test_LeaveMatch_InsideARoom_GoesBackToTheRoom()
        {
            _playerSession.EnterRoom(SampleClients.CreateRoom());

            ScreenId nextScreen = _summaryFlow.LeaveMatch();

            Assert.Equal(ScreenId.Room, nextScreen);
        }

        [Fact]
        public void Test_LeaveMatch_DuringAMatch_ForgetsTheMatch()
        {
            _summaryFlow.LeaveMatch();

            Assert.Null(_summaryFlow.CurrentMatchView);
        }

        [Fact]
        public void Test_CurrentMatchView_DuringAMatch_ReturnsTheViewOfTheSession()
        {
            MatchView? matchView = _summaryFlow.CurrentMatchView;

            Assert.Same(_playerSession.CurrentMatchView, matchView);
        }
    }
}
