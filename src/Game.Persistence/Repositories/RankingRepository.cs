using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Game.Persistence
{
    public sealed class RankingRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public RankingRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);

            _contextFactory = contextFactory;
        }

        public async Task<List<PlayerRanking>> GetGlobalRankingAsync()
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            List<PlayerRanking> rankings = await databaseContext.PlayerRankings
                .OrderByDescending(ranking => ranking.Wins)
                .ThenByDescending(ranking => ranking.TotalScore)
                .ThenBy(ranking => ranking.MatchesPlayed)
                .ToListAsync();

            return rankings;
        }
    }
}
