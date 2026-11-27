using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Match.Tests
{
    internal sealed class RecordingMatchRecordRepository : IMatchRecordRepository
    {
        private const int NoFinishedMatches = 0;

        private readonly Lock _recordLock = new Lock();
        private readonly TaskCompletionSource<RecordedFinish> _firstFinish = new TaskCompletionSource<RecordedFinish>(TaskCreationOptions.RunContinuationsAsynchronously);

        private int _lastMatchId;
        private int _lastRoomId;
        private int _lastPlayerCount;

        internal int LastMatchId
        {
            get
            {
                lock (_recordLock)
                {
                    return _lastMatchId;
                }
            }
        }

        internal int LastRoomId
        {
            get
            {
                lock (_recordLock)
                {
                    return _lastRoomId;
                }
            }
        }

        internal int LastPlayerCount
        {
            get
            {
                lock (_recordLock)
                {
                    return _lastPlayerCount;
                }
            }
        }

        internal Task<RecordedFinish> FirstFinish => _firstFinish.Task;

        internal List<int> InProgressMatchIds { get; } = new List<int>();

        public Task<int> CountFinishedByPlayerAsync(int playerId)
        {
            Task<int> counting = Task.FromResult(NoFinishedMatches);

            return counting;
        }

        public Task<List<MatchRecord>> ListFinishedPageByPlayerAsync(int playerId, int skippedCount, int pageSize)
        {
            Task<List<MatchRecord>> listing = Task.FromResult(new List<MatchRecord>());

            return listing;
        }

        public Task<MatchRecord?> FindFinishedAsync(int matchId)
        {
            Task<MatchRecord?> finding = Task.FromResult<MatchRecord?>(null);

            return finding;
        }

        public Task<int> CreateAsync(int roomId, int playerCount)
        {
            int createdMatchId;
            lock (_recordLock)
            {
                _lastMatchId++;
                _lastRoomId = roomId;
                _lastPlayerCount = playerCount;
                createdMatchId = _lastMatchId;
            }

            Task<int> creation = Task.FromResult(createdMatchId);

            return creation;
        }

        public Task<List<int>> ListInProgressIdsAsync()
        {
            Task<List<int>> listing = Task.FromResult(new List<int>(InProgressMatchIds));

            return listing;
        }

        public Task FinishAsync(int matchId, string endReason, DateTime finishedAt)
        {
            _firstFinish.TrySetResult(new RecordedFinish(matchId, endReason));
            Task finishing = Task.CompletedTask;

            return finishing;
        }

        public Task<bool> HasInProgressByPlayerAsync(int playerId)
        {
            Task<bool> checking = Task.FromResult(false);

            return checking;
        }

        public Task<List<int>> ListIdsByPlayerAsync(int playerId)
        {
            Task<List<int>> listing = Task.FromResult(new List<int>());

            return listing;
        }

        public Task<int> DeleteFinishedWithoutParticipantsAsync(IReadOnlyCollection<int> matchIds)
        {
            Task<int> deletion = Task.FromResult(NoFinishedMatches);

            return deletion;
        }
    }
}
