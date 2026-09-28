using System;
using System.Threading.Tasks;

using Dapper;

using Npgsql;

namespace Game.Persistence
{
    public sealed class PlayerRepository
    {
        private const string CountSql = "SELECT count(*) FROM player";
        private const string InsertSql =
            "INSERT INTO player (username, email, password_hash) VALUES (@Username, @Email, @PasswordHash)";
        private const string FindByUsernameSql =
            "SELECT player_id AS PlayerId, username AS Username, email AS Email, password_hash AS PasswordHash "
            + "FROM player WHERE lower(username) = lower(@Username)";
        private const string UsernameIndex = "ux_player_username_lower";

        private readonly NpgsqlDataSource _dataSource;

        public PlayerRepository(NpgsqlDataSource dataSource)
        {
            ArgumentNullException.ThrowIfNull(dataSource);

            _dataSource = dataSource;
        }

        public async Task<long> CountAsync()
        {
            await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync();

            return await connection.ExecuteScalarAsync<long>(CountSql);
        }

        public async Task<PlayerCreationOutcome> CreateAsync(string username, string email, string passwordHash)
        {
            await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync();
            try
            {
                await connection.ExecuteAsync(
                    InsertSql,
                    new { Username = username, Email = email, PasswordHash = passwordHash });
            }
            catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return exception.ConstraintName == UsernameIndex
                    ? PlayerCreationOutcome.UsernameTaken
                    : PlayerCreationOutcome.EmailTaken;
            }

            return PlayerCreationOutcome.Created;
        }

        public async Task<PlayerAccount?> FindByUsernameAsync(string username)
        {
            await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync();

            return await connection.QuerySingleOrDefaultAsync<PlayerAccount>(
                FindByUsernameSql,
                new { Username = username });
        }
    }
}
