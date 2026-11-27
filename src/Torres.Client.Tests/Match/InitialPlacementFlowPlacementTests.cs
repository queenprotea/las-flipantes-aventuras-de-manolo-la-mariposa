using System;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class InitialPlacementFlowPlacementTests
    {
        private const string PlacePieceCall = nameof(RecordingGameSessionConnection.PlacePieceAsync);
        private const int SeatNumberTwo = 2;

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly MatchCell _flowerCell = new MatchCell();
        private readonly InitialPlacementFlow _placementFlow;

        public InitialPlacementFlowPlacementTests()
        {
            _placementFlow = new InitialPlacementFlow(_gameSessionConnection, _playerSession);
        }

        [Theory]
        [InlineData(MatchActionResult.CellOccupied, TextKeys.InitialPlacement.FlowerOccupiedNotice)]
        [InlineData(MatchActionResult.NotAFlower, TextKeys.InitialPlacement.NotAFlowerNotice)]
        [InlineData(MatchActionResult.OutsideBoard, TextKeys.InitialPlacement.NotAFlowerNotice)]
        [InlineData(MatchActionResult.TimeExpired, TextKeys.InitialPlacement.TimeExpiredNotice)]
        [InlineData(MatchActionResult.NotYourTurn, TextKeys.InitialPlacement.NotYourPlacementNotice)]
        [InlineData(MatchActionResult.WrongPhase, TextKeys.InitialPlacement.NotYourPlacementNotice)]
        [InlineData(MatchActionResult.NotInMatch, TextKeys.Common.ServerErrorTitle)]
        [InlineData(MatchActionResult.NotIdentified, TextKeys.Common.ServerErrorTitle)]
        public void Test_TakeNotice_WithARejectedPlacement_ShowsItsNotice(MatchActionResult rejection, string noticeKey)
        {
            _gameSessionConnection.PlacePieceReply = Task.FromResult(rejection);
            _playerSession.ApplyMatchChange(SampleClients.CreatePlacementChange());
            _placementFlow.Place(_flowerCell);

            Notice? notice = _placementFlow.TakeNotice();

            Assert.Equal(noticeKey, notice?.TitleKey);
        }

        [Fact]
        public void Test_TakeNotice_WithAnAppliedPlacement_ReturnsNull()
        {
            _gameSessionConnection.PlacePieceReply = Task.FromResult(MatchActionResult.Applied);
            _playerSession.ApplyMatchChange(SampleClients.CreatePlacementChange());
            _placementFlow.Place(_flowerCell);

            Notice? notice = _placementFlow.TakeNotice();

            Assert.Null(notice);
        }

        [Fact]
        public void Test_Place_OutsideAMatch_ReturnsFalse()
        {
            bool canPlace = _placementFlow.Place(_flowerCell);

            Assert.False(canPlace);
        }

        [Fact]
        public void Test_Place_DuringThePlacementOfAnotherSeat_ReturnsFalse()
        {
            var otherSeatView = new MatchView { Phase = MatchPhase.CaterpillarPlacement, ActiveSeatNumber = SeatNumberTwo, ViewerSeatNumber = SampleClients.SeatNumberOne };
            _playerSession.ApplyMatchChange(new MatchChange(MatchChangeKind.StateChanged, otherSeatView, DateTime.UtcNow));

            bool canPlace = _placementFlow.Place(_flowerCell);

            Assert.False(canPlace);
        }

        [Fact]
        public void Test_Place_DuringTheButterflyPlacementOfTheViewer_AsksTheServerToPlace()
        {
            var butterflyView = new MatchView { Phase = MatchPhase.ButterflyPlacement, ActiveSeatNumber = SampleClients.SeatNumberOne, ViewerSeatNumber = SampleClients.SeatNumberOne };
            _playerSession.ApplyMatchChange(new MatchChange(MatchChangeKind.StateChanged, butterflyView, DateTime.UtcNow));

            _placementFlow.Place(_flowerCell);

            Assert.Contains(PlacePieceCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_Open_OutsideAMatch_RedirectsToTheMainMenu()
        {
            ScreenId? redirect = _placementFlow.Open();

            Assert.Equal(ScreenId.MainMenu, redirect);
        }

        [Fact]
        public void Test_Open_DuringAMatch_StaysOnThePlacement()
        {
            _playerSession.ApplyMatchChange(SampleClients.CreatePlacementChange());

            ScreenId? redirect = _placementFlow.Open();

            Assert.Null(redirect);
        }

        [Fact]
        public void Test_FindPlacementTimeLeft_WithoutStepEnd_ReturnsZero()
        {
            TimeSpan placementTimeLeft = _placementFlow.FindPlacementTimeLeft();

            Assert.Equal(TimeSpan.Zero, placementTimeLeft);
        }

        [Fact]
        public void Test_FindPhaseScreen_DuringAPlayerTurn_GoesToTheMatch()
        {
            ScreenId? phaseScreen = InitialPlacementFlow.FindPhaseScreen(new MatchView { Phase = MatchPhase.PlayerTurn });

            Assert.Equal(ScreenId.Match, phaseScreen);
        }

        [Fact]
        public void Test_FindPhaseScreen_OfAFinishedMatch_GoesToTheResult()
        {
            ScreenId? phaseScreen = InitialPlacementFlow.FindPhaseScreen(new MatchView { Phase = MatchPhase.Finished });

            Assert.Equal(ScreenId.Result, phaseScreen);
        }

        [Fact]
        public void Test_FindPhaseScreen_DuringTheCaterpillarPlacement_StaysOnThePlacement()
        {
            ScreenId? phaseScreen = InitialPlacementFlow.FindPhaseScreen(new MatchView { Phase = MatchPhase.CaterpillarPlacement });

            Assert.Null(phaseScreen);
        }

        [Fact]
        public void Test_CurrentMatchView_DuringAMatch_ReturnsTheViewOfTheSession()
        {
            _playerSession.ApplyMatchChange(SampleClients.CreatePlacementChange());

            MatchView? matchView = _placementFlow.CurrentMatchView;

            Assert.Same(_playerSession.CurrentMatchView, matchView);
        }
    }
}
