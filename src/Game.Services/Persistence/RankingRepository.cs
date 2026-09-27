// Game.Services/Persistence/RankingRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Dapper;

using Npgsql;

namespace Game.Services.Persistence
{
    public sealed record RankingRow(int Id, string Username, long Wins, long Points, long Matches, long Rank);

    public sealed class RankingRepository
    {
        private const int TopCount = 10;

        private const string GlobalRankingSql = @"
            WITH ranked AS (
                SELECT
                    player_id      AS id,
                    username       AS username,
                    wins           AS wins,
                    total_score    AS points,
                    matches_played AS matches,
                    RANK() OVER (ORDER BY wins DESC, total_score DESC, matches_played ASC) AS rank
                FROM player_ranking
            )
            SELECT * FROM ranked WHERE rank <= @TopCount
            UNION
            SELECT * FROM ranked WHERE id = @CurrentUserId
            ORDER BY rank;";

        private readonly NpgsqlDataSource _dataSource;

        public RankingRepository(NpgsqlDataSource dataSource)
        {
            ArgumentNullException.ThrowIfNull(dataSource);
            _dataSource = dataSource;
        }

        public async Task<IReadOnlyList<RankingRow>> GetGlobalRankingAsync(int currentUserId)
        {
            await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync();

            var rows = await connection.QueryAsync<RankingRow>(
                GlobalRankingSql,
                new { TopCount, CurrentUserId = currentUserId });

            return rows.AsList();
        }
    }
}