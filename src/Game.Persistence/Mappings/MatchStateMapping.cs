using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class MatchStateMapping : IEntityTypeConfiguration<MatchState>
    {
        private const string MatchStateTable = "match_state";
        private const string MatchIdColumn = "match_id";
        private const string RoundNumberColumn = "round_number";
        private const string TurnNumberColumn = "turn_number";
        private const string ActiveSeatNumberColumn = "active_seat_number";
        private const string StateDocumentColumn = "state_document";
        private const string SavedAtColumn = "saved_at";
        private const string JsonDocumentColumnType = "jsonb";

        public void Configure(EntityTypeBuilder<MatchState> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(MatchStateTable);
            builder.HasKey(mappedState => mappedState.MatchId);
            builder.Property(mappedState => mappedState.MatchId)
                .HasColumnName(MatchIdColumn)
                .ValueGeneratedNever();
            builder.Property(mappedState => mappedState.RoundNumber).HasColumnName(RoundNumberColumn);
            builder.Property(mappedState => mappedState.TurnNumber).HasColumnName(TurnNumberColumn);
            builder.Property(mappedState => mappedState.ActiveSeatNumber).HasColumnName(ActiveSeatNumberColumn);
            builder.Property(mappedState => mappedState.StateDocument)
                .HasColumnName(StateDocumentColumn)
                .HasColumnType(JsonDocumentColumnType);
            builder.Property(mappedState => mappedState.SavedAt)
                .HasColumnName(SavedAtColumn)
                .ValueGeneratedOnAdd();
        }
    }
}
