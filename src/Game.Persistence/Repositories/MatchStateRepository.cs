using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Game.Persistence
{
    public sealed class MatchStateRepository : IMatchStateRepository
    {
        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public MatchStateRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public Task SaveAsync(MatchState savedState)
        {
            Task saving = WriteStateAsync(savedState);

            return saving;
        }

        public Task DeleteAsync(int matchId)
        {
            Task deleting = DeleteStateAsync(matchId);

            return deleting;
        }

        public Task<List<MatchState>> ListAsync()
        {
            Task<List<MatchState>> listing = ReadStatesAsync();

            return listing;
        }

        private async Task WriteStateAsync(MatchState savedState)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            int updatedCount = await databaseContext.MatchStates
                .Where(storedState => storedState.MatchId == savedState.MatchId)
                .ExecuteUpdateAsync(stateUpdate => stateUpdate
                    .SetProperty(storedState => storedState.RoundNumber, savedState.RoundNumber)
                    .SetProperty(storedState => storedState.TurnNumber, savedState.TurnNumber)
                    .SetProperty(storedState => storedState.ActiveSeatNumber, savedState.ActiveSeatNumber)
                    .SetProperty(storedState => storedState.StateDocument, savedState.StateDocument)
                    .SetProperty(storedState => storedState.SavedAt, savedState.SavedAt));
            if (updatedCount == 0)
            {
                databaseContext.MatchStates.Add(savedState);
                await databaseContext.SaveChangesAsync();
            }
        }

        private async Task DeleteStateAsync(int matchId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            await databaseContext.MatchStates
                .Where(storedState => storedState.MatchId == matchId)
                .ExecuteDeleteAsync();
        }

        private async Task<List<MatchState>> ReadStatesAsync()
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            List<MatchState> savedStates = await databaseContext.MatchStates
                .AsNoTracking()
                .ToListAsync();

            return savedStates;
        }
    }
}
