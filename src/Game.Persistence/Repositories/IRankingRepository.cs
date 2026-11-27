using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface IRankingRepository
    {
        Task<List<PlayerRanking>> GetGlobalRankingAsync();
    }
}
