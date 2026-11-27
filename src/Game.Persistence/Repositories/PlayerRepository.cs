using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Game.Persistence
{
    public sealed class PlayerRepository : IPlayerRepository
    {
        private const string UsernameIndex = "ux_player_username_lower";
        private const string LowerCaseIndexJustification = "EF Core translates ToLower() into lower(), which uses the unique lower(...) index of the table; string.Equals with a StringComparison is not translated into SQL.";

        private readonly IDbContextFactory<TorresDbContext> _contextFactory;

        public PlayerRepository(IDbContextFactory<TorresDbContext> contextFactory)
        {
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

        [SuppressMessage("Performance", "CA1862", Justification = LowerCaseIndexJustification)]
        public async Task<Player?> FindByUsernameAsync(string username)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            Player? player = await databaseContext.Players
                .AsNoTracking()
                .SingleOrDefaultAsync(storedPlayer => storedPlayer.Username.ToLower() == username.ToLower());

            return player;
        }

        public Task<Player?> FindByEmailAsync(string email)
        {
            Task<Player?> playerReading = ReadByEmailAsync(email);

            return playerReading;
        }

        public Task<Player?> FindByIdAsync(int playerId)
        {
            Task<Player?> playerReading = ReadByIdAsync(playerId);

            return playerReading;
        }

        public Task<string?> FindAvatarReferenceAsync(int playerId)
        {
            Task<string?> avatarReferenceReading = ReadAvatarReferenceAsync(playerId);

            return avatarReferenceReading;
        }

        public Task<ProfileUpdateOutcome> UpdateProfileAsync(int playerId, string username, string? avatarReference)
        {
            Task<ProfileUpdateOutcome> profileWriting = WriteProfileAsync(playerId, username, avatarReference);

            return profileWriting;
        }

        public Task<bool> DeleteAsync(int playerId)
        {
            Task<bool> deletion = RemovePlayerAsync(playerId);

            return deletion;
        }

        [SuppressMessage("Performance", "CA1862", Justification = LowerCaseIndexJustification)]
        public Player? FindByUsername(string username)
        {
            using TorresDbContext databaseContext = _contextFactory.CreateDbContext();

            Player? player = databaseContext.Players
                .AsNoTracking()
                .SingleOrDefault(storedPlayer => storedPlayer.Username.ToLower() == username.ToLower());

            return player;
        }

        public List<Player> ListByIds(IReadOnlyCollection<int> playerIds)
        {
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

        [SuppressMessage("Performance", "CA1862", Justification = LowerCaseIndexJustification)]
        private async Task<Player?> ReadByEmailAsync(string email)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            Player? player = await databaseContext.Players
                .AsNoTracking()
                .SingleOrDefaultAsync(storedPlayer => storedPlayer.Email.ToLower() == email.ToLower());

            return player;
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

        private async Task<bool> RemovePlayerAsync(int playerId)
        {
            await using TorresDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

            int removedCount = await databaseContext.Players
                .Where(storedPlayer => storedPlayer.PlayerId == playerId)
                .ExecuteDeleteAsync();
            bool wasRemoved = removedCount > 0;

            return wasRemoved;
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
