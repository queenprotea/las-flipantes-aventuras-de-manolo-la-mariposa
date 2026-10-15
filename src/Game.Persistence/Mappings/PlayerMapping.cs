using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Persistence
{
    public sealed class PlayerMapping : IEntityTypeConfiguration<Player>
    {
        private const string PlayerTable = "player";
        private const string PlayerIdColumn = "player_id";
        private const string UsernameColumn = "username";
        private const string EmailColumn = "email";
        private const string PasswordHashColumn = "password_hash";
        private const string AvatarReferenceColumn = "avatar_reference";
        private const string CreatedAtColumn = "created_at";

        public void Configure(EntityTypeBuilder<Player> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable(PlayerTable);
            builder.HasKey(mappedPlayer => mappedPlayer.PlayerId);
            builder.Property(mappedPlayer => mappedPlayer.PlayerId)
                .HasColumnName(PlayerIdColumn)
                .ValueGeneratedOnAdd();
            builder.Property(mappedPlayer => mappedPlayer.Username).HasColumnName(UsernameColumn);
            builder.Property(mappedPlayer => mappedPlayer.Email).HasColumnName(EmailColumn);
            builder.Property(mappedPlayer => mappedPlayer.PasswordHash).HasColumnName(PasswordHashColumn);
            builder.Property(mappedPlayer => mappedPlayer.AvatarReference).HasColumnName(AvatarReferenceColumn);
            builder.Property(mappedPlayer => mappedPlayer.CreatedAt)
                .HasColumnName(CreatedAtColumn)
                .ValueGeneratedOnAdd();
        }
    }
}
