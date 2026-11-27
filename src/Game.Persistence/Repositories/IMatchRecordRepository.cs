using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface IMatchRecordRepository
    {
        Task<int> CreateAsync(int roomId, int playerCount);

        Task FinishAsync(int matchId, string endReason, DateTime finishedAt);

        Task<List<int>> ListInProgressIdsAsync();
    }
}
