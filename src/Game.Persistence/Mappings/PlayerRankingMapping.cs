using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class PlayerRankingMapping : IEntityTypeConfiguration<PlayerRanking>
    {
        private const string PlayerRankingView = "player_ranking";
        private const string PlayerIdColumn = "player_id";
        private const string UsernameColumn = "username";
        private const string MatchesPlayedColumn = "matches_played";
        private const string WinsColumn = "wins";
        private const string TotalScoreColumn = "total_score";
        private const string BestScoreColumn = "best_score";
        private const string LastMatchAtColumn = "last_match_at";

        public void Configure(EntityTypeBuilder<PlayerRanking> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToView(PlayerRankingView);
            builder.HasNoKey();
            builder.Property(mappedRanking => mappedRanking.PlayerId).HasColumnName(PlayerIdColumn);
            builder.Property(mappedRanking => mappedRanking.Username).HasColumnName(UsernameColumn);
            builder.Property(mappedRanking => mappedRanking.MatchesPlayed).HasColumnName(MatchesPlayedColumn);
            builder.Property(mappedRanking => mappedRanking.Wins).HasColumnName(WinsColumn);
            builder.Property(mappedRanking => mappedRanking.TotalScore).HasColumnName(TotalScoreColumn);
            builder.Property(mappedRanking => mappedRanking.BestScore).HasColumnName(BestScoreColumn);
            builder.Property(mappedRanking => mappedRanking.LastMatchAt).HasColumnName(LastMatchAtColumn);
        }
    }
}
