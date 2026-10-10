using System;
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

        private static PlayerCreationOutcome ToTakenOutcome(string? violatedIndex)
        {
            PlayerCreationOutcome takenOutcome = violatedIndex == UsernameIndex
                ? PlayerCreationOutcome.UsernameTaken
                : PlayerCreationOutcome.EmailTaken;

            return takenOutcome;
        }
    }
}
