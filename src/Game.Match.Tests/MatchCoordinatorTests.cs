using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Persistence;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Match.Tests
{
    public sealed class MatchCoordinatorTests
    {
        private const int SeatNumberOne = 1;
        private const int SeatNumberTwo = 2;
        private const int TwoSeatCount = 2;
        private const int ThirdSeatIndex = 2;
        private const int FiveFlowers = 5;
        private const int FourFreeFlowers = 4;
        private const int StartNoticeOnly = 1;
        private const int StartAndChangeNotices = 2;
        private const int MostStepsOfAShortMatch = 20;
        private const int LevelTwo = 2;
        private const int FourCaterpillarsLeft = 4;
        private const int MatchTurnPoints = 5;
        private const short FirstPosition = 1;
        private const short SecondPosition = 2;
        private const int SevenCardsLeft = 7;
        private const int OneCardInHand = 1;
        private const int FourActionPointsLeft = 4;
        private const int OneCardPoint = 1;
        private const int OneRecordedMatch = 1;

        private static readonly TimeSpan _recordWaitLimit = TimeSpan.FromSeconds(5);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly RecordingMatchListener _listener = new RecordingMatchListener();
        private readonly RecordingMatchRecordRepository _recordRepository = new RecordingMatchRecordRepository();
        private readonly RecordingMatchParticipantRepository _participantRepository = new RecordingMatchParticipantRepository();
        private readonly MatchCoordinator _coordinator;

        public MatchCoordinatorTests()
        {
            _coordinator = new MatchCoordinator(_fakeTimeProvider, _listener, _recordRepository, _participantRepository, SampleMatches.FiveFlowerCells, new Random(SampleMatches.ChanceSeed));
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithMembersOutOfOrder_SeatsOldestMemberFirst()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Dani, SampleMatches.Ana, SampleMatches.Beto };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, entrants), SampleMatches.StepTime, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.Ana.DisplayName, view.Seats[0].DisplayName);
            Assert.Equal(SampleMatches.Beto.DisplayName, view.Seats[1].DisplayName);
            Assert.Equal(SampleMatches.Dani.DisplayName, view.Seats[ThirdSeatIndex].DisplayName);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithInactiveMember_LeavesMemberUnseated()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.Beto, SampleMatches.InactiveEva };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, entrants), SampleMatches.StepTime, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(TwoSeatCount, view.Seats.Count);
            Assert.False(_listener.HasNotified(SampleMatches.InactiveEva.EntrantId));
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithGuest_KeepsRoomColorAndGuestMark()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.CaroGuest };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, entrants), SampleMatches.StepTime, SampleMatches.Rules);

            MatchSeat guestSeat = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[1];
            Assert.True(guestSeat.IsGuest);
            Assert.Equal(SeatColor.Red, guestSeat.Color);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTwoMembers_GivesEachRecipientTheirOwnSeat()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.Beto };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, entrants), SampleMatches.StepTime, SampleMatches.Rules);

            Assert.Equal(SeatNumberOne, _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).ViewerSeatNumber);
            Assert.Equal(SeatNumberTwo, _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).ViewerSeatNumber);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTwoMembers_OpensCaterpillarPlacementOfSeatOne()
        {
            TimeSpan stepTime = SampleMatches.StepTime;

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), stepTime, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.CaterpillarPlacement, view.Phase);
            Assert.Equal(SeatNumberOne, view.ActiveSeatNumber);
            Assert.Equal(stepTime, view.StepTimeLeft);
            Assert.Equal(FiveFlowers, view.PlacementTargets.Count);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithRepeatedEntrant_ThrowsInvalidOperationException()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.Ana };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, entrants), SampleMatches.StepTime, SampleMatches.Rules));
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarOnFreeFlower_PassesPlacementToNextSeat()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Equal(MatchActionResult.Applied, placementResult);
            Assert.Equal(SeatNumberTwo, view.ActiveSeatNumber);
            Assert.Single(view.Caterpillars);
            Assert.Equal(FourFreeFlowers, view.PlacementTargets.Count);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarOutOfTurn_ReturnsNotYourTurn()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(MatchActionResult.NotYourTurn, placementResult);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarOnGround_ReturnsNotAFlower()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.GroundCell);

            Assert.Equal(MatchActionResult.NotAFlower, placementResult);
        }

        [Fact]
        public void Test_PlacePiece_CaterpillarOutsideBoard_ThrowsArgumentOutOfRangeException()
        {
            int entrantId = SampleMatches.Ana.EntrantId;

            Assert.Throws<ArgumentOutOfRangeException>(() => _coordinator.PlacePiece(entrantId, MatchPiece.Caterpillar, SampleMatches.OutsideCell));
        }

        [Fact]
        public void Test_PlacePiece_CaterpillarWithoutMatch_ReturnsNotInMatch()
        {
            int entrantId = SampleMatches.Ana.EntrantId;

            MatchActionResult placementResult = _coordinator.PlacePiece(entrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(MatchActionResult.NotInMatch, placementResult);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarAfterDeadlineBeforeCountdownFires_ReturnsTimeExpired()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);
            _fakeTimeProvider.AdjustTime(_fakeTimeProvider.GetUtcNow() + SampleMatches.StepTime);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(MatchActionResult.TimeExpired, placementResult);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarOnActiveSeat_GivesNextSeatFullPlacementTime()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);

            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(SampleMatches.StepTime, _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).StepTimeLeft);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarInOneMatch_LeavesOtherMatchUnchanged()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.CaroAndDani), SampleMatches.StepTime, SampleMatches.Rules);

            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            MatchView otherMatchView = _listener.FindLatestViewFor(SampleMatches.Dani.EntrantId);
            Assert.Equal(SeatNumberOne, otherMatchView.ActiveSeatNumber);
            Assert.Empty(otherMatchView.Caterpillars);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarOnActiveSeat_SendsChangeAfterStart()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);

            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(new RecordedNoticeKind[] { RecordedNoticeKind.MatchStarted, RecordedNoticeKind.MatchStateChanged }, _listener.NoticeKinds);
        }

        [Fact]
        public async Task Test_ExpirePlacement_AtPlacementDeadline_PassesPlacementToNextSeat()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(StartAndChangeNotices, _listener.NoticeCount);
            Assert.Equal(SeatNumberTwo, view.ActiveSeatNumber);
            Assert.Empty(view.Caterpillars);
        }

        [Fact]
        public async Task Test_ExpirePlacement_JustBeforePlacementDeadline_DoesNotRun()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);

            _fakeTimeProvider.Advance(SampleMatches.JustBeforeDeadline);

            Assert.Equal(StartNoticeOnly, _listener.NoticeCount);
        }

        [Fact]
        public async Task Test_ExpirePlacement_AtFirstDeadlineAfterPlacing_KeepsNextSeatPlacing()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaBetoAndDani), SampleMatches.StepTime, SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            _fakeTimeProvider.Advance(SampleMatches.RestOfFirstPlacement);

            Assert.Equal(SeatNumberTwo, _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_ExpirePlacement_OfLastSeat_GivesButterflyToLastPlacer()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaBetoAndDani), SampleMatches.StepTime, SampleMatches.Rules);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Dani.EntrantId);
            Assert.Equal(MatchPhase.ButterflyPlacement, view.Phase);
            Assert.Equal(SeatNumberTwo, view.ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_TurnTime_BeforeRunningOut_SendsNoChange()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            int noticeCountAtFirstTurn = _listener.NoticeCount;

            _fakeTimeProvider.Advance(SampleMatches.JustBeforeDeadline);

            Assert.Equal(noticeCountAtFirstTurn, _listener.NoticeCount);
        }

        [Fact]
        public async Task Test_PlacePiece_ButterflyDuringCaterpillarPlacement_ReturnsWrongPhase()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);

            Assert.Equal(MatchActionResult.WrongPhase, placementResult);
        }

        [Fact]
        public async Task Test_PlacePiece_ButterflyByLastPlacer_StartsFirstTurnWithTheWholeTurnTime()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchActionResult.Applied, placementResult);
            Assert.Equal(MatchPhase.PlayerTurn, view.Phase);
            Assert.Equal(SeatNumberOne, view.ActiveSeatNumber);
            Assert.True(view.HasButterfly);
            Assert.Equal(SampleMatches.CenterFlower.Row, view.ButterflyCell.Row);
            Assert.Equal(SampleMatches.CenterFlower.Column, view.ButterflyCell.Column);
            Assert.Equal(SampleMatches.StepTime, view.StepTimeLeft);
        }

        [Fact]
        public async Task Test_TurnTime_RunningOut_PassesTheTurnToSeatTwo()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.PlayerTurn, view.Phase);
            Assert.Equal(SeatNumberTwo, view.ActiveSeatNumber);
            Assert.Equal(SampleMatches.StepTime, view.StepTimeLeft);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOut_FinishesTheMatchOnce()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            Assert.Equal(1, _listener.FinishCount);
            Assert.Equal(MatchPhase.Finished, _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).Phase);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOut_ReleasesBothPlayers()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            Assert.Equal(new int[] { SampleMatches.Ana.EntrantId, SampleMatches.Beto.EntrantId }, _listener.ReleasedEntrantIds.Order());
            Assert.Equal(MatchActionResult.NotInMatch, _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower));
        }

        [Fact]
        public async Task Test_FinishedMatch_StopsItsClock()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.OneTurnPerRoundRules);
            AdvanceUntilFinished();
            int noticeCountAtFinish = _listener.NoticeCount;

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            Assert.Equal(noticeCountAtFinish, _listener.NoticeCount);
        }

        [Fact]
        public async Task Test_FinishedMatch_LetsBothPlayersStartAgain()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.OneTurnPerRoundRules);
            AdvanceUntilFinished();

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);

            Assert.Equal(MatchActionResult.Applied, _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower));
        }

        [Fact]
        public async Task Test_EndMatch_BySeatedPlayer_LetsBothPlayersStartAgain()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);

            EndMatchResult endResult = _coordinator.EndMatch(SampleMatches.Beto.EntrantId);
            int noticeCountAtEnd = _listener.NoticeCount;
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);

            Assert.Equal(EndMatchResult.Ended, endResult);
            Assert.Equal(noticeCountAtEnd + StartNoticeOnly, _listener.NoticeCount);
        }

        [Fact]
        public async Task Test_EndMatch_DuringPlacement_StopsPlacementCountdown()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);
            _coordinator.EndMatch(SampleMatches.Ana.EntrantId);
            int noticeCountAtEnd = _listener.NoticeCount;

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            Assert.Equal(noticeCountAtEnd, _listener.NoticeCount);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarAfterMatchEnded_ReturnsNotInMatch()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);
            _coordinator.EndMatch(SampleMatches.Beto.EntrantId);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(MatchActionResult.NotInMatch, placementResult);
        }

        [Fact]
        public void Test_EndMatch_WithoutMatch_ReturnsNotInMatch()
        {
            EndMatchResult endResult = _coordinator.EndMatch(SampleMatches.Ana.EntrantId);

            Assert.Equal(EndMatchResult.NotInMatch, endResult);
        }

        [Fact]
        public void Test_BoardContains_WithRowEight_ReturnsFalse()
        {
            MatchCell outsideCell = SampleMatches.OutsideCell;

            bool isOnBoard = MatchCoordinator.BoardContains(outsideCell);

            Assert.False(isOnBoard);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarInTurn_ChargesItsCostWithoutRestartingTheClock()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);

            MatchActionResult actionResult = _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.SouthWestFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchActionResult.Applied, actionResult);
            Assert.Equal(SampleMatches.CaterpillarPlacementCost, MatchTurnPoints - view.ActionPointsLeft);
            Assert.Equal(SampleMatches.RestOfFirstPlacement, view.StepTimeLeft);
        }

        [Fact]
        public async Task Test_MoveCaterpillar_ToGroundNeighbor_ShowsItOnTheDestination()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            var move = new MatchMove { Origin = SampleMatches.NorthWestFlower, Destination = SampleMatches.SouthOfNorthWest };

            MatchActionResult actionResult = _coordinator.MoveCaterpillar(SampleMatches.Ana.EntrantId, move);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Equal(MatchActionResult.Applied, actionResult);
            Assert.Contains(view.Caterpillars, caterpillar => (caterpillar.Cell.Row == SampleMatches.SouthOfNorthWest.Row) && (caterpillar.Cell.Column == SampleMatches.SouthOfNorthWest.Column));
        }

        [Fact]
        public async Task Test_MoveCaterpillar_OutOfTurn_ReturnsNotYourTurn()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            var move = new MatchMove { Origin = SampleMatches.NorthEastFlower, Destination = SampleMatches.SouthOfNorthEast };

            MatchActionResult actionResult = _coordinator.MoveCaterpillar(SampleMatches.Beto.EntrantId, move);

            Assert.Equal(MatchActionResult.NotYourTurn, actionResult);
        }

        [Fact]
        public async Task Test_AddFlowerLevel_OnFreeFlower_ShowsTheHigherFlower()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            MatchActionResult actionResult = _coordinator.AddFlowerLevel(SampleMatches.Ana.EntrantId, SampleMatches.SouthWestFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchActionResult.Applied, actionResult);
            Assert.Contains(view.Flowers, flower => (flower.Cell.Row == SampleMatches.SouthWestFlower.Row) && (flower.Cell.Column == SampleMatches.SouthWestFlower.Column) && (flower.Level == LevelTwo));
        }

        [Fact]
        public async Task Test_EndTurn_BySeatOne_GivesSeatTwoTheWholeTurnTime()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);

            MatchActionResult actionResult = _coordinator.EndTurn(SampleMatches.Ana.EntrantId);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchActionResult.Applied, actionResult);
            Assert.Equal(SeatNumberTwo, view.ActiveSeatNumber);
            Assert.Equal(SampleMatches.StepTime, view.StepTimeLeft);
        }

        [Fact]
        public void Test_EndTurn_WithoutMatch_ReturnsNotInMatch()
        {
            MatchActionResult actionResult = _coordinator.EndTurn(SampleMatches.Ana.EntrantId);

            Assert.Equal(MatchActionResult.NotInMatch, actionResult);
        }

        [Fact]
        public async Task Test_PlaceButterfly_ByLastPlacer_SendsMoveTargetsOnlyToThePlayingSeat()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.NotEmpty(anaView.MoveTargets);
            Assert.Empty(betoView.MoveTargets);
            Assert.Equal(new int[] { FourCaterpillarsLeft, FourCaterpillarsLeft }, anaView.Seats.Select(seat => seat.CaterpillarsLeft));
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOut_SendsTheRanking()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(TwoSeatCount, view.RankedSeatNumbers.Count);
            Assert.False(view.WasAbandoned);
        }

        [Fact]
        public async Task Test_DrawCard_OnOwnTurn_ShowsTheCardOnlyToItsOwner()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            MatchActionResult drawResult = _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Equal(MatchActionResult.Applied, drawResult);
            Assert.Single(anaView.Hand);
            Assert.Equal(SevenCardsLeft, anaView.DeckCount);
            Assert.Equal(FourActionPointsLeft, anaView.ActionPointsLeft);
            Assert.Empty(betoView.Hand);
            Assert.Equal(OneCardInHand, betoView.Seats[0].HandCount);
        }

        [Fact]
        public async Task Test_DrawCard_OnOwnTurn_TellsOnlyTheActivePlayerWhatItCanDo()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Equal(anaView.Hand, anaView.PlayableCards);
            Assert.True(anaView.CanDrawCard);
            Assert.False(betoView.CanDrawCard);
            Assert.Empty(betoView.PlayableCards);
        }

        [Fact]
        public async Task Test_DrawCard_OutOfTurn_ReturnsNotYourTurn()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            MatchActionResult drawResult = _coordinator.DrawCard(SampleMatches.Beto.EntrantId);

            Assert.Equal(MatchActionResult.NotYourTurn, drawResult);
        }

        [Fact]
        public async Task Test_PlayCard_NotInHand_ReturnsCardNotInHand()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            MatchActionResult playResult = _coordinator.PlayCard(SampleMatches.Ana.EntrantId, new MatchCardPlay { Card = MatchCard.SevenActionPoints });

            Assert.Equal(MatchActionResult.CardNotInHand, playResult);
        }

        [Fact]
        public void Test_DrawCard_WithoutMatch_ReturnsNotInMatch()
        {
            MatchActionResult drawResult = _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            Assert.Equal(MatchActionResult.NotInMatch, drawResult);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithCardCosts_SendsThemInTheView()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);

            MatchActionCosts actionCosts = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).ActionCosts;
            Assert.Equal(SampleMatches.Rules.ActionPointCosts.CardDraw, actionCosts.CardDraw);
            Assert.Equal(SampleMatches.Rules.ActionPointCosts.CardPlay, actionCosts.CardPlay);
        }

        private async Task StartPlayedOpeningAsync(MatchRuleSet ruleSet)
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, ruleSet);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithGuest_RecordsOnlyThePlayerWithAccount()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.CaroGuest };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, entrants), SampleMatches.StepTime, SampleMatches.Rules);

            MatchParticipant seatedPlayer = Assert.Single(_participantRepository.LastSeatedPlayers);
            Assert.Equal(SampleMatches.RoomId, _recordRepository.LastRoomId);
            Assert.Equal(TwoSeatCount, _recordRepository.LastPlayerCount);
            Assert.Equal(SampleMatches.Ana.EntrantId, seatedPlayer.PlayerId);
            Assert.Equal(SeatNumberOne, seatedPlayer.SeatNumber);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTwoMembers_ReturnsTheRecordedMatchId()
        {
            int matchId = await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);

            Assert.Equal(_recordRepository.LastMatchId, matchId);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOut_RecordsCompletedMatchWithPositions()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            RecordedFinish recordedFinish = await _recordRepository.FirstFinish.WaitAsync(_recordWaitLimit);
            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            Assert.Equal(MatchEndReasonCodes.Completed, recordedFinish.EndReason);
            Assert.Equal(new short?[] { FirstPosition, SecondPosition }, playerResults.Select(playerResult => playerResult.FinalPosition).Order());
            Assert.DoesNotContain(null, playerResults.Select(playerResult => playerResult.FinalScore));
            Assert.DoesNotContain(playerResults, playerResult => playerResult.WasWithdrawn);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOutWithAKeptCard_RecordsTheFinalScoreWithTheCardPoint()
        {
            await StartPlayedOpeningAsync(SampleMatches.OneTurnPerRoundRules);
            _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            AdvanceUntilFinished();

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            MatchParticipant anaResult = playerResults.Single(playerResult => playerResult.PlayerId == SampleMatches.Ana.EntrantId);
            MatchSeat anaSeat = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[0];
            Assert.Equal(OneCardPoint, anaSeat.CardPoints);
            Assert.Equal(anaSeat.FinalScore, anaResult.FinalScore);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithEntrantAlreadySeated_WritesNoSecondMatch()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules));

            Assert.Equal(OneRecordedMatch, _recordRepository.LastMatchId);
        }

        [Fact]
        public async Task Test_EndMatch_BySeatedPlayer_RecordsAbandonedMatchWithoutResults()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.AnaAndBeto), SampleMatches.StepTime, SampleMatches.Rules);

            _coordinator.EndMatch(SampleMatches.Ana.EntrantId);

            RecordedFinish recordedFinish = await _recordRepository.FirstFinish.WaitAsync(_recordWaitLimit);
            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            Assert.Equal(MatchEndReasonCodes.Abandoned, recordedFinish.EndReason);
            Assert.Equal(new short?[] { null, null }, playerResults.Select(playerResult => playerResult.FinalPosition));
        }

        private void AdvanceUntilFinished()
        {
            for (var i = 0; (i < MostStepsOfAShortMatch) && (_listener.FinishCount == 0); i++)
            {
                _fakeTimeProvider.Advance(SampleMatches.StepTime);
            }
        }
    }
}
