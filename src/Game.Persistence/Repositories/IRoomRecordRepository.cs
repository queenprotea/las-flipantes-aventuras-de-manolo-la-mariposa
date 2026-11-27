using System;
using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface IRoomRecordRepository
    {
        Task<int?> CreateAsync(string code, string visibility);

        Task CloseAsync(int roomId, DateTime closedAtUtc);

        Task<int> CloseAllOpenAsync(DateTime closedAtUtc);
    }
}
