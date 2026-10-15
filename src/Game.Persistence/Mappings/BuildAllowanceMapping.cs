using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class BuildAllowanceMapping : IEntityTypeConfiguration<BuildAllowance>
    {
        private const string BuildAllowanceTable = "build_allowance";
        private const string PlayerCountColumn = "player_count";
        private const string RoundNumberColumn = "round_number";
        private const string TurnNumberColumn = "turn_number";
        private const string BuildingCountColumn = "building_count";

        public void Configure(EntityTypeBuilder<BuildAllowance> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(BuildAllowanceTable);
            builder.HasKey(mappedAllowance => new { mappedAllowance.PlayerCount, mappedAllowance.RoundNumber, mappedAllowance.TurnNumber });
            builder.Property(mappedAllowance => mappedAllowance.PlayerCount).HasColumnName(PlayerCountColumn);
            builder.Property(mappedAllowance => mappedAllowance.RoundNumber).HasColumnName(RoundNumberColumn);
            builder.Property(mappedAllowance => mappedAllowance.TurnNumber).HasColumnName(TurnNumberColumn);
            builder.Property(mappedAllowance => mappedAllowance.BuildingCount).HasColumnName(BuildingCountColumn);
        }
    }
}
