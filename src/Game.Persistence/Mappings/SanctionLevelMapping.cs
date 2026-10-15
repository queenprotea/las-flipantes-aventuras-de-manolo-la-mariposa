using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class SanctionLevelMapping : IEntityTypeConfiguration<SanctionLevel>
    {
        private const string SanctionLevelTable = "sanction_level";
        private const string LevelColumn = "level";
        private const string BanDurationColumn = "ban_duration";

        public void Configure(EntityTypeBuilder<SanctionLevel> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(SanctionLevelTable);
            builder.HasKey(mappedLevel => mappedLevel.Level);
            builder.Property(mappedLevel => mappedLevel.Level)
                .HasColumnName(LevelColumn)
                .ValueGeneratedNever();
            builder.Property(mappedLevel => mappedLevel.BanDuration).HasColumnName(BanDurationColumn);
        }
    }
}
