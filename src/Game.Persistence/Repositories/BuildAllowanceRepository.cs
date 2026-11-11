using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Game.Persistence
{
    public sealed class BuildAllowanceRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public BuildAllowanceRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);

            _contextFactory = contextFactory;
        }

        public async Task<List<BuildAllowance>> GetAllAsync()
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            List<BuildAllowance> buildAllowances = await databaseContext.BuildAllowances
                .AsNoTracking()
                .OrderBy(buildAllowance => buildAllowance.PlayerCount)
                .ThenBy(buildAllowance => buildAllowance.RoundNumber)
                .ThenBy(buildAllowance => buildAllowance.TurnNumber)
                .ToListAsync();

            return buildAllowances;
        }
    }
}
