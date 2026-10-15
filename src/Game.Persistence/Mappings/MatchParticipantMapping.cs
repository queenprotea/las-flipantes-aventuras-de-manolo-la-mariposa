using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class MatchParticipantMapping : IEntityTypeConfiguration<MatchParticipant>
    {
        private const string MatchParticipantTable = "match_participant";
        private const string MatchIdColumn = "match_id";
        private const string SeatNumberColumn = "seat_number";
        private const string PlayerIdColumn = "player_id";
        private const string FinalScoreColumn = "final_score";
        private const string FinalPositionColumn = "final_position";
        private const string WasWithdrawnColumn = "was_withdrawn";

        public void Configure(EntityTypeBuilder<MatchParticipant> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(MatchParticipantTable);
            builder.HasKey(mappedParticipant => new { mappedParticipant.MatchId, mappedParticipant.SeatNumber });
            builder.Property(mappedParticipant => mappedParticipant.MatchId).HasColumnName(MatchIdColumn);
            builder.Property(mappedParticipant => mappedParticipant.SeatNumber).HasColumnName(SeatNumberColumn);
            builder.Property(mappedParticipant => mappedParticipant.PlayerId).HasColumnName(PlayerIdColumn);
            builder.Property(mappedParticipant => mappedParticipant.FinalScore).HasColumnName(FinalScoreColumn);
            builder.Property(mappedParticipant => mappedParticipant.FinalPosition).HasColumnName(FinalPositionColumn);
            builder.Property(mappedParticipant => mappedParticipant.WasWithdrawn).HasColumnName(WasWithdrawnColumn);
        }
    }
}
