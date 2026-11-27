using System;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class MatchFlowActionTests
    {
        private const string PlacePieceCall = nameof(RecordingGameSessionConnection.PlacePieceAsync);
        private const string AddFlowerLevelCall = nameof(RecordingGameSessionConnection.AddFlowerLevelAsync);
        private const string MoveCaterpillarCall = nameof(RecordingGameSessionConnection.MoveCaterpillarAsync);
        private const string PlayCardCall = nameof(RecordingGameSessionConnection.PlayCardAsync);
        private const string EndTurnCall = nameof(RecordingGameSessionConnection.EndTurnAsync);
        private const int SeatNumberTwo = 2;

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly MatchCell _targetCell = new MatchCell();
        private readonly MatchFlow _matchFlow;

        public MatchFlowActionTests()
        {
            _matchFlow = new MatchFlow(_gameSessionConnection, _playerSession);
        }

        [Theory]
        [InlineData(MatchActionResult.NotYourTurn, TextKeys.Match.NotYourTurnNotice)]
        [InlineData(MatchActionResult.WrongPhase, TextKeys.Match.NotYourTurnNotice)]
        [InlineData(MatchActionResult.TimeExpired, TextKeys.Match.TimeExpiredNotice)]
        [InlineData(MatchActionResult.NoCaterpillarsLeft, TextKeys.Match.NoCaterpillarsLeftNotice)]
        [InlineData(MatchActionResult.NotAFlower, TextKeys.Match.NotAFlowerNotice)]
        [InlineData(MatchActionResult.OutsideBoard, TextKeys.Match.NotAFlowerNotice)]
        [InlineData(MatchActionResult.CellOccupied, TextKeys.Match.CellOccupiedNotice)]
        [InlineData(MatchActionResult.NotYourCaterpillar, TextKeys.Match.NotYourCaterpillarNotice)]
        [InlineData(MatchActionResult.NotNeighborCell, TextKeys.Match.NotNeighborNotice)]
        [InlineData(MatchActionResult.ClimbTooHigh, TextKeys.Match.ClimbTooHighNotice)]
        [InlineData(MatchActionResult.FlowerAtMaximumLevel, TextKeys.Match.FlowerAtMaximumNotice)]
        [InlineData(MatchActionResult.NoBuildsLeft, TextKeys.Match.NoBuildsLeftNotice)]
        [InlineData(MatchActionResult.NotNextToPlanter, TextKeys.Match.NotNextToPlanterNotice)]
        [InlineData(MatchActionResult.JoinsPlanters, TextKeys.Match.JoinsPlantersNotice)]
        [InlineData(MatchActionResult.DeckEmpty, TextKeys.Match.DeckEmptyNotice)]
        [InlineData(MatchActionResult.CardNotInHand, TextKeys.Match.CardNotInHandNotice)]
        [InlineData(MatchActionResult.PointsCardAlreadyUsed, TextKeys.Match.PointsCardUsedNotice)]
        [InlineData(MatchActionResult.WrongCardTarget, TextKeys.Match.WrongCardTargetNotice)]
        [InlineData(MatchActionResult.NotInMatch, TextKeys.Common.ServerErrorTitle)]
        [InlineData(MatchActionResult.NotIdentified, TextKeys.Common.ServerErrorTitle)]
        public void Test_TakeOutcome_WithARejectedAction_ShowsItsNotice(MatchActionResult rejection, string noticeKey)
        {
            _gameSessionConnection.DrawCardReply = Task.FromResult(rejection);
            _matchFlow.DrawCard();

            MatchActionOutcome? outcome = _matchFlow.TakeOutcome();

            Assert.Equal(noticeKey, outcome?.Notice?.TitleKey);
        }

        [Fact]
        public void Test_TakeOutcome_WithAnAppliedAction_ShowsNoNotice()
        {
            _gameSessionConnection.DrawCardReply = Task.FromResult(MatchActionResult.Applied);
            _matchFlow.DrawCard();

            MatchActionOutcome? outcome = _matchFlow.TakeOutcome();

            Assert.Equal(new MatchActionOutcome(null), outcome);
        }

        [Fact]
        public void Test_TakeOutcome_WithoutAction_ReturnsNull()
        {
            MatchActionOutcome? outcome = _matchFlow.TakeOutcome();

            Assert.Null(outcome);
        }

        [Fact]
        public void Test_PlaceCaterpillar_OnACell_AsksTheServerToPlaceAPiece()
        {
            _matchFlow.PlaceCaterpillar(_targetCell);

            Assert.Contains(PlacePieceCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnACell_AsksTheServerToBuild()
        {
            _matchFlow.AddFlowerLevel(_targetCell);

            Assert.Contains(AddFlowerLevelCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_MoveCaterpillar_WithAMove_AsksTheServerToMove()
        {
            _matchFlow.MoveCaterpillar(new MatchMove());

            Assert.Contains(MoveCaterpillarCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_PlayCard_WithACard_AsksTheServerToPlayIt()
        {
            _matchFlow.PlayCard(new MatchCardPlay());

            Assert.Contains(PlayCardCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_EndTurn_DuringTheTurn_AsksTheServerToEndIt()
        {
            _matchFlow.EndTurn();

            Assert.Contains(EndTurnCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_IsActing_AfterStartingAnAction_ReturnsTrue()
        {
            _matchFlow.EndTurn();

            bool isActing = _matchFlow.IsActing;

            Assert.True(isActing);
        }

        [Fact]
        public void Test_Open_OutsideAMatch_RedirectsToTheMainMenu()
        {
            ScreenId? redirect = _matchFlow.Open();

            Assert.Equal(ScreenId.MainMenu, redirect);
        }

        [Fact]
        public void Test_Open_DuringAMatch_StaysOnTheMatch()
        {
            _playerSession.ApplyMatchChange(SampleClients.CreatePlacementChange());

            ScreenId? redirect = _matchFlow.Open();

            Assert.Null(redirect);
        }

        [Fact]
        public void Test_RoomCode_OutsideARoom_ReturnsAnEmptyCode()
        {
            string roomCode = _matchFlow.RoomCode;

            Assert.Equal(string.Empty, roomCode);
        }

        [Fact]
        public void Test_FindStepTimeLeft_WithoutStepEnd_ReturnsZero()
        {
            TimeSpan stepTimeLeft = _matchFlow.FindStepTimeLeft();

            Assert.Equal(TimeSpan.Zero, stepTimeLeft);
        }

        [Fact]
        public void Test_IsViewerPlaying_OnTheTurnOfTheViewer_ReturnsTrue()
        {
            var matchView = new MatchView { Phase = MatchPhase.PlayerTurn, ActiveSeatNumber = SampleClients.SeatNumberOne, ViewerSeatNumber = SampleClients.SeatNumberOne };

            bool isViewerPlaying = MatchFlow.IsViewerPlaying(matchView);

            Assert.True(isViewerPlaying);
        }

        [Fact]
        public void Test_IsViewerPlaying_OnTheTurnOfAnotherSeat_ReturnsFalse()
        {
            var matchView = new MatchView { Phase = MatchPhase.PlayerTurn, ActiveSeatNumber = SeatNumberTwo, ViewerSeatNumber = SampleClients.SeatNumberOne };

            bool isViewerPlaying = MatchFlow.IsViewerPlaying(matchView);

            Assert.False(isViewerPlaying);
        }

        [Fact]
        public void Test_FindNextScreen_DuringTheButterflyPlacement_GoesToTheRoundSummary()
        {
            ScreenId? nextScreen = MatchFlow.FindNextScreen(new MatchView { Phase = MatchPhase.ButterflyPlacement });

            Assert.Equal(ScreenId.RoundSummary, nextScreen);
        }

        [Fact]
        public void Test_FindNextScreen_OfAFinishedMatch_GoesToTheResult()
        {
            ScreenId? nextScreen = MatchFlow.FindNextScreen(new MatchView { Phase = MatchPhase.Finished });

            Assert.Equal(ScreenId.Result, nextScreen);
        }

        [Fact]
        public void Test_FindNextScreen_DuringAPlayerTurn_StaysOnTheMatch()
        {
            ScreenId? nextScreen = MatchFlow.FindNextScreen(new MatchView { Phase = MatchPhase.PlayerTurn });

            Assert.Null(nextScreen);
        }
    }
}
