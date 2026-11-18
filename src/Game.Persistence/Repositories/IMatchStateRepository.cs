using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface IMatchStateRepository
    {
        Task SaveAsync(MatchState savedState);

        Task DeleteAsync(int matchId);

        Task<List<MatchState>> ListAsync();
    }
}
