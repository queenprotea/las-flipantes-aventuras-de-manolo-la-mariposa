using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class ReportMapping : IEntityTypeConfiguration<Report>
    {
        private const string ReportTable = "report";
        private const string ReportIdColumn = "report_id";
        private const string ReporterPlayerIdColumn = "reporter_player_id";
        private const string ReportedPlayerIdColumn = "reported_player_id";
        private const string RoomIdColumn = "room_id";
        private const string MatchIdColumn = "match_id";
        private const string ReportedTextColumn = "reported_text";
        private const string CreatedAtColumn = "created_at";

        public void Configure(EntityTypeBuilder<Report> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(ReportTable);
            builder.HasKey(mappedReport => mappedReport.ReportId);
            builder.Property(mappedReport => mappedReport.ReportId)
                .HasColumnName(ReportIdColumn)
                .ValueGeneratedOnAdd();
            builder.Property(mappedReport => mappedReport.ReporterPlayerId).HasColumnName(ReporterPlayerIdColumn);
            builder.Property(mappedReport => mappedReport.ReportedPlayerId).HasColumnName(ReportedPlayerIdColumn);
            builder.Property(mappedReport => mappedReport.RoomId).HasColumnName(RoomIdColumn);
            builder.Property(mappedReport => mappedReport.MatchId).HasColumnName(MatchIdColumn);
            builder.Property(mappedReport => mappedReport.ReportedText).HasColumnName(ReportedTextColumn);
            builder.Property(mappedReport => mappedReport.CreatedAt)
                .HasColumnName(CreatedAtColumn)
                .ValueGeneratedOnAdd();
        }
    }
}
