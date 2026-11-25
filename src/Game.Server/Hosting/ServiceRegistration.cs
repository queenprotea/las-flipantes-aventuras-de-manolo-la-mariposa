using System;

using Game.Persistence;
using Game.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Game.Server
{
    public static class ServiceRegistration
    {
        public static void AddSettingValues(IServiceCollection services, HostSettings hostSettings)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(hostSettings);

            services.AddSingleton(TimeProvider.System);
            services.AddSingleton(Random.Shared);
            services.AddSingleton(new SessionTimings(TimeSpan.FromSeconds(hostSettings.Session.ConnectionCheckPeriodSeconds), hostSettings.Session.ConnectionCheckMissesToDrop));
            services.AddSingleton(new SessionTokenIssuer(hostSettings.SessionSigningKey, TimeProvider.System));
            services.AddSingleton(new HistoryPaging(hostSettings.History.MatchesPerPage));
            services.AddSingleton(new StartingBoard(hostSettings.StartingFlowerCells));
            services.AddSingleton(new AvatarUploadTiming(TimeSpan.FromSeconds(hostSettings.Avatar.UploadIdleSeconds)));
            services.AddSingleton(new AvatarStore(hostSettings.Avatar.ResolveFolderPath()));
            services.AddSingleton<IEmailSender>(hostSettings.EmailSender);
        }

        public static void AddRepositories(IServiceCollection services, DatabaseSettings databaseSettings)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(databaseSettings);

            services.AddDbContextFactory<TorresDbContext>(databaseSettings.ConfigureContext);
            services.AddSingleton<PlayerRepository>();
            services.AddSingleton<PasswordRecoveryRepository>();
            services.AddSingleton<RankingRepository>();
            services.AddSingleton<FriendshipRepository>();
            services.AddSingleton<RoomRecordRepository>();
            services.AddSingleton<RoomInvitationRepository>();
            services.AddSingleton<ReportRepository>();
            services.AddSingleton<SanctionRepository>();
            services.AddSingleton<SanctionLevelRepository>();
            services.AddSingleton<SystemParameterRepository>();
            services.AddSingleton<TurnLayoutRepository>();
            services.AddSingleton<BuildAllowanceRepository>();
            services.AddSingleton<ActionCostRepository>();
            services.AddSingleton<MatchRecordRepository>();
            services.AddSingleton<MatchParticipantRepository>();
            services.AddSingleton<MatchStateRepository>();
        }

        public static void AddGameServices(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSingleton<ServerStatusService>();
            services.AddSingleton<SystemParameterReader>();

            services.AddSingleton<PendingRegistrationRegistry>();
            services.AddSingleton<AccountRegistration>();
            services.AddSingleton<AccessRecovery>();
            services.AddSingleton<AccountService>();

            services.AddSingleton<GuestIdIssuer>();
            services.AddSingleton<SessionRegistry>();
            services.AddSingleton<SessionService>();

            services.AddSingleton<RankingService>();
            services.AddSingleton<HistoryService>();
            services.AddSingleton<FriendsService>();
            services.AddSingleton<AvatarUploadRegistry>();
            services.AddSingleton<ProfileService>();

            services.AddSingleton<RoomRegistry>();
            services.AddSingleton<RoomService>();
            services.AddSingleton<RoomStartup>();
            services.AddSingleton<InvitationService>();
            services.AddSingleton<ChatService>();
            services.AddSingleton<ReportRecorder>();

            services.AddSingleton<MatchRuleReader>();
            services.AddSingleton<MatchService>();

            services.AddSingleton<GameSessionService>();
            services.AddSingleton<ConnectionSupervisor>();
        }
    }
}
