using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingRankingRepository : IRankingRepository
    {
        internal List<PlayerRanking> Rankings { get; } = new List<PlayerRanking>();

        public Task<List<PlayerRanking>> GetGlobalRankingAsync()
        {
            Task<List<PlayerRanking>> reading = Task.FromResult(new List<PlayerRanking>(Rankings));

            return reading;
        }
    }
}
