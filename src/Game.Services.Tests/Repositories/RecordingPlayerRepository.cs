using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingPlayerRepository : IPlayerRepository
    {
        private int _lastPlayerId = SamplePlayers.EvaId;

        internal List<Player> Players { get; } = SamplePlayers.CreateAccounts();

        public Task<long> CountAsync()
        {
            Task<long> counting = Task.FromResult((long)Players.Count);

            return counting;
        }

        public Task<PlayerCreationOutcome> CreateAsync(string username, string email, string passwordHash)
        {
            PlayerCreationOutcome creationOutcome = PlayerCreationOutcome.Created;
            if (FindByUsername(username) is not null)
            {
                creationOutcome = PlayerCreationOutcome.UsernameTaken;
            }
            else if (FindByEmail(email) is not null)
            {
                creationOutcome = PlayerCreationOutcome.EmailTaken;
            }
            else
            {
                _lastPlayerId++;
                Players.Add(new Player { PlayerId = _lastPlayerId, Username = username, Email = email, PasswordHash = passwordHash });
            }

            Task<PlayerCreationOutcome> creation = Task.FromResult(creationOutcome);

            return creation;
        }

        public Task<Player?> FindByUsernameAsync(string username)
        {
            Task<Player?> finding = Task.FromResult(FindByUsername(username));

            return finding;
        }

        public Task<Player?> FindByEmailAsync(string email)
        {
            Task<Player?> finding = Task.FromResult(FindByEmail(email));

            return finding;
        }

        public Task<Player?> FindByIdAsync(int playerId)
        {
            Task<Player?> finding = Task.FromResult(Players.Find(player => player.PlayerId == playerId));

            return finding;
        }

        public Task<string?> FindAvatarReferenceAsync(int playerId)
        {
            Task<string?> finding = Task.FromResult(Players.Find(player => player.PlayerId == playerId)?.AvatarReference);

            return finding;
        }

        public Task<ProfileUpdateOutcome> UpdateProfileAsync(int playerId, string username, string? avatarReference)
        {
            Player? updatedPlayer = Players.Find(player => player.PlayerId == playerId);
            Player? usernameOwner = FindByUsername(username);
            ProfileUpdateOutcome updateOutcome = ProfileUpdateOutcome.Updated;
            if (updatedPlayer is null)
            {
                updateOutcome = ProfileUpdateOutcome.PlayerNotFound;
            }
            else if ((usernameOwner is not null) && (usernameOwner.PlayerId != playerId))
            {
                updateOutcome = ProfileUpdateOutcome.UsernameTaken;
            }
            else
            {
                updatedPlayer.Username = username;
                updatedPlayer.AvatarReference = avatarReference;
            }

            Task<ProfileUpdateOutcome> updating = Task.FromResult(updateOutcome);

            return updating;
        }

        public Player? FindByUsername(string username)
        {
            Player? foundPlayer = Players.Find(player => string.Equals(player.Username, username, StringComparison.OrdinalIgnoreCase));

            return foundPlayer;
        }

        public List<Player> ListByIds(IReadOnlyCollection<int> playerIds)
        {
            List<Player> listedPlayers = Players.Where(player => playerIds.Contains(player.PlayerId)).ToList();

            return listedPlayers;
        }

        public Task<List<Player>> ListByIdsAsync(IReadOnlyCollection<int> playerIds)
        {
            Task<List<Player>> listing = Task.FromResult(ListByIds(playerIds));

            return listing;
        }

        private Player? FindByEmail(string email)
        {
            Player? foundPlayer = Players.Find(player => string.Equals(player.Email, email, StringComparison.OrdinalIgnoreCase));

            return foundPlayer;
        }
    }
}
