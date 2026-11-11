using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Game.Persistence
{
    public sealed class TurnLayoutRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public TurnLayoutRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);

            _contextFactory = contextFactory;
        }

        public async Task<List<TurnLayout>> GetAllAsync()
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            List<TurnLayout> turnLayouts = await databaseContext.TurnLayouts
                .AsNoTracking()
                .OrderBy(turnLayout => turnLayout.PlayerCount)
                .ThenBy(turnLayout => turnLayout.RoundNumber)
                .ToListAsync();

            return turnLayouts;
        }
    }
}
