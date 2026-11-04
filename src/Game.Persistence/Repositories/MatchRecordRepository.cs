using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Game.Persistence
{
    public sealed class MatchRecordRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public MatchRecordRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);

            _contextFactory = contextFactory;
        }

        public Task<int> CountFinishedByPlayerAsync(int playerId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(playerId);

            Task<int> counting = CountFinishedAsync(playerId);

            return counting;
        }

        public Task<List<MatchRecord>> ListFinishedPageByPlayerAsync(int playerId, int skippedCount, int pageSize)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(playerId);
            ArgumentOutOfRangeException.ThrowIfNegative(skippedCount);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

            Task<List<MatchRecord>> pageReading = ReadFinishedPageAsync(playerId, skippedCount, pageSize);

            return pageReading;
        }

        public Task<MatchRecord?> FindFinishedAsync(int matchId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(matchId);

            Task<MatchRecord?> finding = ReadFinishedAsync(matchId);

            return finding;
        }

        private async Task<int> CountFinishedAsync(int playerId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            int finishedCount = await SelectFinishedByPlayer(databaseContext, playerId).CountAsync();

            return finishedCount;
        }

        private async Task<List<MatchRecord>> ReadFinishedPageAsync(int playerId, int skippedCount, int pageSize)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            List<MatchRecord> pageMatches = await SelectFinishedByPlayer(databaseContext, playerId)
                .OrderByDescending(storedMatch => storedMatch.FinishedAt)
                .ThenByDescending(storedMatch => storedMatch.MatchId)
                .Skip(skippedCount)
                .Take(pageSize)
                .ToListAsync();

            return pageMatches;
        }

        private async Task<MatchRecord?> ReadFinishedAsync(int matchId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            MatchRecord? finishedMatch = await databaseContext.MatchRecords
                .AsNoTracking()
                .SingleOrDefaultAsync(storedMatch => (storedMatch.MatchId == matchId) && (storedMatch.Status == MatchStatusCodes.Finished));

            return finishedMatch;
        }

        private static IQueryable<MatchRecord> SelectFinishedByPlayer(TorresDbContext databaseContext, int playerId)
        {
            IQueryable<MatchRecord> finishedMatches = databaseContext.MatchRecords
                .AsNoTracking()
                .Where(storedMatch => storedMatch.Status == MatchStatusCodes.Finished)
                .Where(storedMatch => databaseContext.MatchParticipants
                    .Any(storedParticipant => (storedParticipant.MatchId == storedMatch.MatchId) && (storedParticipant.PlayerId == playerId)));

            return finishedMatches;
        }
    }
}
