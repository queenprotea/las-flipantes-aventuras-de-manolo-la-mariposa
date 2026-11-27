using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Persistence;

using Npgsql;

namespace Game.Services.Tests
{
    internal sealed class FailingPlayerRepository : IPlayerRepository
    {
        private const string FailureMessage = "The test database is down.";

        public Task<long> CountAsync()
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<PlayerCreationOutcome> CreateAsync(string username, string email, string passwordHash)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<Player?> FindByUsernameAsync(string username)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<Player?> FindByEmailAsync(string email)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<Player?> FindByIdAsync(int playerId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<string?> FindAvatarReferenceAsync(int playerId)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<ProfileUpdateOutcome> UpdateProfileAsync(int playerId, string username, string? avatarReference)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Player? FindByUsername(string username)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public List<Player> ListByIds(IReadOnlyCollection<int> playerIds)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<List<Player>> ListByIdsAsync(IReadOnlyCollection<int> playerIds)
        {
            throw new NpgsqlException(FailureMessage);
        }

        public Task<bool> DeleteAsync(int playerId)
        {
            throw new NpgsqlException(FailureMessage);
        }
    }
}
