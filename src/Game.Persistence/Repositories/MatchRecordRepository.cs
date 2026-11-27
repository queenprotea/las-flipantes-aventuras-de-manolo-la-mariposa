using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Game.Persistence
{
    public sealed class MatchRecordRepository : IMatchRecordRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public MatchRecordRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public Task<int> CountFinishedByPlayerAsync(int playerId)
        {
            Task<int> counting = CountFinishedAsync(playerId);

            return counting;
        }

        public Task<List<MatchRecord>> ListFinishedPageByPlayerAsync(int playerId, int skippedCount, int pageSize)
        {
            Task<List<MatchRecord>> pageReading = ReadFinishedPageAsync(playerId, skippedCount, pageSize);

            return pageReading;
        }

        public Task<MatchRecord?> FindFinishedAsync(int matchId)
        {
            Task<MatchRecord?> finding = ReadFinishedAsync(matchId);

            return finding;
        }

        public Task<int> CreateAsync(int roomId, int playerCount)
        {
            Task<int> creation = CreateInProgressAsync(roomId, playerCount);

            return creation;
        }

        public Task FinishAsync(int matchId, string endReason, DateTime finishedAt)
        {
            Task finishing = WriteFinishAsync(matchId, endReason, finishedAt);

            return finishing;
        }

        public Task<List<int>> ListInProgressIdsAsync()
        {
            Task<List<int>> listing = ReadInProgressIdsAsync();

            return listing;
        }

        public Task<bool> HasInProgressByPlayerAsync(int playerId)
        {
            Task<bool> lookup = HasInProgressAsync(playerId);

            return lookup;
        }

        public Task<List<int>> ListIdsByPlayerAsync(int playerId)
        {
            Task<List<int>> listing = ReadIdsByPlayerAsync(playerId);

            return listing;
        }

        public Task<int> DeleteFinishedWithoutParticipantsAsync(IReadOnlyCollection<int> matchIds)
        {
            Task<int> deletion = RemoveFinishedWithoutParticipantsAsync(matchIds);

            return deletion;
        }

        private async Task<List<int>> ReadInProgressIdsAsync()
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            List<int> inProgressIds = await databaseContext.MatchRecords
                .AsNoTracking()
                .Where(storedMatch => storedMatch.Status == MatchStatusCodes.InProgress)
                .OrderBy(storedMatch => storedMatch.MatchId)
                .Select(storedMatch => storedMatch.MatchId)
                .ToListAsync();

            return inProgressIds;
        }

        private async Task<int> CreateInProgressAsync(int roomId, int playerCount)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            var newMatch = new MatchRecord
            {
                RoomId = roomId,
                Status = MatchStatusCodes.InProgress,
                PlayerCount = (short)playerCount,
            };
            databaseContext.MatchRecords.Add(newMatch);
            await databaseContext.SaveChangesAsync();
            int createdMatchId = newMatch.MatchId;

            return createdMatchId;
        }

        private async Task WriteFinishAsync(int matchId, string endReason, DateTime finishedAt)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            await databaseContext.MatchRecords
                .Where(storedMatch => (storedMatch.MatchId == matchId) && (storedMatch.Status == MatchStatusCodes.InProgress))
                .ExecuteUpdateAsync(matchUpdate => matchUpdate
                    .SetProperty(storedMatch => storedMatch.Status, MatchStatusCodes.Finished)
                    .SetProperty(storedMatch => storedMatch.FinishedAt, finishedAt)
                    .SetProperty(storedMatch => storedMatch.EndReason, endReason));
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

        private async Task<bool> HasInProgressAsync(int playerId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            bool hasMatchInProgress = await SelectByPlayer(databaseContext, playerId)
                .AnyAsync(storedMatch => storedMatch.Status == MatchStatusCodes.InProgress);

            return hasMatchInProgress;
        }

        private async Task<List<int>> ReadIdsByPlayerAsync(int playerId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            List<int> playedMatchIds = await SelectByPlayer(databaseContext, playerId)
                .Select(storedMatch => storedMatch.MatchId)
                .ToListAsync();

            return playedMatchIds;
        }

        private async Task<int> RemoveFinishedWithoutParticipantsAsync(IReadOnlyCollection<int> matchIds)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            int removedCount = await databaseContext.MatchRecords
                .Where(storedMatch => matchIds.Contains(storedMatch.MatchId) && (storedMatch.Status == MatchStatusCodes.Finished))
                .Where(storedMatch => !databaseContext.MatchParticipants.Any(storedParticipant => storedParticipant.MatchId == storedMatch.MatchId))
                .ExecuteDeleteAsync();

            return removedCount;
        }

        private static IQueryable<MatchRecord> SelectByPlayer(TorresDbContext databaseContext, int playerId)
        {
            IQueryable<MatchRecord> playedMatches = databaseContext.MatchRecords
                .AsNoTracking()
                .Where(storedMatch => databaseContext.MatchParticipants
                    .Any(storedParticipant => (storedParticipant.MatchId == storedMatch.MatchId) && (storedParticipant.PlayerId == playerId)));

            return playedMatches;
        }

        private static IQueryable<MatchRecord> SelectFinishedByPlayer(TorresDbContext databaseContext, int playerId)
        {
            IQueryable<MatchRecord> finishedMatches = SelectByPlayer(databaseContext, playerId)
                .Where(storedMatch => storedMatch.Status == MatchStatusCodes.Finished);

            return finishedMatches;
        }
    }
}
