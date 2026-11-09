using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Game.Persistence
{
    public sealed class PlayerRepository
    {
        private const string UsernameIndex = "ux_player_username_lower";

        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public PlayerRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);

            _contextFactory = contextFactory;
        }

        public async Task<long> CountAsync()
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            long playerCount = await databaseContext.Players.LongCountAsync();

            return playerCount;
        }

        public async Task<PlayerCreationOutcome> CreateAsync(string username, string email, string passwordHash)
        {
            ArgumentNullException.ThrowIfNull(username);
            ArgumentNullException.ThrowIfNull(email);
            ArgumentNullException.ThrowIfNull(passwordHash);

            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
            var newPlayer = new Player
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
            };
            databaseContext.Players.Add(newPlayer);
            PlayerCreationOutcome creationOutcome = PlayerCreationOutcome.Created;
            try
            {
                await databaseContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } uniqueViolation)
            {
                creationOutcome = ToTakenOutcome(uniqueViolation.ConstraintName);
            }

            return creationOutcome;
        }

        public async Task<Player?> FindByUsernameAsync(string username)
        {
            ArgumentNullException.ThrowIfNull(username);

            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            Player? player = await databaseContext.Players
                .AsNoTracking()
                .SingleOrDefaultAsync(storedPlayer => storedPlayer.Username.ToLower() == username.ToLower());

            return player;
        }

        public Task<Player?> FindByIdAsync(int playerId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(playerId);

            Task<Player?> playerReading = ReadByIdAsync(playerId);

            return playerReading;
        }

        public Task<string?> FindAvatarReferenceAsync(int playerId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(playerId);

            Task<string?> avatarReferenceReading = ReadAvatarReferenceAsync(playerId);

            return avatarReferenceReading;
        }

        public Task<ProfileUpdateOutcome> UpdateProfileAsync(int playerId, string username, string? avatarReference)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(playerId);
            ArgumentException.ThrowIfNullOrWhiteSpace(username);

            Task<ProfileUpdateOutcome> profileWriting = WriteProfileAsync(playerId, username, avatarReference);

            return profileWriting;
        }

        public Player? FindByUsername(string username)
        {
            ArgumentNullException.ThrowIfNull(username);

            using TorresDbContext databaseContext = _contextFactory.CreateDbContext();

            Player? player = databaseContext.Players
                .AsNoTracking()
                .SingleOrDefault(storedPlayer => storedPlayer.Username.ToLower() == username.ToLower());

            return player;
        }

        public List<Player> ListByIds(IReadOnlyCollection<int> playerIds)
        {
            ArgumentNullException.ThrowIfNull(playerIds);

            using TorresDbContext databaseContext = _contextFactory.CreateDbContext();

            List<Player> players = databaseContext.Players
                .AsNoTracking()
                .Where(storedPlayer => playerIds.Contains(storedPlayer.PlayerId))
                .OrderBy(storedPlayer => storedPlayer.Username)
                .ToList();

            return players;
        }

        public Task<List<Player>> ListByIdsAsync(IReadOnlyCollection<int> playerIds)
        {
            ArgumentNullException.ThrowIfNull(playerIds);

            Task<List<Player>> playersReading = ReadByIdsAsync(playerIds);

            return playersReading;
        }

        private async Task<List<Player>> ReadByIdsAsync(IReadOnlyCollection<int> playerIds)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            List<Player> players = await databaseContext.Players
                .AsNoTracking()
                .Where(storedPlayer => playerIds.Contains(storedPlayer.PlayerId))
                .ToListAsync();

            return players;
        }

        private async Task<Player?> ReadByIdAsync(int playerId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            Player? player = await databaseContext.Players
                .AsNoTracking()
                .SingleOrDefaultAsync(storedPlayer => storedPlayer.PlayerId == playerId);

            return player;
        }

        private async Task<string?> ReadAvatarReferenceAsync(int playerId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            string? avatarReference = await databaseContext.Players
                .AsNoTracking()
                .Where(storedPlayer => storedPlayer.PlayerId == playerId)
                .Select(storedPlayer => storedPlayer.AvatarReference)
                .SingleOrDefaultAsync();

            return avatarReference;
        }

        private async Task<ProfileUpdateOutcome> WriteProfileAsync(int playerId, string username, string? avatarReference)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
            Player? storedPlayer = await databaseContext.Players.SingleOrDefaultAsync(candidate => candidate.PlayerId == playerId);
            if (storedPlayer is null)
            {
                return ProfileUpdateOutcome.PlayerNotFound;
            }

            storedPlayer.Username = username;
            storedPlayer.AvatarReference = avatarReference;
            ProfileUpdateOutcome updateOutcome = ProfileUpdateOutcome.Updated;
            try
            {
                await databaseContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: UsernameIndex })
            {
                updateOutcome = ProfileUpdateOutcome.UsernameTaken;
            }

            return updateOutcome;
        }

        private static PlayerCreationOutcome ToTakenOutcome(string? violatedIndex)
        {
            PlayerCreationOutcome takenOutcome = violatedIndex == UsernameIndex
                ? PlayerCreationOutcome.UsernameTaken
                : PlayerCreationOutcome.EmailTaken;

            return takenOutcome;
        }
    }
}
