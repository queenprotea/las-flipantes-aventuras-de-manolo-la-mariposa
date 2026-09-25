using System;
using System.Threading.Tasks;

using Dapper;

using Npgsql;

namespace Game.Persistence
{
    public sealed class PlayerRepository
    {
        private const string CountSql = "SELECT count(*) FROM player";

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
    }
}
