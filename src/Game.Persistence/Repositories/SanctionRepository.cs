using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Game.Persistence
{
    public sealed class SanctionRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public SanctionRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);

            _contextFactory = contextFactory;
        }

        public async Task<Sanction?> FindInForceAsync(int playerId, DateTime nowUtc)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(playerId);

            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            Sanction? sanctionInForce = await databaseContext.Sanctions
                .AsNoTracking()
                .Where(storedSanction => (storedSanction.PlayerId == playerId) && (storedSanction.EffectiveFrom <= nowUtc))
                .Where(storedSanction => storedSanction.IsPermanent || (storedSanction.EffectiveUntil > nowUtc))
                .OrderByDescending(storedSanction => storedSanction.IsPermanent)
                .ThenByDescending(storedSanction => storedSanction.EffectiveUntil)
                .FirstOrDefaultAsync();

            return sanctionInForce;
        }

        public Task<bool> HasPermanentAsync(int playerId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(playerId);

            Task<bool> permanentBanReading = ReadPermanentBanExistenceAsync(playerId);

            return permanentBanReading;
        }

        public Task<DateTime?> FindLatestThresholdAsync(int playerId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(playerId);

            Task<DateTime?> latestThresholdReading = ReadLatestThresholdAsync(playerId);

            return latestThresholdReading;
        }

        public Task<int> CountAsync(int playerId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(playerId);

            Task<int> sanctionCounting = CountForPlayerAsync(playerId);

            return sanctionCounting;
        }

        public Task CreateAsync(Sanction newSanction)
        {
            ArgumentNullException.ThrowIfNull(newSanction);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newSanction.PlayerId);

            Task creation = StoreAsync(newSanction);

            return creation;
        }

        private async Task<bool> ReadPermanentBanExistenceAsync(int playerId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            bool hasPermanentBan = await databaseContext.Sanctions
                .AnyAsync(storedSanction => (storedSanction.PlayerId == playerId) && storedSanction.IsPermanent);

            return hasPermanentBan;
        }

        private async Task<DateTime?> ReadLatestThresholdAsync(int playerId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            DateTime? latestThresholdAt = await databaseContext.Sanctions
                .Where(storedSanction => storedSanction.PlayerId == playerId)
                .MaxAsync(storedSanction => (DateTime?)storedSanction.ThresholdAt);

            return latestThresholdAt;
        }

        private async Task<int> CountForPlayerAsync(int playerId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            int sanctionCount = await databaseContext.Sanctions
                .CountAsync(storedSanction => storedSanction.PlayerId == playerId);

            return sanctionCount;
        }

        private async Task StoreAsync(Sanction newSanction)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
            databaseContext.Sanctions.Add(newSanction);
            await databaseContext.SaveChangesAsync();
        }
    }
}
