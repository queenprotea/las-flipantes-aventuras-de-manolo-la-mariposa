using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface IPlayerRepository
    {
        Task<long> CountAsync();

        Task<PlayerCreationOutcome> CreateAsync(string username, string email, string passwordHash);

        Task<Player?> FindByUsernameAsync(string username);

        Task<Player?> FindByEmailAsync(string email);

        Task<Player?> FindByIdAsync(int playerId);

        Task<string?> FindAvatarReferenceAsync(int playerId);

        Task<ProfileUpdateOutcome> UpdateProfileAsync(int playerId, string username, string? avatarReference);

        Task<bool> DeleteAsync(int playerId);

        Player? FindByUsername(string username);

        List<Player> ListByIds(IReadOnlyCollection<int> playerIds);

        Task<List<Player>> ListByIdsAsync(IReadOnlyCollection<int> playerIds);
    }
}
