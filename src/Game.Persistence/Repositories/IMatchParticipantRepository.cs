using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface IMatchParticipantRepository
    {
        Task AddAsync(int matchId, IReadOnlyList<MatchParticipant> seatedPlayers);

        Task WriteResultsAsync(int matchId, IReadOnlyList<MatchParticipant> playerResults);
    }
}
