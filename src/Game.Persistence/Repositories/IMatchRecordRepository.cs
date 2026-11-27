using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface IMatchRecordRepository
    {
        Task<int> CountFinishedByPlayerAsync(int playerId);

        Task<List<MatchRecord>> ListFinishedPageByPlayerAsync(int playerId, int skippedCount, int pageSize);

        Task<MatchRecord?> FindFinishedAsync(int matchId);

        Task<int> CreateAsync(int roomId, int playerCount);

        Task FinishAsync(int matchId, string endReason, DateTime finishedAtUtc);

        Task<List<int>> ListInProgressIdsAsync();
    }
}
