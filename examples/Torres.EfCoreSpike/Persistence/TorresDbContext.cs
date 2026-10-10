using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Torres.EfCoreSpike.Persistence
{
    public sealed class TorresDbContext : DbContext
    {
        private const string PlayerTable = "player";
        private const string PlayerIdColumn = "player_id";
        private const string UsernameColumn = "username";
        private const string EmailColumn = "email";
        private const string PasswordHashColumn = "password_hash";
        private const string AvatarReferenceColumn = "avatar_reference";
        private const string CreatedAtColumn = "created_at";

        public TorresDbContext(DbContextOptions<TorresDbContext> contextOptions)
            : base(contextOptions)
        {
        }

        public DbSet<Player> Players => Set<Player>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            modelBuilder.Entity<Player>(MapPlayer);
        }

        private static void MapPlayer(EntityTypeBuilder<Player> player)
        {
            player.ToTable(PlayerTable);
            player.HasKey(mappedPlayer => mappedPlayer.PlayerId);
            player.Property(mappedPlayer => mappedPlayer.PlayerId)
                .HasColumnName(PlayerIdColumn)
                .ValueGeneratedOnAdd();
            player.Property(mappedPlayer => mappedPlayer.Username).HasColumnName(UsernameColumn);
            player.Property(mappedPlayer => mappedPlayer.Email).HasColumnName(EmailColumn);
            player.Property(mappedPlayer => mappedPlayer.PasswordHash).HasColumnName(PasswordHashColumn);
            player.Property(mappedPlayer => mappedPlayer.AvatarReference).HasColumnName(AvatarReferenceColumn);
            player.Property(mappedPlayer => mappedPlayer.CreatedAt)
                .HasColumnName(CreatedAtColumn)
                .ValueGeneratedOnAdd();
        }
    }
}
