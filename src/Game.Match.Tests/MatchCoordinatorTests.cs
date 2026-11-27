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
        private const int SeatNumberThree = 3;
        private const int SecondSeatIndex = 1;
        private const int NorthWestPlanterNumber = 1;
        private const int NorthEastPlanterNumber = 2;
        private const int ReconnectionLongerThanTheTurnSeconds = 180;
        private const int OneFinish = 1;
        private const int NoFinish = 0;
        private const int PointsLeftAfterCaterpillarPlacement = MatchTurnPoints - SampleMatches.CaterpillarPlacementCost;

        private static readonly TimeSpan _recordWaitLimit = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan _oneTick = TimeSpan.FromTicks(1);
        private static readonly TimeSpan _halfReconnectionTime = SampleMatches.ReconnectionTime / 2;
        private static readonly MatchTimeLimits _reconnectionLongerThanTheTurnLimits = new MatchTimeLimits(SampleMatches.StepTime, TimeSpan.FromSeconds(ReconnectionLongerThanTheTurnSeconds));
        private static readonly bool[] _expectedSeatWithdrawals = new bool[] { true, false, true };

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly RecordingMatchListener _listener = new RecordingMatchListener();
        private readonly RecordingMatchRecordRepository _recordRepository = new RecordingMatchRecordRepository();
        private readonly RecordingMatchParticipantRepository _participantRepository = new RecordingMatchParticipantRepository();
        private readonly RecordingMatchStateRepository _stateRepository = new RecordingMatchStateRepository();
        private readonly MatchCoordinator _coordinator;

        public MatchCoordinatorTests()
        {
            _coordinator = new MatchCoordinator(_fakeTimeProvider, _listener, new MatchRepositories(_recordRepository, _participantRepository, _stateRepository), SampleMatches.FiveFlowerCells, new Random(SampleMatches.ChanceSeed));
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithMembersOutOfOrder_SeatsOldestMemberFirst()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Dani, SampleMatches.Ana, SampleMatches.Beto };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.Ana.DisplayName, view.Seats[0].DisplayName);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithMembersOutOfOrder_SeatsTheSecondOldestMemberSecond()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Dani, SampleMatches.Ana, SampleMatches.Beto };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.Beto.DisplayName, view.Seats[1].DisplayName);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithMembersOutOfOrder_SeatsTheNewestMemberLast()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Dani, SampleMatches.Ana, SampleMatches.Beto };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.Dani.DisplayName, view.Seats[ThirdSeatIndex].DisplayName);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithInactiveMember_SeatsOnlyTheActiveMembers()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.Beto, SampleMatches.InactiveEva };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(TwoSeatCount, view.Seats.Count);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithInactiveMember_SendsNothingToTheInactiveMember()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.Beto, SampleMatches.InactiveEva };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);

            Assert.False(_listener.HasNotified(SampleMatches.InactiveEva.EntrantId));
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithGuest_MarksTheGuestSeat()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.CaroGuest };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchSeat guestSeat = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[1];
            Assert.True(guestSeat.IsGuest);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithGuest_KeepsTheRoomColor()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.CaroGuest };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchSeat guestSeat = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[1];
            Assert.Equal(SeatColor.Red, guestSeat.Color);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTwoMembers_GivesTheFirstMemberSeatOne()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.Beto };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);

            Assert.Equal(SeatNumberOne, _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).ViewerSeatNumber);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTwoMembers_GivesTheSecondMemberSeatTwo()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.Beto };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);

            Assert.Equal(SeatNumberTwo, _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).ViewerSeatNumber);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTwoMembers_OpensTheCaterpillarPlacement()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.CaterpillarPlacement, view.Phase);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTwoMembers_GivesThePlacementToSeatOne()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SeatNumberOne, view.ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTwoMembers_GivesTheWholeStepTime()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.StepTime, view.StepTimeLeft);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTwoMembers_OffersTheFiveFlowers()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(FiveFlowers, view.PlacementTargets.Count);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithRepeatedEntrant_ThrowsInvalidOperationException()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.Ana };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules));
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarOnFreeFlower_ReturnsApplied()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(MatchActionResult.Applied, placementResult);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarOnFreeFlower_PassesPlacementToNextSeat()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Equal(SeatNumberTwo, view.ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarOnFreeFlower_ShowsTheCaterpillar()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Single(view.Caterpillars);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarOnFreeFlower_OffersTheFourFreeFlowers()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Equal(FourFreeFlowers, view.PlacementTargets.Count);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarOutOfTurn_ReturnsNotYourTurn()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(MatchActionResult.NotYourTurn, placementResult);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarOnGround_ReturnsNotAFlower()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

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
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            _fakeTimeProvider.AdjustTime(_fakeTimeProvider.GetUtcNow() + SampleMatches.StepTime);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(MatchActionResult.TimeExpired, placementResult);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarOnActiveSeat_GivesNextSeatFullPlacementTime()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);

            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(SampleMatches.StepTime, _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).StepTimeLeft);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarInOneMatch_KeepsThePlacementOfTheOtherMatch()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.CaroAndDani), SampleMatches.TimeLimits, SampleMatches.Rules);

            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            MatchView otherMatchView = _listener.FindLatestViewFor(SampleMatches.Dani.EntrantId);
            Assert.Equal(SeatNumberOne, otherMatchView.ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarInOneMatch_LeavesTheOtherBoardWithoutCaterpillars()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.CaroAndDani), SampleMatches.TimeLimits, SampleMatches.Rules);

            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            MatchView otherMatchView = _listener.FindLatestViewFor(SampleMatches.Dani.EntrantId);
            Assert.Empty(otherMatchView.Caterpillars);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarOnActiveSeat_SendsChangeAfterStart()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(new RecordedNoticeKind[] { RecordedNoticeKind.MatchStarted, RecordedNoticeKind.MatchStateChanged }, _listener.NoticeKinds);
        }

        [Fact]
        public async Task Test_ExpirePlacement_AtPlacementDeadline_SendsOneChangeNotice()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            Assert.Equal(StartAndChangeNotices, _listener.NoticeCount);
        }

        [Fact]
        public async Task Test_ExpirePlacement_AtPlacementDeadline_PassesPlacementToNextSeat()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SeatNumberTwo, view.ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_ExpirePlacement_AtPlacementDeadline_PlacesNoCaterpillar()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Empty(view.Caterpillars);
        }

        [Fact]
        public async Task Test_ExpirePlacement_JustBeforePlacementDeadline_DoesNotRun()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            _fakeTimeProvider.Advance(SampleMatches.JustBeforeDeadline);

            Assert.Equal(StartNoticeOnly, _listener.NoticeCount);
        }

        [Fact]
        public async Task Test_ExpirePlacement_AtFirstDeadlineAfterPlacing_KeepsNextSeatPlacing()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaBetoAndDani), SampleMatches.TimeLimits, SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            _fakeTimeProvider.Advance(SampleMatches.RestOfFirstPlacement);

            Assert.Equal(SeatNumberTwo, _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_ExpirePlacement_OfLastSeat_OpensTheButterflyPlacement()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaBetoAndDani), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Dani.EntrantId);
            Assert.Equal(MatchPhase.ButterflyPlacement, view.Phase);
        }

        [Fact]
        public async Task Test_ExpirePlacement_OfLastSeat_GivesButterflyToLastPlacer()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaBetoAndDani), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Dani.EntrantId);
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
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);

            Assert.Equal(MatchActionResult.WrongPhase, placementResult);
        }

        [Fact]
        public async Task Test_PlacePiece_ButterflyByLastPlacer_ReturnsApplied()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);

            Assert.Equal(MatchActionResult.Applied, placementResult);
        }

        [Fact]
        public async Task Test_PlacePiece_ButterflyByLastPlacer_StartsThePlayerTurn()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);

            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.PlayerTurn, view.Phase);
        }

        [Fact]
        public async Task Test_PlacePiece_ButterflyByLastPlacer_GivesTheTurnToSeatOne()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);

            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SeatNumberOne, view.ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_PlacePiece_ButterflyByLastPlacer_ShowsTheButterfly()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);

            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.True(view.HasButterfly);
        }

        [Fact]
        public async Task Test_PlacePiece_ButterflyByLastPlacer_PutsTheButterflyOnTheChosenRow()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);

            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.CenterFlower.Row, view.ButterflyCell.Row);
        }

        [Fact]
        public async Task Test_PlacePiece_ButterflyByLastPlacer_PutsTheButterflyOnTheChosenColumn()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);

            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.CenterFlower.Column, view.ButterflyCell.Column);
        }

        [Fact]
        public async Task Test_PlacePiece_ButterflyByLastPlacer_StartsFirstTurnWithTheWholeTurnTime()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);

            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.StepTime, view.StepTimeLeft);
        }

        [Fact]
        public async Task Test_TurnTime_RunningOut_KeepsThePlayerTurn()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.PlayerTurn, view.Phase);
        }

        [Fact]
        public async Task Test_TurnTime_RunningOut_PassesTheTurnToSeatTwo()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SeatNumberTwo, view.ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_TurnTime_RunningOut_GivesTheWholeTurnTime()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.StepTime, view.StepTimeLeft);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOut_FinishesTheMatchOnce()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            Assert.Equal(OneFinish, _listener.FinishCount);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOut_SendsTheFinishedPhase()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            Assert.Equal(MatchPhase.Finished, _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).Phase);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOut_ReleasesBothPlayers()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            Assert.Equal(new int[] { SampleMatches.Ana.EntrantId, SampleMatches.Beto.EntrantId }, _listener.ReleasedEntrantIds.Order());
        }

        [Fact]
        public async Task Test_PlacePiece_AfterTheMatchFinished_ReturnsNotInMatch()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.OneTurnPerRoundRules);
            AdvanceUntilFinished();

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(MatchActionResult.NotInMatch, placementResult);
        }

        [Fact]
        public async Task Test_StepTime_AfterTheMatchFinished_SendsNoNotice()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.OneTurnPerRoundRules);
            AdvanceUntilFinished();
            int noticeCountAtFinish = _listener.NoticeCount;

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            Assert.Equal(noticeCountAtFinish, _listener.NoticeCount);
        }

        [Fact]
        public async Task Test_PlacePiece_InANewMatchAfterTheLastFinished_ReturnsApplied()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.OneTurnPerRoundRules);
            AdvanceUntilFinished();
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(MatchActionResult.Applied, placementResult);
        }

        [Fact]
        public async Task Test_Forfeit_WithTwoSeats_ReturnsForfeited()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            ForfeitResult forfeitResult = _coordinator.Forfeit(SampleMatches.Beto.EntrantId);

            Assert.Equal(ForfeitResult.Forfeited, forfeitResult);
        }

        [Fact]
        public async Task Test_StartMatchAsync_AfterAForfeitWithTwoSeats_StartsANewMatch()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.Forfeit(SampleMatches.Beto.EntrantId);

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.CaterpillarPlacement, view.Phase);
        }

        [Fact]
        public async Task Test_Forfeit_WithTwoSeatsDuringPlacement_StopsPlacementCountdown()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.Forfeit(SampleMatches.Ana.EntrantId);
            int noticeCountAtEnd = _listener.NoticeCount;

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            Assert.Equal(noticeCountAtEnd, _listener.NoticeCount);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarAfterTheOtherSeatForfeited_ReturnsNotInMatch()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.Forfeit(SampleMatches.Beto.EntrantId);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(MatchActionResult.NotInMatch, placementResult);
        }

        [Fact]
        public void Test_Forfeit_WithoutMatch_ReturnsNotInMatch()
        {
            ForfeitResult forfeitResult = _coordinator.Forfeit(SampleMatches.Ana.EntrantId);

            Assert.Equal(ForfeitResult.NotInMatch, forfeitResult);
        }

        [Fact]
        public void Test_BoardContains_WithRowEight_ReturnsFalse()
        {
            MatchCell outsideCell = SampleMatches.OutsideCell;

            bool isOnBoard = MatchCoordinator.BoardContains(outsideCell);

            Assert.False(isOnBoard);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarInTurn_ReturnsApplied()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);

            MatchActionResult actionResult = _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.SouthWestFlower);

            Assert.Equal(MatchActionResult.Applied, actionResult);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarInTurn_ChargesItsCost()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);

            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.SouthWestFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(PointsLeftAfterCaterpillarPlacement, view.ActionPointsLeft);
        }

        [Fact]
        public async Task Test_PlacePiece_CaterpillarInTurn_KeepsTheTurnClockRunning()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);

            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.SouthWestFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.RestOfFirstPlacement, view.StepTimeLeft);
        }

        [Fact]
        public async Task Test_MoveCaterpillar_ToGroundNeighbor_ReturnsApplied()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            var move = new MatchMove { Origin = SampleMatches.NorthWestFlower, Destination = SampleMatches.SouthOfNorthWest };

            MatchActionResult actionResult = _coordinator.MoveCaterpillar(SampleMatches.Ana.EntrantId, move);

            Assert.Equal(MatchActionResult.Applied, actionResult);
        }

        [Fact]
        public async Task Test_MoveCaterpillar_ToGroundNeighbor_ShowsItOnTheDestination()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            var move = new MatchMove { Origin = SampleMatches.NorthWestFlower, Destination = SampleMatches.SouthOfNorthWest };

            _coordinator.MoveCaterpillar(SampleMatches.Ana.EntrantId, move);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            bool hasCaterpillarOnDestination = SampleMatches.HasCaterpillarOn(view, SampleMatches.SouthOfNorthWest);
            Assert.True(hasCaterpillarOnDestination);
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
        public async Task Test_AddFlowerLevel_OnFreeFlower_ReturnsApplied()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            MatchActionResult actionResult = _coordinator.AddFlowerLevel(SampleMatches.Ana.EntrantId, SampleMatches.SouthWestFlower);

            Assert.Equal(MatchActionResult.Applied, actionResult);
        }

        [Fact]
        public async Task Test_AddFlowerLevel_OnFreeFlower_ShowsTheHigherFlower()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.AddFlowerLevel(SampleMatches.Ana.EntrantId, SampleMatches.SouthWestFlower);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            int flowerLevel = SampleMatches.FindFlowerLevel(view, SampleMatches.SouthWestFlower);
            Assert.Equal(LevelTwo, flowerLevel);
        }

        [Fact]
        public async Task Test_AddFlowerLevel_AboveAPlanter_ReturnsApplied()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            MatchActionResult actionResult = _coordinator.AddFlowerLevel(SampleMatches.Ana.EntrantId, SampleMatches.NorthOfNorthEast);

            Assert.Equal(MatchActionResult.Applied, actionResult);
        }

        [Fact]
        public async Task Test_AddFlowerLevel_AboveAPlanter_KeepsTheNorthWestPlanterNumber()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.AddFlowerLevel(SampleMatches.Ana.EntrantId, SampleMatches.NorthOfNorthEast);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(NorthWestPlanterNumber, SampleMatches.FindPlanterNumber(view, SampleMatches.NorthWestFlower));
        }

        [Fact]
        public async Task Test_AddFlowerLevel_AboveAPlanter_NumbersTheNewLevelWithItsPlanter()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.AddFlowerLevel(SampleMatches.Ana.EntrantId, SampleMatches.NorthOfNorthEast);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(NorthEastPlanterNumber, SampleMatches.FindPlanterNumber(view, SampleMatches.NorthOfNorthEast));
        }

        [Fact]
        public async Task Test_AddFlowerLevel_AboveAPlanter_KeepsTheNorthEastPlanterNumber()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.AddFlowerLevel(SampleMatches.Ana.EntrantId, SampleMatches.NorthOfNorthEast);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(NorthEastPlanterNumber, SampleMatches.FindPlanterNumber(view, SampleMatches.NorthEastFlower));
        }

        [Fact]
        public async Task Test_EndTurn_BySeatOne_ReturnsApplied()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);

            MatchActionResult actionResult = _coordinator.EndTurn(SampleMatches.Ana.EntrantId);

            Assert.Equal(MatchActionResult.Applied, actionResult);
        }

        [Fact]
        public async Task Test_EndTurn_BySeatOne_GivesTheTurnToSeatTwo()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);

            _coordinator.EndTurn(SampleMatches.Ana.EntrantId);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SeatNumberTwo, view.ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_EndTurn_BySeatOne_GivesSeatTwoTheWholeTurnTime()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);

            _coordinator.EndTurn(SampleMatches.Ana.EntrantId);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.StepTime, view.StepTimeLeft);
        }

        [Fact]
        public void Test_EndTurn_WithoutMatch_ReturnsNotInMatch()
        {
            MatchActionResult actionResult = _coordinator.EndTurn(SampleMatches.Ana.EntrantId);

            Assert.Equal(MatchActionResult.NotInMatch, actionResult);
        }

        [Fact]
        public async Task Test_PlaceButterfly_ByLastPlacer_SendsMoveTargetsToThePlayingSeat()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.NotEmpty(anaView.MoveTargets);
        }

        [Fact]
        public async Task Test_PlaceButterfly_ByLastPlacer_SendsNoMoveTargetsToTheWaitingSeat()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Empty(betoView.MoveTargets);
        }

        [Fact]
        public async Task Test_PlaceButterfly_ByLastPlacer_LeavesFourCaterpillarsToEachSeat()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(new int[] { FourCaterpillarsLeft, FourCaterpillarsLeft }, anaView.Seats.Select(seat => seat.CaterpillarsLeft));
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOut_SendsTheRanking()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(TwoSeatCount, view.RankedSeatNumbers.Count);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOut_MarksTheMatchAsNotAbandoned()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.False(view.WasAbandoned);
        }

        [Fact]
        public async Task Test_DrawCard_OnOwnTurn_ReturnsApplied()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            MatchActionResult drawResult = _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            Assert.Equal(MatchActionResult.Applied, drawResult);
        }

        [Fact]
        public async Task Test_DrawCard_OnOwnTurn_ShowsTheCardInTheOwnerHand()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Single(anaView.Hand);
        }

        [Fact]
        public async Task Test_DrawCard_OnOwnTurn_ShowsSevenCardsLeftInTheDeck()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SevenCardsLeft, anaView.DeckCount);
        }

        [Fact]
        public async Task Test_DrawCard_OnOwnTurn_ShowsFourActionPointsLeft()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(FourActionPointsLeft, anaView.ActionPointsLeft);
        }

        [Fact]
        public async Task Test_DrawCard_OnOwnTurn_HidesTheCardFromTheOtherPlayer()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Empty(betoView.Hand);
        }

        [Fact]
        public async Task Test_DrawCard_OnOwnTurn_ShowsTheOtherPlayerTheHandCount()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Equal(OneCardInHand, betoView.Seats[0].HandCount);
        }

        [Fact]
        public async Task Test_DrawCard_OnOwnTurn_MarksEveryCardInHandPlayable()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(anaView.Hand, anaView.PlayableCards);
        }

        [Fact]
        public async Task Test_DrawCard_OnOwnTurn_LetsTheActivePlayerDrawAgain()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.True(anaView.CanDrawCard);
        }

        [Fact]
        public async Task Test_DrawCard_OnOwnTurn_DoesNotLetTheOtherPlayerDraw()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.False(betoView.CanDrawCard);
        }

        [Fact]
        public async Task Test_DrawCard_OnOwnTurn_OffersNoPlayableCardToTheOtherPlayer()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
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
        public async Task Test_StartMatchAsync_WithCardCosts_SendsTheCardDrawCost()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchActionCosts actionCosts = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).ActionCosts;
            Assert.Equal(SampleMatches.Rules.ActionPointCosts.CardDraw, actionCosts.CardDraw);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithCardCosts_SendsTheCardPlayCost()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchActionCosts actionCosts = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).ActionCosts;
            Assert.Equal(SampleMatches.Rules.ActionPointCosts.CardPlay, actionCosts.CardPlay);
        }

        private async Task StartPlayedOpeningAsync(MatchRuleSet ruleSet)
        {
            await StartPlayedOpeningAsync(SampleMatches.AnaAndBeto, SampleMatches.TimeLimits, ruleSet);
        }

        private async Task StartPlayedOpeningAsync(IReadOnlyList<MatchEntrant> twoEntrants, MatchTimeLimits timeLimits, MatchRuleSet ruleSet)
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, twoEntrants), timeLimits, ruleSet);
            _coordinator.PlacePiece(twoEntrants[0].EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(twoEntrants[1].EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);
            _coordinator.PlacePiece(twoEntrants[1].EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithGuest_RecordsOneParticipant()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.CaroGuest };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);

            Assert.Single(_participantRepository.LastSeatedPlayers);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithGuest_RecordsTheRoom()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.CaroGuest };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);

            Assert.Equal(SampleMatches.RoomId, _recordRepository.LastRoomId);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithGuest_RecordsBothSeats()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.CaroGuest };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);

            Assert.Equal(TwoSeatCount, _recordRepository.LastPlayerCount);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithGuest_RecordsOnlyThePlayerWithAccount()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.CaroGuest };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchParticipant seatedPlayer = _participantRepository.LastSeatedPlayers.Single();
            Assert.Equal(SampleMatches.Ana.EntrantId, seatedPlayer.PlayerId);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithGuest_RecordsTheSeatOfThePlayerWithAccount()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.CaroGuest };

            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchParticipant seatedPlayer = _participantRepository.LastSeatedPlayers.Single();
            Assert.Equal(SeatNumberOne, seatedPlayer.SeatNumber);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTwoMembers_ReturnsTheRecordedMatchId()
        {
            int matchId = await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            Assert.Equal(_recordRepository.LastMatchId, matchId);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOut_RecordsTheMatchAsCompleted()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            RecordedFinish recordedFinish = await _recordRepository.FirstFinish.WaitAsync(_recordWaitLimit);
            Assert.Equal(MatchEndReasonCodes.Completed, recordedFinish.EndReason);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOut_RecordsBothPositions()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            Assert.Equal(new short?[] { FirstPosition, SecondPosition }, playerResults.Select(playerResult => playerResult.FinalPosition).Order());
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOut_RecordsEveryFinalScore()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            Assert.DoesNotContain(null, playerResults.Select(playerResult => playerResult.FinalScore));
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOut_RecordsNoWithdrawal()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.OneTurnPerRoundRules);

            AdvanceUntilFinished();

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            Assert.DoesNotContain(true, playerResults.Select(playerResult => playerResult.WasWithdrawn));
        }

        [Fact]
        public async Task Test_StartMatchAsync_WhenEveryStepTimesOutWithAKeptCard_ShowsTheCardPoint()
        {
            await StartPlayedOpeningAsync(SampleMatches.OneTurnPerRoundRules);
            _coordinator.DrawCard(SampleMatches.Ana.EntrantId);

            AdvanceUntilFinished();

            MatchSeat anaSeat = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[0];
            Assert.Equal(OneCardPoint, anaSeat.CardPoints);
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
            Assert.Equal(anaSeat.FinalScore, anaResult.FinalScore);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithEntrantAlreadySeated_ThrowsInvalidOperationException()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules));
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithEntrantAlreadySeated_WritesNoSecondMatch()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            await Record.ExceptionAsync(() => _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules));

            Assert.Equal(OneRecordedMatch, _recordRepository.LastMatchId);
        }

        [Fact]
        public async Task Test_Forfeit_WithTwoSeats_RecordsTheMatchAsAbandoned()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            _coordinator.Forfeit(SampleMatches.Ana.EntrantId);

            RecordedFinish recordedFinish = await _recordRepository.FirstFinish.WaitAsync(_recordWaitLimit);
            Assert.Equal(MatchEndReasonCodes.Abandoned, recordedFinish.EndReason);
        }

        [Fact]
        public async Task Test_Forfeit_WithTwoSeats_RecordsTheWithdrawal()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            _coordinator.Forfeit(SampleMatches.Ana.EntrantId);

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            MatchParticipant anaResult = playerResults.Single(playerResult => playerResult.PlayerId == SampleMatches.Ana.EntrantId);
            Assert.True(anaResult.WasWithdrawn);
        }

        [Fact]
        public async Task Test_Forfeit_WithTwoSeats_RecordsTheForfeitingPlayerLast()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            _coordinator.Forfeit(SampleMatches.Ana.EntrantId);

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            MatchParticipant anaResult = playerResults.Single(playerResult => playerResult.PlayerId == SampleMatches.Ana.EntrantId);
            Assert.Equal(SecondPosition, anaResult.FinalPosition);
        }

        [Fact]
        public async Task Test_Forfeit_WithTwoSeats_RecordsTheOtherPlayerAsNotWithdrawn()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            _coordinator.Forfeit(SampleMatches.Ana.EntrantId);

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            MatchParticipant betoResult = playerResults.Single(playerResult => playerResult.PlayerId == SampleMatches.Beto.EntrantId);
            Assert.False(betoResult.WasWithdrawn);
        }

        [Fact]
        public async Task Test_Forfeit_WithTwoSeats_RecordsAbandonedMatchWithTheOtherPlayerFirst()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            _coordinator.Forfeit(SampleMatches.Ana.EntrantId);

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            MatchParticipant betoResult = playerResults.Single(playerResult => playerResult.PlayerId == SampleMatches.Beto.EntrantId);
            Assert.Equal(FirstPosition, betoResult.FinalPosition);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_OnTurnOfOtherSeat_ReturnsTrue()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            bool hasOpenedWindow = _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            Assert.True(hasOpenedWindow);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_OnTurnOfOtherSeat_MarksTheSeatDisconnectedForTheOthers()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            MatchSeat betoSeat = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[SecondSeatIndex];
            Assert.True(betoSeat.IsDisconnected);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_OnTurnOfOtherSeat_KeepsTheSeatInTheMatch()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);

            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            MatchSeat betoSeat = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[SecondSeatIndex];
            Assert.False(betoSeat.IsRetired);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_JustBeforeTheReconnectionTimeRunsOut_KeepsTheSeat()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime - _oneTick);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.False(anaView.Seats[SecondSeatIndex].IsRetired);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_JustBeforeTheReconnectionTimeRunsOut_KeepsThePlayerTurn()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime - _oneTick);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.PlayerTurn, anaView.Phase);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOutWithTwoSeats_RetiresTheSeat()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.True(anaView.Seats[SecondSeatIndex].IsRetired);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOutWithTwoSeats_ClearsTheDisconnectedMark()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.False(anaView.Seats[SecondSeatIndex].IsDisconnected);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOutWithTwoSeats_FinishesTheMatch()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.Finished, anaView.Phase);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOutWithTwoSeats_FinishesTheMatchAsAbandoned()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.True(anaView.WasAbandoned);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOutWithTwoSeats_FinishesItOnce()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            Assert.Equal(OneFinish, _listener.FinishCount);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOutWithTwoSeats_RecordsTheMatchAsAbandoned()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            RecordedFinish recordedFinish = await _recordRepository.FirstFinish.WaitAsync(_recordWaitLimit);
            Assert.Equal(MatchEndReasonCodes.Abandoned, recordedFinish.EndReason);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOutWithTwoSeats_RecordsTheWithdrawal()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            MatchParticipant betoResult = playerResults.Single(playerResult => playerResult.PlayerId == SampleMatches.Beto.EntrantId);
            Assert.True(betoResult.WasWithdrawn);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOutWithTwoSeats_RecordsTheLastPosition()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            MatchParticipant betoResult = playerResults.Single(playerResult => playerResult.PlayerId == SampleMatches.Beto.EntrantId);
            Assert.Equal(SecondPosition, betoResult.FinalPosition);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOutWithThreeSeats_KeepsThePlayerTurn()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.OpenReconnectionWindow(SampleMatches.Dani.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.PlayerTurn, anaView.Phase);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOutWithThreeSeats_KeepsTheTurnOfSeatOne()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.OpenReconnectionWindow(SampleMatches.Dani.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SeatNumberOne, anaView.ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOutWithThreeSeats_RetiresTheSeat()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.OpenReconnectionWindow(SampleMatches.Dani.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.True(anaView.Seats[ThirdSeatIndex].IsRetired);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOutWithThreeSeats_TakesItsCaterpillarsOffTheBoard()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.OpenReconnectionWindow(SampleMatches.Dani.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.DoesNotContain(SeatNumberThree, anaView.Caterpillars.Select(caterpillar => caterpillar.SeatNumber));
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOutWithThreeSeats_DoesNotFinishTheMatch()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.OpenReconnectionWindow(SampleMatches.Dani.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            Assert.Equal(NoFinish, _listener.FinishCount);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOut_LeavesTheSeatDisconnectedInItsLastView()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.OpenReconnectionWindow(SampleMatches.Dani.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            MatchView daniView = _listener.FindLatestViewFor(SampleMatches.Dani.EntrantId);
            Assert.True(daniView.Seats[ThirdSeatIndex].IsDisconnected);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_RunningOut_SendsNoViewToTheRetiredSeat()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.OpenReconnectionWindow(SampleMatches.Dani.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            MatchView daniView = _listener.FindLatestViewFor(SampleMatches.Dani.EntrantId);
            Assert.False(daniView.Seats[ThirdSeatIndex].IsRetired);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_OnOwnTurn_KeepsTheSeatUntilTheTurnRunsOut()
        {
            await StartPlayedOpeningOfThreeAsync();
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);
            _coordinator.OpenReconnectionWindow(SampleMatches.Ana.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.RestOfFirstPlacement - _oneTick);

            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.False(betoView.Seats[0].IsRetired);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_OnOwnTurn_RetiresTheSeatWhenTheTurnRunsOut()
        {
            await StartPlayedOpeningOfThreeAsync();
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);
            _coordinator.OpenReconnectionWindow(SampleMatches.Ana.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.RestOfFirstPlacement);

            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.True(betoView.Seats[0].IsRetired);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_OnOwnTurn_GivesTheTurnToTheNextSeatWhenItRunsOut()
        {
            await StartPlayedOpeningOfThreeAsync();
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);
            _coordinator.OpenReconnectionWindow(SampleMatches.Ana.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.RestOfFirstPlacement);

            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Equal(SeatNumberTwo, betoView.ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_OnOwnTurn_GivesTheNextSeatTheWholeTurnTime()
        {
            await StartPlayedOpeningOfThreeAsync();
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);
            _coordinator.OpenReconnectionWindow(SampleMatches.Ana.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.RestOfFirstPlacement);

            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Equal(SampleMatches.StepTime, betoView.StepTimeLeft);
        }

        [Fact]
        public async Task Test_CloseReconnectionWindow_BeforeItRunsOut_ReturnsTrue()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);
            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime - _oneTick);

            bool isBackInMatch = _coordinator.CloseReconnectionWindow(SampleMatches.Beto.EntrantId);

            Assert.True(isBackInMatch);
        }

        [Fact]
        public async Task Test_ReconnectionTime_RunningOutAfterTheWindowClosed_ShowsTheSeatConnected()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);
            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime - _oneTick);
            _coordinator.CloseReconnectionWindow(SampleMatches.Beto.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            MatchSeat betoSeat = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).Seats[SecondSeatIndex];
            Assert.False(betoSeat.IsDisconnected);
        }

        [Fact]
        public async Task Test_ReconnectionTime_RunningOutAfterTheWindowClosed_KeepsTheSeatInTheMatch()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);
            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime - _oneTick);
            _coordinator.CloseReconnectionWindow(SampleMatches.Beto.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            MatchSeat betoSeat = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).Seats[SecondSeatIndex];
            Assert.False(betoSeat.IsRetired);
        }

        [Fact]
        public async Task Test_CloseReconnectionWindow_AfterTheSeatWasRetired_ReturnsFalse()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.OpenReconnectionWindow(SampleMatches.Dani.EntrantId);
            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            bool isBackInMatch = _coordinator.CloseReconnectionWindow(SampleMatches.Dani.EntrantId);

            Assert.False(isBackInMatch);
        }

        [Fact]
        public async Task Test_DrawCard_AfterReconnectingTooLate_ReturnsNotInMatch()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.OpenReconnectionWindow(SampleMatches.Dani.EntrantId);
            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);
            _coordinator.CloseReconnectionWindow(SampleMatches.Dani.EntrantId);

            MatchActionResult drawResult = _coordinator.DrawCard(SampleMatches.Dani.EntrantId);

            Assert.Equal(MatchActionResult.NotInMatch, drawResult);
        }

        [Fact]
        public async Task Test_CloseReconnectionWindow_AfterTheSeatWasRetired_ShowsTheSeatRetired()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.OpenReconnectionWindow(SampleMatches.Dani.EntrantId);
            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            _coordinator.CloseReconnectionWindow(SampleMatches.Dani.EntrantId);

            MatchView daniView = _listener.FindLatestViewFor(SampleMatches.Dani.EntrantId);
            Assert.True(daniView.Seats[ThirdSeatIndex].IsRetired);
        }

        [Fact]
        public async Task Test_CloseReconnectionWindow_AfterTheSeatWasRetired_ShowsThePlayerItsOwnSeat()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.OpenReconnectionWindow(SampleMatches.Dani.EntrantId);
            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            _coordinator.CloseReconnectionWindow(SampleMatches.Dani.EntrantId);

            MatchView daniView = _listener.FindLatestViewFor(SampleMatches.Dani.EntrantId);
            Assert.Equal(SeatNumberThree, daniView.ViewerSeatNumber);
        }

        [Fact]
        public async Task Test_CloseReconnectionWindow_AfterTheSeatWasRetired_SendsNothingToTheOthers()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.OpenReconnectionWindow(SampleMatches.Dani.EntrantId);
            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);
            MatchView anaViewBefore = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);

            _coordinator.CloseReconnectionWindow(SampleMatches.Dani.EntrantId);

            Assert.Same(anaViewBefore, _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId));
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTimeLimits_SendsTheReconnectionTimeInTheView()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);

            MatchView view = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.ReconnectionTime, view.ReconnectionTime);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_ForBothSeats_FinishesTheMatchOnce()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Ana.EntrantId);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);
            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            Assert.Equal(OneFinish, _listener.FinishCount);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_ForBothSeats_SendsNothingAfterTheFinish()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Ana.EntrantId);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);
            _fakeTimeProvider.Advance(SampleMatches.StepTime);
            int noticeCountAfterFinish = _listener.NoticeCount;

            _fakeTimeProvider.Advance(SampleMatches.StepTime);

            Assert.Equal(noticeCountAfterFinish, _listener.NoticeCount);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_TwiceForTheSameSeat_ReturnsFalse()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);
            _fakeTimeProvider.Advance(_halfReconnectionTime);

            bool hasOpenedWindow = _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            Assert.False(hasOpenedWindow);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_TwiceForTheSameSeat_KeepsTheFirstWindow()
        {
            await StartPlayedOpeningAsync(SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);
            _fakeTimeProvider.Advance(_halfReconnectionTime);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);

            _fakeTimeProvider.Advance(_halfReconnectionTime);

            Assert.True(_listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[SecondSeatIndex].IsRetired);
        }

        [Fact]
        public void Test_OpenReconnectionWindow_WithoutMatch_ReturnsFalse()
        {
            bool hasOpenedWindow = _coordinator.OpenReconnectionWindow(SampleMatches.Ana.EntrantId);

            Assert.False(hasOpenedWindow);
        }

        [Fact]
        public async Task Test_Forfeit_AfterARetirementForTheReconnectionWindow_RecordsBothWithdrawals()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.Beto, SampleMatches.Dani };
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Dani.EntrantId);
            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            _coordinator.Forfeit(SampleMatches.Ana.EntrantId);

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            Assert.Equal(_expectedSeatWithdrawals, playerResults.OrderBy(playerResult => playerResult.SeatNumber).Select(playerResult => playerResult.WasWithdrawn));
        }

        [Fact]
        public async Task Test_Forfeit_OutOfTurnWithThreeSeats_ReturnsForfeited()
        {
            await StartPlayedOpeningOfThreeAsync();

            ForfeitResult forfeitResult = _coordinator.Forfeit(SampleMatches.Dani.EntrantId);

            Assert.Equal(ForfeitResult.Forfeited, forfeitResult);
        }

        [Fact]
        public async Task Test_Forfeit_OutOfTurnWithThreeSeats_KeepsThePlayerTurn()
        {
            await StartPlayedOpeningOfThreeAsync();

            _coordinator.Forfeit(SampleMatches.Dani.EntrantId);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.PlayerTurn, anaView.Phase);
        }

        [Fact]
        public async Task Test_Forfeit_OutOfTurnWithThreeSeats_KeepsTheTurnOfSeatOne()
        {
            await StartPlayedOpeningOfThreeAsync();

            _coordinator.Forfeit(SampleMatches.Dani.EntrantId);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SeatNumberOne, anaView.ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_Forfeit_OutOfTurnWithThreeSeats_RetiresTheSeat()
        {
            await StartPlayedOpeningOfThreeAsync();

            _coordinator.Forfeit(SampleMatches.Dani.EntrantId);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.True(anaView.Seats[ThirdSeatIndex].IsRetired);
        }

        [Fact]
        public async Task Test_Forfeit_OutOfTurnWithThreeSeats_TakesItsCaterpillarsOffTheBoard()
        {
            await StartPlayedOpeningOfThreeAsync();

            _coordinator.Forfeit(SampleMatches.Dani.EntrantId);

            MatchView anaView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.DoesNotContain(SeatNumberThree, anaView.Caterpillars.Select(caterpillar => caterpillar.SeatNumber));
        }

        [Fact]
        public async Task Test_Forfeit_OutOfTurnWithThreeSeats_DoesNotFinishTheMatch()
        {
            await StartPlayedOpeningOfThreeAsync();

            _coordinator.Forfeit(SampleMatches.Dani.EntrantId);

            Assert.Equal(NoFinish, _listener.FinishCount);
        }

        [Fact]
        public async Task Test_Forfeit_OnOwnTurn_RetiresTheSeat()
        {
            await StartPlayedOpeningOfThreeAsync();
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);

            _coordinator.Forfeit(SampleMatches.Ana.EntrantId);

            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.True(betoView.Seats[0].IsRetired);
        }

        [Fact]
        public async Task Test_Forfeit_OnOwnTurn_GivesTheTurnToTheNextSeat()
        {
            await StartPlayedOpeningOfThreeAsync();
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);

            _coordinator.Forfeit(SampleMatches.Ana.EntrantId);

            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Equal(SeatNumberTwo, betoView.ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_Forfeit_OnOwnTurn_GivesTheNextSeatTheWholeTurnTime()
        {
            await StartPlayedOpeningOfThreeAsync();
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);

            _coordinator.Forfeit(SampleMatches.Ana.EntrantId);

            MatchView betoView = _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId);
            Assert.Equal(SampleMatches.StepTime, betoView.StepTimeLeft);
        }

        [Fact]
        public async Task Test_Forfeit_Twice_ReturnsNotInMatch()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.Forfeit(SampleMatches.Dani.EntrantId);

            ForfeitResult secondResult = _coordinator.Forfeit(SampleMatches.Dani.EntrantId);

            Assert.Equal(ForfeitResult.NotInMatch, secondResult);
        }

        [Fact]
        public async Task Test_DrawCard_AfterForfeitingTwice_ReturnsNotInMatch()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.Forfeit(SampleMatches.Dani.EntrantId);
            _coordinator.Forfeit(SampleMatches.Dani.EntrantId);

            MatchActionResult drawResult = _coordinator.DrawCard(SampleMatches.Dani.EntrantId);

            Assert.Equal(MatchActionResult.NotInMatch, drawResult);
        }

        [Fact]
        public async Task Test_Forfeit_DuringTheReconnectionWindow_StopsTheWindow()
        {
            await StartPlayedOpeningOfThreeAsync();
            _coordinator.OpenReconnectionWindow(SampleMatches.Dani.EntrantId);
            _coordinator.Forfeit(SampleMatches.Dani.EntrantId);
            int noticeCountAfterForfeit = _listener.NoticeCount;

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            Assert.Equal(noticeCountAfterForfeit, _listener.NoticeCount);
        }

        private async Task StartPlayedOpeningOfThreeAsync()
        {
            var entrants = new MatchEntrant[] { SampleMatches.Ana, SampleMatches.Beto, SampleMatches.Dani };
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);
            _coordinator.PlacePiece(SampleMatches.Dani.EntrantId, MatchPiece.Caterpillar, SampleMatches.SouthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Dani.EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithOnlyGuests_RecordsTheMatchWithBothSeats()
        {
            var openingOfGuests = new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.CaroAndEliGuests);

            await _coordinator.StartMatchAsync(openingOfGuests, SampleMatches.TimeLimits, SampleMatches.Rules);

            Assert.Equal(TwoSeatCount, _recordRepository.LastPlayerCount);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithOnlyGuests_RecordsNoParticipant()
        {
            var openingOfGuests = new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.CaroAndEliGuests);

            await _coordinator.StartMatchAsync(openingOfGuests, SampleMatches.TimeLimits, SampleMatches.Rules);

            Assert.Empty(_participantRepository.LastSeatedPlayers);
        }

        [Fact]
        public async Task Test_PlacePiece_AfterComingBackDuringOwnPlacement_PlacesTheCaterpillar()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);
            _coordinator.OpenReconnectionWindow(SampleMatches.Ana.EntrantId);
            _coordinator.CloseReconnectionWindow(SampleMatches.Ana.EntrantId);

            MatchActionResult placementResult = _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);

            Assert.Equal(MatchActionResult.Applied, placementResult);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_DuringOwnPlacementUntilItRunsOut_RetiresTheSeat()
        {
            await StartMatchWithAnaDisconnectedDuringHerPlacementAsync();

            _fakeTimeProvider.Advance(SampleMatches.RestOfFirstPlacement);

            Assert.True(_listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).Seats[0].IsRetired);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_DuringOwnPlacementUntilItRunsOut_KeepsTheCaterpillarPlacementGoing()
        {
            await StartMatchWithAnaDisconnectedDuringHerPlacementAsync();

            _fakeTimeProvider.Advance(SampleMatches.RestOfFirstPlacement);

            Assert.Equal(MatchPhase.CaterpillarPlacement, _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).Phase);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_DuringOwnPlacementUntilItRunsOut_PassesThePlacementToSeatTwo()
        {
            await StartMatchWithAnaDisconnectedDuringHerPlacementAsync();

            _fakeTimeProvider.Advance(SampleMatches.RestOfFirstPlacement);

            Assert.Equal(SeatNumberTwo, _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_CloseReconnectionWindow_OfAGuestBeforeItRunsOut_ReturnsTrue()
        {
            await StartGuestMatchWithCaroDisconnectedAsync();

            bool isBackInMatch = _coordinator.CloseReconnectionWindow(SampleMatches.CaroGuest.EntrantId);

            Assert.True(isBackInMatch);
        }

        [Fact]
        public async Task Test_CloseReconnectionWindow_OfAGuestBeforeItRunsOut_ClearsTheDisconnectedMark()
        {
            await StartGuestMatchWithCaroDisconnectedAsync();

            _coordinator.CloseReconnectionWindow(SampleMatches.CaroGuest.EntrantId);

            Assert.False(_listener.FindLatestViewFor(SampleMatches.Dani.EntrantId).Seats[0].IsDisconnected);
        }

        [Fact]
        public async Task Test_CloseReconnectionWindow_OfAGuestBeforeItRunsOut_KeepsTheGuestSeat()
        {
            await StartGuestMatchWithCaroDisconnectedAsync();

            _coordinator.CloseReconnectionWindow(SampleMatches.CaroGuest.EntrantId);

            Assert.False(_listener.FindLatestViewFor(SampleMatches.Dani.EntrantId).Seats[0].IsRetired);
        }

        [Fact]
        public async Task Test_CloseReconnectionWindow_AfterItsTurnRanOut_ReturnsTrue()
        {
            await PlayBetoTurnOutWhileDisconnectedAsync();

            bool isBackInMatch = _coordinator.CloseReconnectionWindow(SampleMatches.Beto.EntrantId);

            Assert.True(isBackInMatch);
        }

        [Fact]
        public async Task Test_CloseReconnectionWindow_AfterItsTurnRanOut_LeavesTheTurnToSeatOne()
        {
            await PlayBetoTurnOutWhileDisconnectedAsync();

            _coordinator.CloseReconnectionWindow(SampleMatches.Beto.EntrantId);

            Assert.Equal(SeatNumberOne, _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_CloseReconnectionWindow_AfterItsTurnRanOut_KeepsTheSeat()
        {
            await PlayBetoTurnOutWhileDisconnectedAsync();

            _coordinator.CloseReconnectionWindow(SampleMatches.Beto.EntrantId);

            Assert.False(_listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).Seats[SecondSeatIndex].IsRetired);
        }

        [Fact]
        public async Task Test_CloseReconnectionWindow_AfterItsTurnRanOut_ClearsTheDisconnectedMark()
        {
            await PlayBetoTurnOutWhileDisconnectedAsync();

            _coordinator.CloseReconnectionWindow(SampleMatches.Beto.EntrantId);

            Assert.False(_listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).Seats[SecondSeatIndex].IsDisconnected);
        }

        [Fact]
        public async Task Test_Forfeit_OfTheButterflyPlacer_KeepsTheButterflyPlacementOpen()
        {
            await PlayRoundOneLeavingDaniLowestAsync();

            _coordinator.Forfeit(SampleMatches.Dani.EntrantId);

            Assert.Equal(MatchPhase.ButterflyPlacement, _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Phase);
        }

        [Fact]
        public async Task Test_Forfeit_OfTheButterflyPlacer_GivesTheButterflyToTheNextLowestScore()
        {
            await PlayRoundOneLeavingDaniLowestAsync();

            _coordinator.Forfeit(SampleMatches.Dani.EntrantId);

            Assert.Equal(SeatNumberTwo, _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_Forfeit_OfAGuest_ReturnsForfeited()
        {
            await StartPlayedOpeningAsync(SampleMatches.AnaAndCaroGuest, SampleMatches.TimeLimits, SampleMatches.Rules);

            ForfeitResult forfeitResult = _coordinator.Forfeit(SampleMatches.CaroGuest.EntrantId);

            Assert.Equal(ForfeitResult.Forfeited, forfeitResult);
        }

        [Fact]
        public async Task Test_Forfeit_OfAGuest_RecordsOnlyThePlayerWithAccount()
        {
            await StartPlayedOpeningAsync(SampleMatches.AnaAndCaroGuest, SampleMatches.TimeLimits, SampleMatches.Rules);

            _coordinator.Forfeit(SampleMatches.CaroGuest.EntrantId);

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            Assert.Equal(new int[] { SampleMatches.Ana.EntrantId }, playerResults.Select(playerResult => playerResult.PlayerId));
        }

        private async Task StartMatchWithAnaDisconnectedDuringHerPlacementAsync()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaBetoAndDani), SampleMatches.TimeLimits, SampleMatches.Rules);
            _fakeTimeProvider.Advance(SampleMatches.FirstPlacementTaken);
            _coordinator.OpenReconnectionWindow(SampleMatches.Ana.EntrantId);
        }

        private async Task StartGuestMatchWithCaroDisconnectedAsync()
        {
            await StartPlayedOpeningAsync(SampleMatches.CaroAndDani, SampleMatches.TimeLimits, SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.CaroGuest.EntrantId);
            _fakeTimeProvider.Advance(_halfReconnectionTime);
        }

        private async Task PlayBetoTurnOutWhileDisconnectedAsync()
        {
            await StartPlayedOpeningAsync(SampleMatches.AnaAndBeto, _reconnectionLongerThanTheTurnLimits, SampleMatches.Rules);
            _coordinator.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);
            _coordinator.EndTurn(SampleMatches.Ana.EntrantId);
            _fakeTimeProvider.Advance(SampleMatches.StepTime);
        }

        private async Task PlayRoundOneLeavingDaniLowestAsync()
        {
            await _coordinator.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaBetoAndDani), SampleMatches.TimeLimits, SampleMatches.OneTurnPerRoundRules);
            _coordinator.PlacePiece(SampleMatches.Ana.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Beto.EntrantId, MatchPiece.Caterpillar, SampleMatches.NorthEastFlower);
            _coordinator.PlacePiece(SampleMatches.Dani.EntrantId, MatchPiece.Caterpillar, SampleMatches.SouthWestFlower);
            _coordinator.PlacePiece(SampleMatches.Dani.EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);
            _coordinator.AddFlowerLevel(SampleMatches.Ana.EntrantId, SampleMatches.SouthOfNorthWest);
            _coordinator.AddFlowerLevel(SampleMatches.Ana.EntrantId, SampleMatches.EastOfNorthWest);
            _coordinator.EndTurn(SampleMatches.Ana.EntrantId);
            _coordinator.AddFlowerLevel(SampleMatches.Beto.EntrantId, SampleMatches.NorthOfNorthEast);
            _coordinator.EndTurn(SampleMatches.Beto.EntrantId);
            _coordinator.EndTurn(SampleMatches.Dani.EntrantId);
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
