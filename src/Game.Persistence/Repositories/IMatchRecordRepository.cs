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

        Task FinishAsync(int matchId, string endReason, DateTime finishedAt);

        Task<List<int>> ListInProgressIdsAsync();

        Task<bool> HasInProgressByPlayerAsync(int playerId);

        Task<List<int>> ListIdsByPlayerAsync(int playerId);

        Task<int> DeleteFinishedWithoutParticipantsAsync(IReadOnlyCollection<int> matchIds);
    }
}
