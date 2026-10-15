using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class ActionCostMapping : IEntityTypeConfiguration<ActionCost>
    {
        private const string ActionCostTable = "action_cost";
        private const string ActionCodeColumn = "action_code";
        private const string ActionPointCostColumn = "action_point_cost";

        public void Configure(EntityTypeBuilder<ActionCost> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(ActionCostTable);
            builder.HasKey(mappedCost => mappedCost.ActionCode);
            builder.Property(mappedCost => mappedCost.ActionCode).HasColumnName(ActionCodeColumn);
            builder.Property(mappedCost => mappedCost.ActionPointCost).HasColumnName(ActionPointCostColumn);
        }
    }
}
