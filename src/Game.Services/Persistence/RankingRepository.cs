using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Dapper;

using Npgsql;

namespace Game.Persistence
{
    public sealed record RankingRow(string Username, long Wins, long TotalScore, long MatchesPlayed);

    public sealed class RankingRepository
    {
        private const string GlobalRankingSql = @"
            SELECT username,
                   wins,
                   total_score AS totalscore,
                   matches_played AS matchesplayed
            FROM player_ranking
            ORDER BY wins DESC, total_score DESC, matches_played ASC";

        private readonly NpgsqlDataSource _dataSource;

        public RankingRepository(NpgsqlDataSource dataSource)
        {
            ArgumentNullException.ThrowIfNull(dataSource);
            _dataSource = dataSource;
        }

        public async Task<List<RankingRow>> GetGlobalRankingAsync()
        {
            await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync();

            var rows = await connection.QueryAsync<RankingRow>(GlobalRankingSql);
            return rows.AsList();
        }
    }
}
