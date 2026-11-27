using System;
using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface ISanctionRepository
    {
        Task<Sanction?> FindInForceAsync(int playerId, DateTime nowUtc);

        Task<bool> HasPermanentAsync(int playerId);

        Task<DateTime?> FindLatestThresholdAsync(int playerId);

        Task<int> CountAsync(int playerId);

        Task CreateAsync(Sanction newSanction);
    }
}
