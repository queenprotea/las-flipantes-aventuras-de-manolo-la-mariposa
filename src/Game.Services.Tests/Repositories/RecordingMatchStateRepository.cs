using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingMatchStateRepository : IMatchStateRepository
    {
        public Task SaveAsync(MatchState savedState)
        {
            Task saving = Task.CompletedTask;

            return saving;
        }

        public Task DeleteAsync(int matchId)
        {
            Task deleting = Task.CompletedTask;

            return deleting;
        }

        public Task<List<MatchState>> ListAsync()
        {
            Task<List<MatchState>> listing = Task.FromResult(new List<MatchState>());

            return listing;
        }
    }
}
