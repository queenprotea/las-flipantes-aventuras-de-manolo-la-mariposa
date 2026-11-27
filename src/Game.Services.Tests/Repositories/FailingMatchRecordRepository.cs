using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Persistence;

using Npgsql;

namespace Game.Services.Tests
{
    internal sealed class FailingMatchRecordRepository : IMatchRecordRepository
    {
        private const string FailureMessage = "The test database is down.";

        public Task<int> CountFinishedByPlayerAsync(int playerId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<List<MatchRecord>> ListFinishedPageByPlayerAsync(int playerId, int skippedCount, int pageSize)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<MatchRecord?> FindFinishedAsync(int matchId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<int> CreateAsync(int roomId, int playerCount)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task FinishAsync(int matchId, string endReason, DateTime finishedAt)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<List<int>> ListInProgressIdsAsync()
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<bool> HasInProgressByPlayerAsync(int playerId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<List<int>> ListIdsByPlayerAsync(int playerId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<int> DeleteFinishedWithoutParticipantsAsync(IReadOnlyCollection<int> matchIds)
        {
            throw new NpgsqlException(FailureMessage);
        }
    }
}
