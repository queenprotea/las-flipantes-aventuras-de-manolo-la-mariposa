using System;

using Microsoft.EntityFrameworkCore;

namespace Game.Persistence
{
    public sealed class TorresDbContext : DbContext
    {
        public TorresDbContext(DbContextOptions<TorresDbContext> contextOptions)
            : base(contextOptions)
        {
        }

        public DbSet<Player> Players => Set<Player>();

        public DbSet<PlayerRanking> PlayerRankings => Set<PlayerRanking>();

        public DbSet<Friendship> Friendships => Set<Friendship>();

        public DbSet<RoomRecord> RoomRecords => Set<RoomRecord>();

        public DbSet<RoomInvitationRecord> RoomInvitationRecords => Set<RoomInvitationRecord>();

        public DbSet<MatchRecord> MatchRecords => Set<MatchRecord>();

        public DbSet<MatchParticipant> MatchParticipants => Set<MatchParticipant>();

        public DbSet<MatchState> MatchStates => Set<MatchState>();

        public DbSet<PasswordRecovery> PasswordRecoveries => Set<PasswordRecovery>();

        public DbSet<Report> Reports => Set<Report>();

        public DbSet<Sanction> Sanctions => Set<Sanction>();

        public DbSet<TurnLayout> TurnLayouts => Set<TurnLayout>();

        public DbSet<BuildAllowance> BuildAllowances => Set<BuildAllowance>();

        public DbSet<ActionCost> ActionCosts => Set<ActionCost>();

        public DbSet<SanctionLevel> SanctionLevels => Set<SanctionLevel>();

        public DbSet<SystemParameter> SystemParameters => Set<SystemParameter>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TorresDbContext).Assembly);
        }
    }
}
