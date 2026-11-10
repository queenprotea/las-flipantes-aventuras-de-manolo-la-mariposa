using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Game.Persistence
{
    public sealed class SanctionLevelRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public SanctionLevelRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);

            _contextFactory = contextFactory;
        }

        public async Task<List<SanctionLevel>> ListAsync()
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            List<SanctionLevel> sanctionLevels = await databaseContext.SanctionLevels
                .AsNoTracking()
                .OrderBy(storedLevel => storedLevel.Level)
                .ToListAsync();

            return sanctionLevels;
        }
    }
}
