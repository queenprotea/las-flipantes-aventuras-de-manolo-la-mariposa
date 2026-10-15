using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class TurnLayoutMapping : IEntityTypeConfiguration<TurnLayout>
    {
        private const string TurnLayoutTable = "turn_layout";
        private const string PlayerCountColumn = "player_count";
        private const string RoundNumberColumn = "round_number";
        private const string TurnCountColumn = "turn_count";

        public void Configure(EntityTypeBuilder<TurnLayout> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(TurnLayoutTable);
            builder.HasKey(mappedLayout => new { mappedLayout.PlayerCount, mappedLayout.RoundNumber });
            builder.Property(mappedLayout => mappedLayout.PlayerCount).HasColumnName(PlayerCountColumn);
            builder.Property(mappedLayout => mappedLayout.RoundNumber).HasColumnName(RoundNumberColumn);
            builder.Property(mappedLayout => mappedLayout.TurnCount).HasColumnName(TurnCountColumn);
        }
    }
}
