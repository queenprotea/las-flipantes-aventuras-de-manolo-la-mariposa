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
    }
}
