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
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton(Random.Shared);
            services.AddSingleton(new MaintenanceTiming(hostSettings.Session.MaintenancePeriod));
            services.AddSingleton(new SessionTokenIssuer(hostSettings.SessionSigningKey, TimeProvider.System));
            services.AddSingleton(new HistoryPaging(hostSettings.History.MatchesPerPage));
            services.AddSingleton(new StartingBoard(hostSettings.StartingFlowerCells));
            services.AddSingleton(new AvatarUploadTiming(TimeSpan.FromSeconds(hostSettings.Avatar.UploadIdleSeconds)));
            services.AddSingleton(new AvatarStore(hostSettings.Avatar.ResolveFolderPath()));
            services.AddSingleton<IEmailSender>(hostSettings.EmailSender);
        }

        public static void AddRepositories(IServiceCollection services, DatabaseSettings databaseSettings)
        {
            services.AddDbContextFactory<TorresDbContext>(databaseSettings.ConfigureContext);
            services.AddSingleton<IPlayerRepository, PlayerRepository>();
            services.AddSingleton<IPasswordRecoveryRepository, PasswordRecoveryRepository>();
            services.AddSingleton<IRankingRepository, RankingRepository>();
            services.AddSingleton<IFriendshipRepository, FriendshipRepository>();
            services.AddSingleton<IRoomRecordRepository, RoomRecordRepository>();
            services.AddSingleton<IRoomInvitationRepository, RoomInvitationRepository>();
            services.AddSingleton<IReportRepository, ReportRepository>();
            services.AddSingleton<ISanctionRepository, SanctionRepository>();
            services.AddSingleton<SanctionLevelRepository>();
            services.AddSingleton<ISystemParameterRepository, SystemParameterRepository>();
            services.AddSingleton<ITurnLayoutRepository, TurnLayoutRepository>();
            services.AddSingleton<IBuildAllowanceRepository, BuildAllowanceRepository>();
            services.AddSingleton<IActionCostRepository, ActionCostRepository>();
            services.AddSingleton<IMatchRecordRepository, MatchRecordRepository>();
            services.AddSingleton<IMatchParticipantRepository, MatchParticipantRepository>();
            services.AddSingleton<IMatchStateRepository, MatchStateRepository>();
        }

        public static void AddGameServices(IServiceCollection services)
        {
            services.AddSingleton<ServerStatusService>();
            services.AddSingleton<SystemParameterReader>();

            services.AddSingleton<PendingRegistrationRegistry>();
            services.AddSingleton<VerificationCodeLifetime>();
            services.AddSingleton<AccountRegistration>();
            services.AddSingleton<AccessRecovery>();
            services.AddSingleton<AccountService>();

            services.AddSingleton<GuestIdIssuer>();
            services.AddSingleton<ICallerChannel, CallerChannel>();
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
            services.AddSingleton<MaintenanceSupervisor>();
        }
    }
}
