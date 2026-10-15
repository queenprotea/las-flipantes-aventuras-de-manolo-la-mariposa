using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class SanctionMapping : IEntityTypeConfiguration<Sanction>
    {
        private const string SanctionTable = "sanction";
        private const string SanctionIdColumn = "sanction_id";
        private const string PlayerIdColumn = "player_id";
        private const string LevelColumn = "level";
        private const string IsPermanentColumn = "is_permanent";
        private const string ThresholdAtColumn = "threshold_at";
        private const string EffectiveFromColumn = "effective_from";
        private const string EffectiveUntilColumn = "effective_until";

        public void Configure(EntityTypeBuilder<Sanction> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(SanctionTable);
            builder.HasKey(mappedSanction => mappedSanction.SanctionId);
            builder.Property(mappedSanction => mappedSanction.SanctionId)
                .HasColumnName(SanctionIdColumn)
                .ValueGeneratedOnAdd();
            builder.Property(mappedSanction => mappedSanction.PlayerId).HasColumnName(PlayerIdColumn);
            builder.Property(mappedSanction => mappedSanction.Level).HasColumnName(LevelColumn);
            builder.Property(mappedSanction => mappedSanction.IsPermanent).HasColumnName(IsPermanentColumn);
            builder.Property(mappedSanction => mappedSanction.ThresholdAt).HasColumnName(ThresholdAtColumn);
            builder.Property(mappedSanction => mappedSanction.EffectiveFrom).HasColumnName(EffectiveFromColumn);
            builder.Property(mappedSanction => mappedSanction.EffectiveUntil).HasColumnName(EffectiveUntilColumn);
        }
    }
}
