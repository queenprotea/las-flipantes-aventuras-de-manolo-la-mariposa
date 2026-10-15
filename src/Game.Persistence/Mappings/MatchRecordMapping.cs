using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class MatchRecordMapping : IEntityTypeConfiguration<MatchRecord>
    {
        private const string MatchRecordTable = "match";
        private const string MatchIdColumn = "match_id";
        private const string RoomIdColumn = "room_id";
        private const string StatusColumn = "status";
        private const string PlayerCountColumn = "player_count";
        private const string StartedAtColumn = "started_at";
        private const string FinishedAtColumn = "finished_at";
        private const string EndReasonColumn = "end_reason";

        public void Configure(EntityTypeBuilder<MatchRecord> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(MatchRecordTable);
            builder.HasKey(mappedMatch => mappedMatch.MatchId);
            builder.Property(mappedMatch => mappedMatch.MatchId)
                .HasColumnName(MatchIdColumn)
                .ValueGeneratedOnAdd();
            builder.Property(mappedMatch => mappedMatch.RoomId).HasColumnName(RoomIdColumn);
            builder.Property(mappedMatch => mappedMatch.Status).HasColumnName(StatusColumn);
            builder.Property(mappedMatch => mappedMatch.PlayerCount).HasColumnName(PlayerCountColumn);
            builder.Property(mappedMatch => mappedMatch.StartedAt)
                .HasColumnName(StartedAtColumn)
                .ValueGeneratedOnAdd();
            builder.Property(mappedMatch => mappedMatch.FinishedAt).HasColumnName(FinishedAtColumn);
            builder.Property(mappedMatch => mappedMatch.EndReason).HasColumnName(EndReasonColumn);
        }
    }
}
