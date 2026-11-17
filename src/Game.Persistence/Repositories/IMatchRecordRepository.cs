using System;
using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface IMatchRecordRepository
    {
        Task<int> CreateAsync(int roomId, int playerCount);

        Task FinishAsync(int matchId, string endReason, DateTime finishedAtUtc);
    }
}
