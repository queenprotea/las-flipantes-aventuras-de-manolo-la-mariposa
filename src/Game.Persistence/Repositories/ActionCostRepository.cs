using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Game.Persistence
{
    public sealed class ActionCostRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public ActionCostRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);

            _contextFactory = contextFactory;
        }

        public async Task<List<ActionCost>> GetAllAsync()
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            List<ActionCost> actionCosts = await databaseContext.ActionCosts
                .AsNoTracking()
                .OrderBy(actionCost => actionCost.ActionCode)
                .ToListAsync();

            return actionCosts;
        }
    }
}
