using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingMatchRecordRepository : IMatchRecordRepository
    {
        private readonly Lock _recordLock = new Lock();
        private readonly RecordingMatchParticipantRepository _participantRepository;

        private int _lastMatchId;

        internal RecordingMatchRecordRepository(RecordingMatchParticipantRepository participantRepository)
        {
            _participantRepository = participantRepository;
        }

        internal List<MatchRecord> FinishedMatches { get; } = new List<MatchRecord>();

        public Task<int> CountFinishedByPlayerAsync(int playerId)
        {
            Task<int> counting = Task.FromResult(ListFinishedOf(playerId).Count);

            return counting;
        }

        public Task<List<MatchRecord>> ListFinishedPageByPlayerAsync(int playerId, int skippedCount, int pageSize)
        {
            List<MatchRecord> pageMatches = ListFinishedOf(playerId).Skip(skippedCount).Take(pageSize).ToList();
            Task<List<MatchRecord>> listing = Task.FromResult(pageMatches);

            return listing;
        }

        public Task<MatchRecord?> FindFinishedAsync(int matchId)
        {
            Task<MatchRecord?> finding = Task.FromResult(FinishedMatches.Find(match => match.MatchId == matchId));

            return finding;
        }

        public Task<int> CreateAsync(int roomId, int playerCount)
        {
            int createdMatchId;
            lock (_recordLock)
            {
                _lastMatchId++;
                createdMatchId = _lastMatchId;
            }

            Task<int> creation = Task.FromResult(createdMatchId);

            return creation;
        }

        public Task FinishAsync(int matchId, string endReason, DateTime finishedAtUtc)
        {
            Task finishing = Task.CompletedTask;

            return finishing;
        }

        public Task<List<int>> ListInProgressIdsAsync()
        {
            Task<List<int>> listing = Task.FromResult(new List<int>());

            return listing;
        }

        public Task<bool> HasInProgressByPlayerAsync(int playerId)
        {
            Task<bool> checking = Task.FromResult(false);

            return checking;
        }

        public Task<List<int>> ListIdsByPlayerAsync(int playerId)
        {
            List<int> playedMatchIds = _participantRepository.Participants
                .Where(participant => participant.PlayerId == playerId)
                .Select(participant => participant.MatchId)
                .ToList();
            Task<List<int>> listing = Task.FromResult(playedMatchIds);

            return listing;
        }

        public Task<int> DeleteFinishedWithoutParticipantsAsync(IReadOnlyCollection<int> matchIds)
        {
            int deletedCount = FinishedMatches.RemoveAll(match => matchIds.Contains(match.MatchId) && !_participantRepository.Participants.Exists(participant => participant.MatchId == match.MatchId));
            Task<int> deletion = Task.FromResult(deletedCount);

            return deletion;
        }

        private List<MatchRecord> ListFinishedOf(int playerId)
        {
            List<int> playedMatchIds = _participantRepository.Participants
                .Where(participant => participant.PlayerId == playerId)
                .Select(participant => participant.MatchId)
                .ToList();
            List<MatchRecord> finishedMatches = FinishedMatches.Where(match => playedMatchIds.Contains(match.MatchId)).ToList();

            return finishedMatches;
        }
    }
}
