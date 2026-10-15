using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class SystemParameterMapping : IEntityTypeConfiguration<SystemParameter>
    {
        private const string SystemParameterTable = "system_parameter";
        private const string ParameterCodeColumn = "parameter_code";
        private const string ParameterValueColumn = "parameter_value";

        public void Configure(EntityTypeBuilder<SystemParameter> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(SystemParameterTable);
            builder.HasKey(mappedParameter => mappedParameter.ParameterCode);
            builder.Property(mappedParameter => mappedParameter.ParameterCode).HasColumnName(ParameterCodeColumn);
            builder.Property(mappedParameter => mappedParameter.ParameterValue).HasColumnName(ParameterValueColumn);
        }
    }
}
