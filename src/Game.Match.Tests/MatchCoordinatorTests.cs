using System;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

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

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly RecordingMatchListener _listener = new RecordingMatchListener();
        private readonly MatchCoordinator _coordinator;

        public MatchCoordinatorTests()
        {
            _coordinator = new MatchCoordinator(_fakeTimeProvider, _listener, SampleMatches.FiveFlowerCells, new Random(SampleMatches.TieBreakSeed));
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithMembersOutOfOrder_SeatsOldestMemberFirst()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Dani, SampleMatches.Ana, SampleMatches.Beto };

            await _coordinator.StartMatchAsync(entrants, SampleMatches.PlacementTime, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.Ana.DisplayName, view.Seats[0].DisplayName);
            Assert.Equal(SampleMatches.Beto.DisplayName, view.Seats[1].DisplayName);
            Assert.Equal(SampleMatches.Dani.DisplayName, view.Seats[ThirdSeatIndex].DisplayName);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithInactiveMember_LeavesMemberUnseated()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.Beto, SampleMatches.InactiveEva };

            await _coordinator.StartMatchAsync(entrants, SampleMatches.PlacementTime, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(TwoSeatCount, view.Seats.Count);
            Assert.False(_listener.HasNotified(SampleMatches.InactiveEva.EntrantId));
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithGuest_KeepsRoomColorAndGuestMark()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.CaroGuest };

            await _coordinator.StartMatchAsync(entrants, SampleMatches.PlacementTime, SampleMatches.Rules);

            MatchSeat guestSeat = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[1];
            Assert.True(guestSeat.IsGuest);
            Assert.Equal(SeatColor.Red, guestSeat.Color);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTwoMembers_GivesEachRecipientTheirOwnSeat()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.Beto };

            await _coordinator.StartMatchAsync(entrants, SampleMatches.PlacementTime, SampleMatches.Rules);

            Assert.Equal(SeatNumberOne, _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).ViewerSeatNumber);
            Assert.Equal(SeatNumberTwo, _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).ViewerSeatNumber);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTwoMembers_OpensCaterpillarPlacementOfSeatOne()
        {
            TimeSpan placementTime = SampleMatches.PlacementTime;

            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, placementTime, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.CaterpillarPlacement, view.Phase);
            Assert.Equal(SeatNumberOne, view.ActiveSeatNumber);
            Assert.Equal(placementTime, view.PlacementTimeLeft);
            Assert.Equal(FiveFlowers, view.PlacementTargets.Count);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithRepeatedEntrant_ThrowsInvalidOperationException()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.Ana };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _coordinator.StartMatchAsync(entrants, SampleMatches.PlacementTime, SampleMatches.Rules));
        }

        [Fact]
        public async Task Test_PlaceCaterpillar_OnFreeFlower_PassesPlacementToNextSeat()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);

            PlacementResult placementResult = _coordinator.PlaceCaterpillar(SampleMatches.Ana.EntrantId, SampleMatches.NorthWestFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Equal(PlacementResult.Placed, placementResult);
            Assert.Equal(SeatNumberTwo, view.ActiveSeatNumber);
            Assert.Single(view.Caterpillars);
            Assert.Equal(FourFreeFlowers, view.PlacementTargets.Count);
        }

        [Fact]
        public async Task Test_PlaceCaterpillar_OutOfTurn_ReturnsNotYourPlacement()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);

            PlacementResult placementResult = _coordinator.PlaceCaterpillar(SampleMatches.Beto.EntrantId, SampleMatches.NorthWestFlower);

            Assert.Equal(PlacementResult.NotYourPlacement, placementResult);
        }

        [Fact]
        public async Task Test_PlaceCaterpillar_OnGround_ReturnsNotAFlower()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);

            PlacementResult placementResult = _coordinator.PlaceCaterpillar(SampleMatches.Ana.EntrantId, SampleMatches.GroundCell);

            Assert.Equal(PlacementResult.NotAFlower, placementResult);
        }

        [Fact]
        public void Test_PlaceCaterpillar_OutsideBoard_ThrowsArgumentOutOfRangeException()
        {
            int entrantId = SampleMatches.Ana.EntrantId;

            Assert.Throws<ArgumentOutOfRangeException>(() => _coordinator.PlaceCaterpillar(entrantId, SampleMatches.OutsideCell));
        }

        [Fact]
        public void Test_PlaceCaterpillar_WithoutMatch_ReturnsNotInMatch()
        {
            int entrantId = SampleMatches.Ana.EntrantId;

            PlacementResult placementResult = _coordinator.PlaceCaterpillar(entrantId, SampleMatches.NorthWestFlower);

            Assert.Equal(PlacementResult.NotInMatch, placementResult);
        }

        [Fact]
        public async Task Test_PlaceCaterpillar_AfterDeadlineBeforeCountdownFires_ReturnsTimeExpired()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);
            _fakeTimeProvider.AdjustTime(_fakeTimeProvider.GetUtcNow() + SampleMatches.PlacementTime);

            PlacementResult placementResult = _coordinator.PlaceCaterpillar(SampleMatches.Ana.EntrantId, SampleMatches.NorthWestFlower);

            Assert.Equal(PlacementResult.TimeExpired, placementResult);
        }

        [Fact]
        public async Task Test_PlaceCaterpillar_OnActiveSeat_GivesNextSeatFullPlacementTime()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);

            _coordinator.PlaceCaterpillar(SampleMatches.Ana.EntrantId, SampleMatches.NorthWestFlower);

            Assert.Equal(SampleMatches.PlacementTime, _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).PlacementTimeLeft);
        }

        [Fact]
        public async Task Test_PlaceCaterpillar_InOneMatch_LeavesOtherMatchUnchanged()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);
            await _coordinator.StartMatchAsync(SampleMatches.CaroAndDani, SampleMatches.PlacementTime, SampleMatches.Rules);

            _coordinator.PlaceCaterpillar(SampleMatches.Ana.EntrantId, SampleMatches.NorthWestFlower);

            MatchView otherMatchView = _listener.FindLatestViewFor(SampleMatches.Dani.EntrantId);
            Assert.Equal(SeatNumberOne, otherMatchView.ActiveSeatNumber);
            Assert.Empty(otherMatchView.Caterpillars);
        }

        [Fact]
        public async Task Test_PlaceCaterpillar_OnActiveSeat_SendsChangeAfterStart()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);

            _coordinator.PlaceCaterpillar(SampleMatches.Ana.EntrantId, SampleMatches.NorthWestFlower);

            Assert.Equal(new RecordedNoticeKind[] { RecordedNoticeKind.MatchStarted, RecordedNoticeKind.MatchStateChanged }, _listener.NoticeKinds);
        }

        [Fact]
        public async Task Test_ExpirePlacement_AtPlacementDeadline_PassesPlacementToNextSeat()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);

            _fakeTimeProvider.Advance(SampleMatches.PlacementTime);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(StartAndChangeNotices, _listener.NoticeCount);
            Assert.Equal(SeatNumberTwo, view.ActiveSeatNumber);
            Assert.Empty(view.Caterpillars);
        }

        [Fact]
        public async Task Test_ExpirePlacement_JustBeforePlacementDeadline_DoesNotRun()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);

            _fakeTimeProvider.Advance(SampleMatches.JustBeforeDeadline);

            Assert.Equal(StartNoticeOnly, _listener.NoticeCount);
        }

        [Fact]
        public async Task Test_ExpirePlacement_AtFirstDeadlineAfterPlacing_KeepsNextSeatPlacing()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaBetoAndDani, SampleMatches.PlacementTime, SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);
            _coordinator.PlaceCaterpillar(SampleMatches.Ana.EntrantId, SampleMatches.NorthWestFlower);

            _fakeTimeProvider.Advance(SampleMatches.RestOfFirstPlacement);

            Assert.Equal(SeatNumberTwo, _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_ExpirePlacement_OfLastSeat_GivesButterflyToLastPlacer()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaBetoAndDani, SampleMatches.PlacementTime, SampleMatches.Rules);
            _coordinator.PlaceCaterpillar(SampleMatches.Ana.EntrantId, SampleMatches.NorthWestFlower);
            _coordinator.PlaceCaterpillar(SampleMatches.Beto.EntrantId, SampleMatches.NorthEastFlower);

            _fakeTimeProvider.Advance(SampleMatches.PlacementTime);

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
        public async Task Test_PlaceButterfly_DuringCaterpillarPlacement_ReturnsWrongPhase()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);

            PlacementResult placementResult = _coordinator.PlaceButterfly(SampleMatches.Ana.EntrantId, SampleMatches.CenterFlower);

            Assert.Equal(PlacementResult.WrongPhase, placementResult);
        }

        [Fact]
        public async Task Test_PlaceButterfly_ByLastPlacer_StartsFirstTurnWithTheWholeTurnTime()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);
            _coordinator.PlaceCaterpillar(SampleMatches.Ana.EntrantId, SampleMatches.NorthWestFlower);
            _coordinator.PlaceCaterpillar(SampleMatches.Beto.EntrantId, SampleMatches.NorthEastFlower);

            PlacementResult placementResult = _coordinator.PlaceButterfly(SampleMatches.Beto.EntrantId, SampleMatches.CenterFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(PlacementResult.Placed, placementResult);
            Assert.Equal(MatchPhase.PlayerTurn, view.Phase);
            Assert.Equal(SeatNumberOne, view.ActiveSeatNumber);
            Assert.True(view.HasButterfly);
            Assert.Equal(SampleMatches.CenterFlower.Row, view.ButterflyCell.Row);
            Assert.Equal(SampleMatches.CenterFlower.Column, view.ButterflyCell.Column);
            Assert.Equal(SampleMatches.PlacementTime, view.PlacementTimeLeft);
        }

        [Fact]
        public async Task Test_TurnTime_RunningOut_PassesTheTurnToSeatTwo()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _fakeTimeProvider.Advance(SampleMatches.PlacementTime);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.PlayerTurn, view.Phase);
            Assert.Equal(SeatNumberTwo, view.ActiveSeatNumber);
            Assert.Equal(SampleMatches.PlacementTime, view.PlacementTimeLeft);
        }

        [Fact]
        public async Task Test_TimeRunningOutForEveryStep_FinishesTheMatchOnce()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            Assert.Equal(1, _listener.FinishCount);
            Assert.Equal(MatchPhase.Finished, _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).Phase);
        }

        [Fact]
        public async Task Test_TimeRunningOutForEveryStep_ReleasesBothPlayers()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            Assert.Equal(new int[] { SampleMatches.Ana.EntrantId, SampleMatches.Beto.EntrantId }, _listener.ReleasedEntrantIds.Order());
            Assert.Equal(PlacementResult.NotInMatch, _coordinator.PlaceCaterpillar(SampleMatches.Ana.EntrantId, SampleMatches.NorthWestFlower));
        }

        [Fact]
        public async Task Test_FinishedMatch_StopsItsClock()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.OneTurnPerRoundRules);
            AdvanceUntilFinished();
            int noticeCountAtFinish = _listener.NoticeCount;

            _fakeTimeProvider.Advance(SampleMatches.PlacementTime);

            Assert.Equal(noticeCountAtFinish, _listener.NoticeCount);
        }

        [Fact]
        public async Task Test_FinishedMatch_LetsBothPlayersStartAgain()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.OneTurnPerRoundRules);
            AdvanceUntilFinished();

            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);

            Assert.Equal(PlacementResult.Placed, _coordinator.PlaceCaterpillar(SampleMatches.Ana.EntrantId, SampleMatches.NorthWestFlower));
        }

        [Fact]
        public async Task Test_EndMatch_BySeatedPlayer_LetsBothPlayersStartAgain()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);

            EndMatchResult endResult = _coordinator.EndMatch(SampleMatches.Beto.EntrantId);
            int noticeCountAtEnd = _listener.NoticeCount;
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);

            Assert.Equal(EndMatchResult.Ended, endResult);
            Assert.Equal(noticeCountAtEnd + StartNoticeOnly, _listener.NoticeCount);
        }

        [Fact]
        public async Task Test_EndMatch_DuringPlacement_StopsPlacementCountdown()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);
            _coordinator.EndMatch(SampleMatches.Ana.EntrantId);
            int noticeCountAtEnd = _listener.NoticeCount;

            _fakeTimeProvider.Advance(SampleMatches.PlacementTime);

            Assert.Equal(noticeCountAtEnd, _listener.NoticeCount);
        }

        [Fact]
        public async Task Test_PlaceCaterpillar_AfterMatchEnded_ReturnsNotInMatch()
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, SampleMatches.Rules);
            _coordinator.EndMatch(SampleMatches.Beto.EntrantId);

            PlacementResult placementResult = _coordinator.PlaceCaterpillar(SampleMatches.Ana.EntrantId, SampleMatches.NorthWestFlower);

            Assert.Equal(PlacementResult.NotInMatch, placementResult);
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

        private async Task StartPlayedOpeningAsync(MatchRuleSet ruleSet)
        {
            await _coordinator.StartMatchAsync(SampleMatches.AnaAndBeto, SampleMatches.PlacementTime, ruleSet);
            _coordinator.PlaceCaterpillar(SampleMatches.Ana.EntrantId, SampleMatches.NorthWestFlower);
            _coordinator.PlaceCaterpillar(SampleMatches.Beto.EntrantId, SampleMatches.NorthEastFlower);
            _coordinator.PlaceButterfly(SampleMatches.Beto.EntrantId, SampleMatches.CenterFlower);
        }

        private void AdvanceUntilFinished()
        {
            for (var i = 0; (i < MostStepsOfAShortMatch) && (_listener.FinishCount == 0); i++)
            {
                _fakeTimeProvider.Advance(SampleMatches.PlacementTime);
            }
        }
    }
}
