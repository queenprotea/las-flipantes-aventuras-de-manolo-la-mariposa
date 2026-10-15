using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class PasswordRecoveryMapping : IEntityTypeConfiguration<PasswordRecovery>
    {
        private const string PasswordRecoveryTable = "password_recovery";
        private const string PlayerIdColumn = "player_id";
        private const string CodeColumn = "code";
        private const string CreatedAtColumn = "created_at";

        public void Configure(EntityTypeBuilder<PasswordRecovery> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(PasswordRecoveryTable);
            builder.HasKey(mappedRecovery => mappedRecovery.PlayerId);
            builder.Property(mappedRecovery => mappedRecovery.PlayerId)
                .HasColumnName(PlayerIdColumn)
                .ValueGeneratedNever();
            builder.Property(mappedRecovery => mappedRecovery.Code).HasColumnName(CodeColumn);
            builder.Property(mappedRecovery => mappedRecovery.CreatedAt)
                .HasColumnName(CreatedAtColumn)
                .ValueGeneratedOnAdd();
        }
    }
}
