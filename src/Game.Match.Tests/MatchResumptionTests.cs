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
        private const int SeatNumberOne = 1;
        private const int FirstRoundNumber = 1;
        private const int FirstTurnNumber = 1;
        private const int TwoSeatCount = 2;
        private const int ThirdSeatIndex = 2;
        private const int ThreeSeatCount = 3;
        private const int UnknownMatchId = 9;
        private const short FirstPosition = 1;
        private const int FullTurnActionPoints = 5;
        private const int NorthWestPlanterNumber = 1;
        private const int NorthEastPlanterNumber = 2;
        private const int NewGuestId = -9;
        private const string BrokenStateDocument = "<MatchCheckpoint>roto</MatchCheckpoint>";

        private static readonly TimeSpan _recordWaitLimit = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan _resumeTime = TimeSpan.FromMinutes(3);
        private static readonly TimeSpan _partOfTheTurn = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan _halfReconnectionTime = SampleMatches.ReconnectionTime / 2;

        private static readonly MatchResumption _resumption = new MatchResumption(
            SampleMatches.TimeLimits,
            new Dictionary<int, MatchRuleSet> { [TwoSeatCount] = SampleMatches.Rules, [ThreeSeatCount] = SampleMatches.Rules },
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
        public async Task Test_StartMatchAsync_WithTwoMembers_SavesOneState()
        {
            await _firstRun.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            await _firstRun.FinishRecordingAsync();

            Assert.Single(_firstRunStateRepository.SavedStates);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTwoMembers_SavesTheFirstPlacement()
        {
            await _firstRun.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            await _firstRun.FinishRecordingAsync();

            MatchState savedState = _firstRunStateRepository.SavedStates.Single();
            Assert.Equal(SeatNumberOne, savedState.ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithTwoMembers_SavesTheRoomCode()
        {
            await _firstRun.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, SampleMatches.AnaAndBeto), SampleMatches.TimeLimits, SampleMatches.Rules);
            await _firstRun.FinishRecordingAsync();

            MatchState savedState = _firstRunStateRepository.SavedStates.Single();
            Assert.Contains(SampleMatches.RoomCode, savedState.StateDocument, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithSavedState_KeepsBothSeatsWaiting()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);

            IReadOnlyList<int> waitingEntrantIds = await RestartAsync();

            Assert.Equal(new int[] { SampleMatches.Ana.EntrantId, SampleMatches.Beto.EntrantId }, waitingEntrantIds);
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithSavedState_KeepsTheSeating()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);

            await RestartAsync();

            MatchSeating? seating = _coordinator.FindSeating(SampleMatches.Ana.EntrantId);
            Assert.NotNull(seating);
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithSavedState_MarksTheSeatingAsAwaitingPlayers()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);

            await RestartAsync();

            MatchSeating? seating = _coordinator.FindSeating(SampleMatches.Ana.EntrantId);
            Assert.True(seating?.IsAwaitingPlayers);
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithSavedState_KeepsTheRoomCode()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);

            await RestartAsync();

            MatchSeating? seating = _coordinator.FindSeating(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.RoomCode, seating?.RoomCode);
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithGuest_ListsTheGuestIdentifier()
        {
            await PlayOpeningAsync(SampleMatches.CaroAndDani);

            IReadOnlyList<int> waitingEntrantIds = await RestartAsync();

            Assert.Contains(SampleMatches.CaroGuest.EntrantId, waitingEntrantIds);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithOnePlayerBack_ReturnsReturned()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();

            ReturnToMatchResult returnResult = _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            Assert.Equal(ReturnToMatchResult.Returned, returnResult);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithOnePlayerBack_ShowsTheWait()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();

            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            MatchView waitingView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.AwaitingPlayers, waitingView.Phase);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithOnePlayerBack_GivesTheWholeResumeTime()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();

            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            MatchView waitingView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(_resumeTime, waitingView.StepTimeLeft);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithOnePlayerBack_ShowsThePlayerBackConnected()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();

            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            MatchView waitingView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.False(waitingView.Seats[0].IsDisconnected);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithOnePlayerBack_ShowsTheWaitWithTheOtherAbsent()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();

            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            MatchView waitingView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.True(waitingView.Seats[1].IsDisconnected);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithOnePlayerBack_SendsNothingToTheAbsentPlayer()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();

            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            Assert.False(_listener.HasNotified(SampleMatches.Beto.EntrantId));
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithTwoPlayersBack_ResumesThePlayerTurn()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            _firstRun.DrawCard(SampleMatches.Ana.EntrantId);
            _fakeTimeProvider.Advance(_partOfTheTurn);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            MatchView resumedView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.PlayerTurn, resumedView.Phase);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithTwoPlayersBack_GivesTheTurnBackToSeatOne()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            _firstRun.DrawCard(SampleMatches.Ana.EntrantId);
            _fakeTimeProvider.Advance(_partOfTheTurn);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            MatchView resumedView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SeatNumberOne, resumedView.ActiveSeatNumber);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithTwoPlayersBack_GivesTheWholeTurnTime()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            _firstRun.DrawCard(SampleMatches.Ana.EntrantId);
            _fakeTimeProvider.Advance(_partOfTheTurn);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            MatchView resumedView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(SampleMatches.StepTime, resumedView.StepTimeLeft);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithTwoPlayersBack_GivesTheWholeActionPoints()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            _firstRun.DrawCard(SampleMatches.Ana.EntrantId);
            _fakeTimeProvider.Advance(_partOfTheTurn);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            MatchView resumedView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(FullTurnActionPoints, resumedView.ActionPointsLeft);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithTwoPlayersBack_UndoesTheCardDrawn()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            _firstRun.DrawCard(SampleMatches.Ana.EntrantId);
            _fakeTimeProvider.Advance(_partOfTheTurn);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            MatchView resumedView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Empty(resumedView.Hand);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithTwoPlayersBack_StopsWaitingForPlayers()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            _firstRun.DrawCard(SampleMatches.Ana.EntrantId);
            _fakeTimeProvider.Advance(_partOfTheTurn);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            Assert.False(_coordinator.FindSeating(SampleMatches.Ana.EntrantId)?.IsAwaitingPlayers);
        }

        [Fact]
        public async Task Test_ReturnToMatch_AfterAPlanterGrewUpwards_KeepsTheNorthWestPlanterNumber()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            _firstRun.AddFlowerLevel(SampleMatches.Ana.EntrantId, SampleMatches.NorthOfNorthEast);
            _firstRun.EndTurn(SampleMatches.Ana.EntrantId);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            MatchView resumedView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(NorthWestPlanterNumber, SampleMatches.FindPlanterNumber(resumedView, SampleMatches.NorthWestFlower));
        }

        [Fact]
        public async Task Test_ReturnToMatch_AfterAPlanterGrewUpwards_KeepsTheNewLevelInItsPlanter()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            _firstRun.AddFlowerLevel(SampleMatches.Ana.EntrantId, SampleMatches.NorthOfNorthEast);
            _firstRun.EndTurn(SampleMatches.Ana.EntrantId);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            MatchView resumedView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(NorthEastPlanterNumber, SampleMatches.FindPlanterNumber(resumedView, SampleMatches.NorthOfNorthEast));
        }

        [Fact]
        public async Task Test_ReturnToMatch_AfterAPlanterGrewUpwards_KeepsTheNorthEastPlanterNumber()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            _firstRun.AddFlowerLevel(SampleMatches.Ana.EntrantId, SampleMatches.NorthOfNorthEast);
            _firstRun.EndTurn(SampleMatches.Ana.EntrantId);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            MatchView resumedView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(NorthEastPlanterNumber, SampleMatches.FindPlanterNumber(resumedView, SampleMatches.NorthEastFlower));
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithTwoOfThreeBack_LetsTheThirdReturnLate()
        {
            await PlayOpeningAsync(SampleMatches.AnaBetoAndDani);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);
            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            ReturnToMatchResult lateReturn = _coordinator.ReturnToMatch(SampleMatches.Dani.EntrantId);

            Assert.Equal(ReturnToMatchResult.Returned, lateReturn);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithTwoOfThreeBack_ShowsTheThirdConnectedAgain()
        {
            await PlayOpeningAsync(SampleMatches.AnaBetoAndDani);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);
            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            _coordinator.ReturnToMatch(SampleMatches.Dani.EntrantId);

            Assert.False(_listener.FindLatestViewFor(SampleMatches.Dani.EntrantId).Seats[ThirdSeatIndex].IsDisconnected);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithTwoOfThreeBack_ShowsTheThirdDisconnected()
        {
            await PlayOpeningAsync(SampleMatches.AnaBetoAndDani);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            Assert.True(_listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[ThirdSeatIndex].IsDisconnected);
        }

        [Fact]
        public async Task Test_ResumedMatch_WithThirdPlayerNeverBack_RetiresHimWhenHisWindowRunsOut()
        {
            await PlayOpeningAsync(SampleMatches.AnaBetoAndDani);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);
            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);

            Assert.True(_listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[ThirdSeatIndex].IsRetired);
        }

        [Fact]
        public async Task Test_ReturnToMatch_AfterTheOtherLeftWhileWaiting_KeepsWaitingForPlayers()
        {
            await PlayOpeningAsync(SampleMatches.AnaBetoAndDani);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);
            _coordinator.OpenReconnectionWindow(SampleMatches.Ana.EntrantId);

            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            Assert.Equal(MatchPhase.AwaitingPlayers, _listener.FindLatestViewFor(SampleMatches.Beto.EntrantId).Phase);
        }

        [Fact]
        public async Task Test_ResumeTime_WithOnePlayerBack_RecordsTheMatchAsAbandoned()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _fakeTimeProvider.Advance(_resumeTime);

            RecordedFinish recordedFinish = await _recordRepository.FirstFinish.WaitAsync(_recordWaitLimit);
            Assert.Equal(MatchEndReasonCodes.Abandoned, recordedFinish.EndReason);
        }

        [Fact]
        public async Task Test_ResumeTime_WithOnePlayerBack_RecordsThePlayerBackFirst()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _fakeTimeProvider.Advance(_resumeTime);

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            MatchParticipant anaResult = playerResults.Single(playerResult => playerResult.PlayerId == SampleMatches.Ana.EntrantId);
            Assert.Equal(FirstPosition, anaResult.FinalPosition);
        }

        [Fact]
        public async Task Test_ResumeTime_WithOnePlayerBack_RecordsTheAbsentPlayerAsWithdrawn()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _fakeTimeProvider.Advance(_resumeTime);

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            MatchParticipant betoResult = playerResults.Single(playerResult => playerResult.PlayerId == SampleMatches.Beto.EntrantId);
            Assert.True(betoResult.WasWithdrawn);
        }

        [Fact]
        public async Task Test_ResumeTime_WithOnePlayerBack_FinishesTheMatch()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _fakeTimeProvider.Advance(_resumeTime);

            MatchView finishedView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.Equal(MatchPhase.Finished, finishedView.Phase);
        }

        [Fact]
        public async Task Test_ResumeTime_WithOnePlayerBack_ShowsTheMatchAsAbandoned()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            _fakeTimeProvider.Advance(_resumeTime);

            MatchView finishedView = _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId);
            Assert.True(finishedView.WasAbandoned);
        }

        [Fact]
        public async Task Test_ResumeTime_WithNobodyBack_RecordsTheMatchInterrupted()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();

            _fakeTimeProvider.Advance(_resumeTime);

            RecordedFinish recordedFinish = await _recordRepository.FirstFinish.WaitAsync(_recordWaitLimit);
            Assert.Equal(MatchEndReasonCodes.Interrupted, recordedFinish.EndReason);
        }

        [Fact]
        public async Task Test_ResumeTime_WithNobodyBack_RecordsNoResults()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();

            _fakeTimeProvider.Advance(_resumeTime);

            IReadOnlyList<MatchParticipant> playerResults = await _participantRepository.FirstResults.WaitAsync(_recordWaitLimit);
            Assert.Empty(playerResults);
        }

        [Fact]
        public async Task Test_ResumeTime_WithNobodyBack_DropsTheSeating()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();

            _fakeTimeProvider.Advance(_resumeTime);

            Assert.Null(_coordinator.FindSeating(SampleMatches.Ana.EntrantId));
        }

        [Fact]
        public async Task Test_Forfeit_WhileWaitingWithTwoSeats_ReturnsForfeited()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();

            ForfeitResult forfeitResult = _coordinator.Forfeit(SampleMatches.Beto.EntrantId);

            Assert.Equal(ForfeitResult.Forfeited, forfeitResult);
        }

        [Fact]
        public async Task Test_Forfeit_WhileWaitingWithTwoSeats_RecordsTheMatchAsAbandoned()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();

            _coordinator.Forfeit(SampleMatches.Beto.EntrantId);

            RecordedFinish recordedFinish = await _recordRepository.FirstFinish.WaitAsync(_recordWaitLimit);
            Assert.Equal(MatchEndReasonCodes.Abandoned, recordedFinish.EndReason);
        }

        [Fact]
        public async Task Test_ReturnToMatch_AfterAForfeitWhileWaitingWithTwoSeats_ReturnsNoMatchToResume()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            await RestartAsync();
            _coordinator.Forfeit(SampleMatches.Beto.EntrantId);

            ReturnToMatchResult returnResult = _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            Assert.Equal(ReturnToMatchResult.NoMatchToResume, returnResult);
        }

        [Fact]
        public async Task Test_ReturnToMatch_OfSeatForfeitedBeforeTheCrash_ReturnsNoMatchToResume()
        {
            await PlayOpeningAsync(SampleMatches.AnaBetoAndDani);
            _firstRun.Forfeit(SampleMatches.Dani.EntrantId);
            await RestartAsync();

            ReturnToMatchResult retiredReturn = _coordinator.ReturnToMatch(SampleMatches.Dani.EntrantId);

            Assert.Equal(ReturnToMatchResult.NoMatchToResume, retiredReturn);
        }

        [Fact]
        public async Task Test_ReturnToMatch_OfSeatForfeitedBeforeTheCrash_LeavesItWithoutSeating()
        {
            await PlayOpeningAsync(SampleMatches.AnaBetoAndDani);
            _firstRun.Forfeit(SampleMatches.Dani.EntrantId);
            await RestartAsync();

            _coordinator.ReturnToMatch(SampleMatches.Dani.EntrantId);

            Assert.Null(_coordinator.FindSeating(SampleMatches.Dani.EntrantId));
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithSeatForfeitedBeforeTheCrash_KeepsItRetired()
        {
            await PlayOpeningAsync(SampleMatches.AnaBetoAndDani);
            _firstRun.Forfeit(SampleMatches.Dani.EntrantId);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Dani.EntrantId);

            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);

            Assert.True(_listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[ThirdSeatIndex].IsRetired);
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithoutSavedState_LeavesNobodyWaiting()
        {
            _recordRepository.InProgressMatchIds.Add(UnknownMatchId);

            IReadOnlyList<int> waitingEntrantIds = await _coordinator.RestoreInterruptedMatchesAsync(_resumption);

            Assert.Empty(waitingEntrantIds);
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithoutSavedState_RecordsTheMatchInterrupted()
        {
            _recordRepository.InProgressMatchIds.Add(UnknownMatchId);

            await _coordinator.RestoreInterruptedMatchesAsync(_resumption);

            RecordedFinish recordedFinish = await _recordRepository.FirstFinish.WaitAsync(_recordWaitLimit);
            Assert.Equal(new RecordedFinish(UnknownMatchId, MatchEndReasonCodes.Interrupted), recordedFinish);
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithUnreadableState_LeavesNobodyWaiting()
        {
            _recordRepository.InProgressMatchIds.Add(UnknownMatchId);
            _stateRepository.StoredStates.Add(new MatchState { MatchId = UnknownMatchId, RoundNumber = FirstRoundNumber, TurnNumber = FirstTurnNumber, ActiveSeatNumber = SeatNumberOne, StateDocument = BrokenStateDocument });

            IReadOnlyList<int> waitingEntrantIds = await _coordinator.RestoreInterruptedMatchesAsync(_resumption);

            Assert.Empty(waitingEntrantIds);
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithUnreadableState_RecordsTheMatchInterrupted()
        {
            _recordRepository.InProgressMatchIds.Add(UnknownMatchId);
            _stateRepository.StoredStates.Add(new MatchState { MatchId = UnknownMatchId, RoundNumber = FirstRoundNumber, TurnNumber = FirstTurnNumber, ActiveSeatNumber = SeatNumberOne, StateDocument = BrokenStateDocument });

            await _coordinator.RestoreInterruptedMatchesAsync(_resumption);

            RecordedFinish recordedFinish = await _recordRepository.FirstFinish.WaitAsync(_recordWaitLimit);
            Assert.Equal(MatchEndReasonCodes.Interrupted, recordedFinish.EndReason);
        }

        [Fact]
        public async Task Test_RestoreInterruptedMatchesAsync_WithUnreadableState_DropsTheState()
        {
            _recordRepository.InProgressMatchIds.Add(UnknownMatchId);
            _stateRepository.StoredStates.Add(new MatchState { MatchId = UnknownMatchId, RoundNumber = FirstRoundNumber, TurnNumber = FirstTurnNumber, ActiveSeatNumber = SeatNumberOne, StateDocument = BrokenStateDocument });

            await _coordinator.RestoreInterruptedMatchesAsync(_resumption);
            await _coordinator.FinishRecordingAsync();

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

        [Fact]
        public async Task Test_ReturnToMatch_AfterACrashDuringTheDisconnection_ReturnsReturned()
        {
            await RestartWithBetoDisconnectedBeforeTheCrashAsync();

            ReturnToMatchResult lateReturn = _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            Assert.Equal(ReturnToMatchResult.Returned, lateReturn);
        }

        [Fact]
        public async Task Test_ReturnToMatch_AfterACrashDuringTheDisconnection_ResumesTheTurns()
        {
            await RestartWithBetoDisconnectedBeforeTheCrashAsync();

            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            Assert.Equal(MatchPhase.PlayerTurn, _listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Phase);
        }

        [Fact]
        public async Task Test_ReturnToMatch_AfterACrashDuringTheDisconnection_KeepsTheSeatPastTheReconnectionTime()
        {
            await RestartWithBetoDisconnectedBeforeTheCrashAsync();

            _coordinator.ReturnToMatch(SampleMatches.Beto.EntrantId);

            Assert.False(_listener.FindLatestViewFor(SampleMatches.Ana.EntrantId).Seats[1].IsRetired);
        }

        [Fact]
        public async Task Test_ReturnToMatch_OfAGuestAfterTheRestart_ReturnsReturned()
        {
            await RestartGuestMatchWithDaniBackAsync();

            ReturnToMatchResult guestReturn = _coordinator.ReturnToMatch(SampleMatches.CaroGuest.EntrantId);

            Assert.Equal(ReturnToMatchResult.Returned, guestReturn);
        }

        [Fact]
        public async Task Test_ReturnToMatch_OfAGuestAfterTheRestart_ResumesTheTurns()
        {
            await RestartGuestMatchWithDaniBackAsync();

            _coordinator.ReturnToMatch(SampleMatches.CaroGuest.EntrantId);

            Assert.Equal(MatchPhase.PlayerTurn, _listener.FindLatestViewFor(SampleMatches.CaroGuest.EntrantId).Phase);
        }

        [Fact]
        public async Task Test_ReturnToMatch_OfAGuestAfterTheRestart_GivesTheGuestItsOwnSeat()
        {
            await RestartGuestMatchWithDaniBackAsync();

            _coordinator.ReturnToMatch(SampleMatches.CaroGuest.EntrantId);

            Assert.Equal(SeatNumberOne, _listener.FindLatestViewFor(SampleMatches.CaroGuest.EntrantId).ViewerSeatNumber);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithAnotherGuestIdentifier_ReturnsNoMatchToResume()
        {
            await RestartGuestMatchWithDaniBackAsync();

            ReturnToMatchResult guestReturn = _coordinator.ReturnToMatch(NewGuestId);

            Assert.Equal(ReturnToMatchResult.NoMatchToResume, guestReturn);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithAnotherGuestIdentifier_KeepsTheMatchWaiting()
        {
            await RestartGuestMatchWithDaniBackAsync();

            _coordinator.ReturnToMatch(NewGuestId);

            Assert.Equal(MatchPhase.AwaitingPlayers, _listener.FindLatestViewFor(SampleMatches.Dani.EntrantId).Phase);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithTwoGuestsBack_ReturnsReturned()
        {
            await RestartGuestsOnlyMatchWithCaroBackAsync();

            ReturnToMatchResult secondReturn = _coordinator.ReturnToMatch(SampleMatches.EliGuest.EntrantId);

            Assert.Equal(ReturnToMatchResult.Returned, secondReturn);
        }

        [Fact]
        public async Task Test_ReturnToMatch_WithTwoGuestsBack_ResumesTheTurns()
        {
            await RestartGuestsOnlyMatchWithCaroBackAsync();

            _coordinator.ReturnToMatch(SampleMatches.EliGuest.EntrantId);

            Assert.Equal(MatchPhase.PlayerTurn, _listener.FindLatestViewFor(SampleMatches.EliGuest.EntrantId).Phase);
        }

        private async Task RestartWithBetoDisconnectedBeforeTheCrashAsync()
        {
            await PlayOpeningAsync(SampleMatches.AnaAndBeto);
            _firstRun.OpenReconnectionWindow(SampleMatches.Beto.EntrantId);
            _fakeTimeProvider.Advance(_halfReconnectionTime);
            await RestartAsync();
            _fakeTimeProvider.Advance(SampleMatches.ReconnectionTime);
            _coordinator.ReturnToMatch(SampleMatches.Ana.EntrantId);
        }

        private async Task RestartGuestMatchWithDaniBackAsync()
        {
            await PlayOpeningAsync(SampleMatches.CaroAndDani);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.Dani.EntrantId);
        }

        private async Task RestartGuestsOnlyMatchWithCaroBackAsync()
        {
            await PlayOpeningAsync(SampleMatches.CaroAndEliGuests);
            await RestartAsync();
            _coordinator.ReturnToMatch(SampleMatches.CaroGuest.EntrantId);
        }

        private async Task PlayOpeningAsync(IReadOnlyList<MatchEntrant> entrants)
        {
            await _firstRun.StartMatchAsync(new MatchOpening(SampleMatches.RoomId, SampleMatches.RoomCode, entrants), SampleMatches.TimeLimits, SampleMatches.Rules);
            MatchCell[] caterpillarCells = new MatchCell[] { SampleMatches.NorthWestFlower, SampleMatches.NorthEastFlower, SampleMatches.SouthWestFlower };
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
