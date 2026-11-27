using System;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Persistence;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class HistoryServiceTests
    {
        private const int MatchesPerPage = 5;
        private const int FirstPage = 1;
        private const int PlayedMatchId = 1;
        private const int OtherPlayersMatchId = 2;
        private const short TwoPlayers = 2;
        private const short FirstSeat = 1;
        private const short SecondSeat = 2;
        private const short FirstPosition = 1;
        private const short SecondPosition = 2;
        private const int WinnerScore = 30;
        private const int RunnerUpScore = 20;
        private const int OneGuest = 1;

        private static readonly DateTime _startedAt = new DateTime(2026, 11, 20, 18, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime _finishedAt = _startedAt.AddMinutes(25);

        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly RecordingMatchParticipantRepository _participantRepository = new RecordingMatchParticipantRepository();
        private readonly MatchParticipant _anaAsWinner = new MatchParticipant { MatchId = PlayedMatchId, PlayerId = SamplePlayers.AnaId, SeatNumber = FirstSeat, FinalPosition = FirstPosition, FinalScore = WinnerScore, WasWithdrawn = false };
        private readonly MatchParticipant _betoAsRunnerUp = new MatchParticipant { MatchId = PlayedMatchId, PlayerId = SamplePlayers.BetoId, SeatNumber = SecondSeat, FinalPosition = SecondPosition, FinalScore = RunnerUpScore, WasWithdrawn = false };
        private readonly MatchParticipant _unknownPlayerAsRunnerUp = new MatchParticipant { MatchId = PlayedMatchId, PlayerId = SamplePlayers.UnknownPlayerId, SeatNumber = SecondSeat, FinalPosition = SecondPosition, FinalScore = RunnerUpScore, WasWithdrawn = false };
        private readonly MatchParticipant _anaAsWithdrawnRunnerUp = new MatchParticipant { MatchId = PlayedMatchId, PlayerId = SamplePlayers.AnaId, SeatNumber = FirstSeat, FinalPosition = SecondPosition, FinalScore = RunnerUpScore, WasWithdrawn = true };
        private readonly MatchParticipant _betoAsWinner = new MatchParticipant { MatchId = PlayedMatchId, PlayerId = SamplePlayers.BetoId, SeatNumber = SecondSeat, FinalPosition = FirstPosition, FinalScore = WinnerScore, WasWithdrawn = false };
        private readonly MatchParticipant _anaWithoutResult = new MatchParticipant { MatchId = PlayedMatchId, PlayerId = SamplePlayers.AnaId, SeatNumber = FirstSeat, FinalPosition = null, FinalScore = null, WasWithdrawn = false };
        private readonly MatchParticipant _betoAsOtherMatchWinner = new MatchParticipant { MatchId = OtherPlayersMatchId, PlayerId = SamplePlayers.BetoId, SeatNumber = FirstSeat, FinalPosition = FirstPosition, FinalScore = WinnerScore, WasWithdrawn = false };
        private readonly MatchParticipant _daniAsOtherMatchRunnerUp = new MatchParticipant { MatchId = OtherPlayersMatchId, PlayerId = SamplePlayers.DaniId, SeatNumber = SecondSeat, FinalPosition = SecondPosition, FinalScore = RunnerUpScore, WasWithdrawn = false };
        private readonly RecordingMatchRecordRepository _recordRepository;
        private readonly HistoryService _historyService;

        public HistoryServiceTests()
        {
            _recordRepository = new RecordingMatchRecordRepository(_participantRepository);
            _historyService = new HistoryService(_callerSessions.Registry, _recordRepository, _participantRepository, new RecordingPlayerRepository(), new HistoryPaging(MatchesPerPage));
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public async Task Test_GetMatchHistoryAsync_WithOneFinishedMatch_ReturnsItsEntry()
        {
            StoreMatch(PlayedMatchId, MatchEndReasonCodes.Completed);
            _participantRepository.Participants.Add(_anaAsWinner);
            _participantRepository.Participants.Add(_betoAsRunnerUp);

            MatchHistoryResponse history = await _historyService.GetMatchHistoryAsync(FirstPage);

            Assert.Equal(new int[] { PlayedMatchId }, history.Entries.Select(entry => entry.MatchId));
        }

        [Fact]
        public async Task Test_GetMatchHistoryAsync_WithOneFinishedMatch_NamesTheRival()
        {
            StoreMatch(PlayedMatchId, MatchEndReasonCodes.Completed);
            _participantRepository.Participants.Add(_anaAsWinner);
            _participantRepository.Participants.Add(_betoAsRunnerUp);

            MatchHistoryResponse history = await _historyService.GetMatchHistoryAsync(FirstPage);

            Assert.Equal(new string[] { SamplePlayers.BetoUsername }, history.Entries[0].RivalUsernames);
        }

        [Fact]
        public async Task Test_GetMatchHistoryAsync_WithoutFinishedMatches_ReturnsNoEntries()
        {
            MatchHistoryResponse history = await _historyService.GetMatchHistoryAsync(FirstPage);

            Assert.Empty(history.Entries);
        }

        [Fact]
        public async Task Test_GetMatchHistoryAsync_WithADeletedRival_LeavesTheRivalOut()
        {
            StoreMatch(PlayedMatchId, MatchEndReasonCodes.Completed);
            _participantRepository.Participants.Add(_anaAsWinner);
            _participantRepository.Participants.Add(_unknownPlayerAsRunnerUp);

            MatchHistoryResponse history = await _historyService.GetMatchHistoryAsync(FirstPage);

            Assert.Empty(history.Entries[0].RivalUsernames);
        }

        [Fact]
        public async Task Test_GetMatchHistoryAsync_AfterBeingWithdrawn_MarksTheEntryWithdrawn()
        {
            StoreMatch(PlayedMatchId, MatchEndReasonCodes.Abandoned);
            _participantRepository.Participants.Add(_anaAsWithdrawnRunnerUp);
            _participantRepository.Participants.Add(_betoAsWinner);

            MatchHistoryResponse history = await _historyService.GetMatchHistoryAsync(FirstPage);

            Assert.True(history.Entries[0].WasWithdrawn);
        }

        [Fact]
        public async Task Test_GetMatchDetailAsync_OfOwnMatch_ReturnsFound()
        {
            StoreMatch(PlayedMatchId, MatchEndReasonCodes.Completed);
            _participantRepository.Participants.Add(_anaAsWinner);
            _participantRepository.Participants.Add(_betoAsRunnerUp);

            MatchDetailResponse detail = await _historyService.GetMatchDetailAsync(PlayedMatchId);

            Assert.Equal(MatchDetailStatus.Found, detail.Status);
        }

        [Fact]
        public async Task Test_GetMatchDetailAsync_OfOwnMatch_ListsTheResultsByPlacement()
        {
            StoreMatch(PlayedMatchId, MatchEndReasonCodes.Completed);
            _participantRepository.Participants.Add(_betoAsRunnerUp);
            _participantRepository.Participants.Add(_anaAsWinner);

            MatchDetailResponse detail = await _historyService.GetMatchDetailAsync(PlayedMatchId);

            Assert.Equal(new string[] { SamplePlayers.AnaUsername, SamplePlayers.BetoUsername }, detail.Results.Select(result => result.Username));
        }

        [Fact]
        public async Task Test_GetMatchDetailAsync_OfInterruptedMatch_ReturnsInterruptedOutcome()
        {
            StoreMatch(PlayedMatchId, MatchEndReasonCodes.Interrupted);
            _participantRepository.Participants.Add(_anaWithoutResult);

            MatchDetailResponse detail = await _historyService.GetMatchDetailAsync(PlayedMatchId);

            Assert.Equal(MatchOutcome.Interrupted, detail.Outcome);
        }

        [Fact]
        public async Task Test_GetMatchDetailAsync_OfInterruptedMatch_ListsNoResults()
        {
            StoreMatch(PlayedMatchId, MatchEndReasonCodes.Interrupted);
            _participantRepository.Participants.Add(_anaWithoutResult);

            MatchDetailResponse detail = await _historyService.GetMatchDetailAsync(PlayedMatchId);

            Assert.Empty(detail.Results);
        }

        [Fact]
        public async Task Test_GetMatchDetailAsync_WithAGuestRival_CountsTheGuest()
        {
            StoreMatch(PlayedMatchId, MatchEndReasonCodes.Completed);
            _participantRepository.Participants.Add(_anaAsWinner);

            MatchDetailResponse detail = await _historyService.GetMatchDetailAsync(PlayedMatchId);

            Assert.Equal(OneGuest, detail.GuestCount);
        }

        [Fact]
        public async Task Test_GetMatchDetailAsync_OfAnotherPlayersMatch_ReturnsNotYours()
        {
            StoreMatch(OtherPlayersMatchId, MatchEndReasonCodes.Completed);
            _participantRepository.Participants.Add(_betoAsOtherMatchWinner);
            _participantRepository.Participants.Add(_daniAsOtherMatchRunnerUp);

            MatchDetailResponse detail = await _historyService.GetMatchDetailAsync(OtherPlayersMatchId);

            Assert.Equal(MatchDetailStatus.NotYours, detail.Status);
        }

        private void StoreMatch(int matchId, string endReason)
        {
            _recordRepository.FinishedMatches.Add(new MatchRecord
            {
                MatchId = matchId,
                Status = MatchStatusCodes.Finished,
                PlayerCount = TwoPlayers,
                StartedAt = _startedAt,
                FinishedAt = _finishedAt,
                EndReason = endReason,
            });
        }
    }
}
