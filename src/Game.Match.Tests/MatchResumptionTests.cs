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
    public sealed class MatchResumptionTests
    {
        private const int UnknownMatchId = 9;
        private const int FullTurnActionPoints = 5;
        private const string BrokenStateDocument = "<MatchCheckpoint>roto</MatchCheckpoint>";

        private static readonly TimeSpan _resumeTime = TimeSpan.FromMinutes(3);
        private static readonly TimeSpan _partOfTheTurn = TimeSpan.FromSeconds(30);

        private static readonly MatchResumption _resumption = new MatchResumption(
            SampleMatches.TimeLimits,
            new Dictionary<int, MatchRuleSet> { [SampleMatches.TwoSeatCount] = SampleMatches.Rules, [SampleMatches.ThreeSeatCount] = SampleMatches.Rules },
            _resumeTime);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly RecordingMatchStateRepository _firstRunStateRepository = new RecordingMatchStateRepository();
        private readonly RecordingMatchListener _listener = new RecordingMatchListener();
        private readonly RecordingMatchRecordRepository _recordRepository = new RecordingMatchRecordRepository();
        private readonly RecordingMatchParticipantRepository _participantRepository = new RecordingMatchParticipantRepository();
        private readonly RecordingMatchStateRepository _stateRepository = new RecordingMatchStateRepository();
        private readonly MatchCoordinator _firstRun;
        private readonly MatchCoordinator _coordinator;

        public MatchResumptionTests()
        {
            var firstRunRepositories = new MatchRepositories(new RecordingMatchRecordRepository(), new RecordingMatchParticipantRepository(), _firstRunStateRepository);
            _firstRun = new MatchCoordinator(_fakeTimeProvider, new RecordingMatchListener(), firstRunRepositories, SampleMatches.FiveFlowerCells, new Random(SampleMatches.ChanceSeed));
            var repositories = new MatchRepositories(_recordRepository, _participantRepository, _stateRepository);
            _coordinator = new MatchCoordinator(_fakeTimeProvider, _listener, repositories, SampleMatches.FiveFlowerCells, new Random(SampleMatches.ChanceSeed));
        }

        [Fact]
        public async Task Test_StartMatchAsync_SavesTheFirstPlacementWithTheRoomCode()
        {
            await _firstRun.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            await _firstRun.FinishRecordingAsync();

            MatchState savedState = Assert.Single(_firstRunStateRepository.SavedStates);
            Assert.Equal(SampleMatches.SeatNumberOne, savedState.ActiveSeatNumber);
            Assert.Contains(SampleMatches.RoomCode, savedState.StateDocument, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithSavedState_KeepsBothSeatsWaiting()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);

            IReadOnlyList<int> waitingEntrantIds = await RestartAsync();

            Assert.Equal(new int[] { SampleMatches.Ana.EntrantId, SampleMatches.Beto.EntrantId }, waitingEntrantIds);
            MatchSeating? seating = _coordinator.FindSeating(SampleMatches.Ana.EntrantId);
            Assert.NotNull(seating);
            Assert.True(seating.IsAwaitingPlayers);
            Assert.Equal(SampleMatches.RoomCode, seating.RoomCode);
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithGuest_ListsTheGuestIdentifier()
        {
            await PlayOpeningAsync(SampleMatches.CaroAndDani);

            IReadOnlyList<int> waitingEntrantIds = await RestartAsync();

            Assert.Contains(SampleMatches.CaroGuest.EntrantId, waitingEntrantIds);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithOnePlayerBack_ShowsTheWaitWithTheOtherAbsent()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();

            ReturnToMatchResult returnResult = _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            Assert.Equal(ReturnToMatchResult.Returned, returnResult);
            MatchView waitingView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.AwaitingPlayers, waitingView.Phase);
            Assert.Equal(_resumeTime, waitingView.StepTimeLeft);
            Assert.False(waitingView.Seats[0].IsDisconnected);
            Assert.True(waitingView.Seats[1].IsDisconnected);
            Assert.False(_listener.HasNotified(SampleMatches.Beto.EntrantId));
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithTwoPlayersBack_PlaysTheInterruptedTurnAgainWhole()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            _firstRun.DrawCard(SampleMatches.Ana.EntrantId);
            _fakeTimeProvider.Advance(_partOfTheTurn);
            await RestartAsync();

            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);
            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            MatchView resumedView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.PlayerTurn, resumedView.Phase);
            Assert.Equal(SampleMatches.SeatNumberOne, resumedView.ActiveSeatNumber);
            Assert.Equal(SampleMatches.StepTime, resumedView.StepTimeLeft);
            Assert.Equal(FullTurnActionPoints, resumedView.ActionPointsLeft);
            Assert.Empty(resumedView.Hand);
            Assert.False(_coordinator.FindSeating(SampleMatches.Ana.EntrantId)?.IsAwaitingPlayers);
        }

        [Fact]
        public async Task Test_ReturnToMatch_AfterAPlanterGrewUpwards_KeepsEveryPlanterNumber()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            _firstRun.AddFlowerLevel(SampleMatches.Ana.EntrantId, SampleMatches.NorthOfNorthEast);
            _firstRun.EndTurn(SampleMatches.Ana.EntrantId);
            await RestartAsync();

            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);
            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            MatchView resumedView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.NorthWestPlanterNumber, SampleMatches.FindPlanterNumber(resumedView, SampleMatches.NorthWestFlower));
            Assert.Equal(SampleMatches.NorthEastPlanterNumber, SampleMatches.FindPlanterNumber(resumedView, SampleMatches.NorthOfNorthEast));
            Assert.Equal(SampleMatches.NorthEastPlanterNumber, SampleMatches.FindPlanterNumber(resumedView, SampleMatches.NorthEastFlower));
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithTwoOfThreeBack_GivesTheThirdAReconnectionWindow()
        {
            await PlayOpeningAsync(SampleMatches.AnaBetoAndDani);
            await RestartAsync();

            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);
            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);
            ReturnToMatchResult lateReturn = _coordinator.ReturnToMatch(SampleMatches.Dani.EntrantId);

            Assert.Equal(ReturnToMatchResult.Returned, lateReturn);
            Assert.False(_listener.FindLatestViewFor(SampleMatches.Dani.EntrantId).Seats[2].IsDisconnected);
        }

        [Fact]
        public async Task Test_ResumedMatch_WithThirdPlayerNeverBack_RetiresHimWhenHisWindowRunsOut()
        {
            await PlayOpeningAsync(SampleMatches.AnaBetoAndDani);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);
            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            Assert.True(_listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[2].IsDisconnected);
            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            Assert.True(_listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[2].IsRetired);
        }

        [Fact]
        public async Task Test_OpenReconnectionWindow_WhileWaiting_CountsThePlayerAsAbsentAgain()
        {
            await PlayOpeningAsync(SampleMatches.AnaBetoAndDani);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _coordinator.OpenReconnectionWindow(SampleMatches.Ana.EntrantId);
            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            Assert.Equal(MatchPhase.AwaitingPlayers, _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).Phase);
        }

        [Fact]
        public async Task Test_ResumeTime_WithOnePlayerBack_FinishesTheMatchAbandonedWithHimFirst()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _fakeTimeProvider.Advance(_resumeTime);

            RecordedFinish recordedFinish = await _recordRepository.FirstFinish.WaitAsync(SampleMatches.RecordWaitLimit);
            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(SampleMatches.RecordWaitLimit);
            Assert.Equal(MatchEndReasonCodes.Abandoned, recordedFinish.EndReason);
            Assert.Equal(SampleMatches.FirstPosition, playerResults.Single(result => result.PlayerId == SampleMatches.Ana.EntrantId).FinalPosition);
            Assert.True(playerResults.Single(result => result.PlayerId == SampleMatches.Beto.EntrantId).WasWithdrawn);
            MatchView finishedView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.Finished, finishedView.Phase);
            Assert.True(finishedView.WasAbandoned);
        }

        [Fact]
        public async Task Test_ResumeTime_WithNobodyBack_RecordsTheMatchInterruptedWithoutResults()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();

            _fakeTimeProvider.Advance(_resumeTime);

            RecordedFinish recordedFinish = await _recordRepository.FirstFinish.WaitAsync(SampleMatches.RecordWaitLimit);
            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(SampleMatches.RecordWaitLimit);
            Assert.Equal(MatchEndReasonCodes.Interrupted, recordedFinish.EndReason);
            Assert.Empty(playerResults);
            Assert.Null(_coordinator.FindSeating(SampleMatches.Ana.EntrantId));
        }

        [Fact]
        public async Task Test_Forfeit_WhileWaitingWithTwoSeats_FinishesTheMatch()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();

            ForfeitResult forfeitResult = _coordinator.Forfeit(SampleMatches.Beto.EntrantId);

            Assert.Equal(ForfeitResult.Forfeited, forfeitResult);
            RecordedFinish recordedFinish = await _recordRepository.FirstFinish.WaitAsync(SampleMatches.RecordWaitLimit);
            Assert.Equal(MatchEndReasonCodes.Abandoned, recordedFinish.EndReason);
            Assert.Equal(ReturnToMatchResult.NoMatchToResume, _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId));
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithSeatForfeitedBeforeTheCrash_KeepsItRetired()
        {
            await PlayOpeningAsync(SampleMatches.AnaBetoAndDani);
            _firstRun.Forfeit(SampleMatches.Dani.EntrantId);
            await RestartAsync();

            ReturnToMatchResult retiredReturn = _coordinator.ReturnToMatch(SampleMatches.Dani.EntrantId);
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            Assert.Equal(ReturnToMatchResult.NoMatchToResume, retiredReturn);
            Assert.Null(_coordinator.FindSeating(SampleMatches.Dani.EntrantId));
            Assert.True(_listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[2].IsRetired);
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithoutSavedState_RecordsTheMatchInterrupted()
        {
            _recordRepository.InProgressMatchIds.Add(UnknownMatchId);

            IReadOnlyList<int> waitingEntrantIds = await _coordinator.RestoreInterruptedMatchesAsync(_resumption);

            RecordedFinish recordedFinish = await _recordRepository.FirstFinish.WaitAsync(SampleMatches.RecordWaitLimit);
            Assert.Empty(waitingEntrantIds);
            Assert.Equal(new RecordedFinish(UnknownMatchId, MatchEndReasonCodes.Interrupted), recordedFinish);
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithUnreadableState_RecordsTheMatchInterruptedAndDropsTheState()
        {
            _recordRepository.InProgressMatchIds.Add(UnknownMatchId);
            _stateRepository.StoredStates.Add(new MatchState
            {
                MatchId = UnknownMatchId,
                RoundNumber = 1,
                TurnNumber = 1,
                ActiveSeatNumber = 1,
                StateDocument = BrokenStateDocument,
            });

            IReadOnlyList<int> waitingEntrantIds = await _coordinator.RestoreInterruptedMatchesAsync(_resumption);
            await _coordinator.FinishRecordingAsync();

            Assert.Empty(waitingEntrantIds);
            Assert.Equal(MatchEndReasonCodes.Interrupted, (await _recordRepository.FirstFinish).EndReason);
            Assert.Equal(new int[] { UnknownMatchId }, _stateRepository.DeletedMatchIds);
        }

        [Fact]
        public async Task Test_MatchFinish_AfterResuming_DropsTheSavedState()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);
            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            _coordinator.Forfeit(SampleMatches.Beto.EntrantId);
            await _coordinator.FinishRecordingAsync();

            Assert.Contains(_recordRepository.InProgressMatchIds[0], _stateRepository.DeletedMatchIds);
        }

        private async Task PlayOpeningAsync(IReadOnlyList<MatchEntrant> entrants)
        {
            await _firstRun.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);
            IReadOnlyList<MatchCell> caterpillarCells = new MatchCell[] { SampleMatches.NorthWestFlower, SampleMatches.NorthEastFlower, SampleMatches.SouthWestFlower };
            for (var seatIndex = 0; seatIndex < entrants.Count; seatIndex++)
            {
                _firstRun.PlacePiece(entrants[seatIndex].EntrantId, MatchPiece.Caterpillar, caterpillarCells[seatIndex]);
            }

            _firstRun.PlacePiece(entrants[^1].EntrantId, MatchPiece.Butterfly, SampleMatches.CenterFlower);
        }

        private async Task<IReadOnlyList<int>> RestartAsync()
        {
            await _firstRun.FinishRecordingAsync();
            MatchState lastSavedState = _firstRunStateRepository.SavedStates[^1];
            _stateRepository.StoredStates.Add(lastSavedState);
            _recordRepository.InProgressMatchIds.Add(lastSavedState.MatchId);
            IReadOnlyList<int> waitingEntrantIds = await _coordinator.RestoreInterruptedMatchesAsync(_resumption);

            return waitingEntrantIds;
        }
    }
}
