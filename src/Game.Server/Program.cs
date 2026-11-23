using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;

using DotNetEnv;

using Game.Contracts;
using Game.Persistence;
using Game.Server;
using Game.Services;

using log4net;
using log4net.Config;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

const string LogConfigurationFile = "log4net.config";
const string HostLoggerName = "Game.Server.Host";
const string ServerListeningMessage = "Server listening on net.tcp port {0}";
const string MetadataPublishedMessage = "Service metadata published on {0}";
const string MetadataHostName = "localhost";
const string StartupDatabaseFailedMessage = "The database could not be used while the server started; the server does not start.";
const string AvatarFolderFailedMessage = "The avatar folder could not be created; the server does not start.";
const string EmailServerMessage = "Emails are delivered to {0}:{1}";
const string StartingBoardFailedMessage = "The starting board in the configuration is not valid; the server does not start.";
const string StoredConfigurationFailedMessage = "The configuration stored in the database is not valid; the server does not start.";
const string UnhandledFailureMessage = "An unexpected failure stopped the server.";

Env.TraversePath().Load();
XmlConfigurator.Configure(
    LogManager.GetRepository(typeof(ServerStatusService).Assembly),
    new FileInfo(Path.Combine(AppContext.BaseDirectory, LogConfigurationFile)));
ILog hostLog = LogManager.GetLogger(typeof(ServerStatusService).Assembly, HostLoggerName);
AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

ConnectionSupervisor? startedSupervisor = null;
MatchService? recordingMatchService = null;
try
{
    DatabaseSettings databaseSettings = DatabaseSettings.ReadFromEnvironment();
    byte[] sessionSigningKey = SessionSettings.ReadSigningKey();

    var hostOptions = new WebApplicationOptions
    {
        Args = args,
        ContentRootPath = AppContext.BaseDirectory,
    };
    WebApplicationBuilder builder = WebApplication.CreateBuilder(hostOptions);
    ServerSettings serverSettings = ServerSettings.Read(builder.Configuration);
    SessionSettings sessionSettings = SessionSettings.Read(builder.Configuration);
    HistorySettings historySettings = HistorySettings.Read(builder.Configuration);
    AvatarSettings avatarSettings = AvatarSettings.Read(builder.Configuration);
    TimeSpan avatarUploadIdleLimit = TimeSpan.FromSeconds(avatarSettings.UploadIdleSeconds);
    var avatarStore = new AvatarStore(avatarSettings.ResolveFolderPath());
    try
    {
        avatarStore.EnsureFolder();
    }
    catch (IOException exception)
    {
        hostLog.Fatal(AvatarFolderFailedMessage, exception);
        return;
    }
    catch (UnauthorizedAccessException exception)
    {
        hostLog.Fatal(AvatarFolderFailedMessage, exception);
        return;
    }
    EmailSettings emailSettings = EmailSettings.Read(builder.Configuration);
    EmailSender emailSender = emailSettings.CreateSender();
    MatchBoardSettings matchBoardSettings = MatchBoardSettings.ReadFromConfiguration(builder.Configuration);
    List<MatchCell> startingFlowerCells = matchBoardSettings.StartingFlowers.Select(startingFlower => new MatchCell { Row = startingFlower.Row, Column = startingFlower.Column }).ToList();

    Uri metadataBaseAddress = new UriBuilder(Uri.UriSchemeHttp, MetadataHostName, serverSettings.MetadataHttpPort).Uri;
    builder.WebHost.UseNetTcp(serverSettings.NetTcpPort);
    builder.Services.AddServiceModelServices();
    if (serverSettings.PublishesServiceMetadata)
    {
        builder.WebHost.ConfigureKestrel(kestrelOptions => kestrelOptions.ListenLocalhost(serverSettings.MetadataHttpPort));
        builder.Services.AddServiceModelMetadata();
    }

    builder.Services.AddDbContextFactory<TorresDbContext>(databaseSettings.ConfigureContext);
    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddSingleton(Random.Shared);
    builder.Services.AddSingleton(new SessionTimings(TimeSpan.FromSeconds(sessionSettings.ConnectionCheckPeriodSeconds), sessionSettings.ConnectionCheckMissesToDrop));
    builder.Services.AddSingleton(new SessionTokenIssuer(sessionSigningKey, TimeProvider.System));
    builder.Services.AddSingleton(new HistoryPaging(historySettings.MatchesPerPage));
    builder.Services.AddSingleton(new StartingBoard(startingFlowerCells));
    builder.Services.AddSingleton<GuestIdIssuer>();
    builder.Services.AddSingleton<IEmailSender>(emailSender);
    builder.Services.AddSingleton<PendingRegistrationRegistry>();
    builder.Services.AddSingleton<PlayerRepository>();
    builder.Services.AddSingleton<RankingRepository>();
    builder.Services.AddSingleton<RoomRecordRepository>();
    builder.Services.AddSingleton<SanctionRepository>();
    builder.Services.AddSingleton<SanctionLevelRepository>();
    builder.Services.AddSingleton<ReportRepository>();
    builder.Services.AddSingleton<SystemParameterRepository>();
    builder.Services.AddSingleton<SystemParameterReader>();
    builder.Services.AddSingleton<TurnLayoutRepository>();
    builder.Services.AddSingleton<BuildAllowanceRepository>();
    builder.Services.AddSingleton<ActionCostRepository>();
    builder.Services.AddSingleton<MatchRuleReader>();
    builder.Services.AddSingleton<FriendshipRepository>();
    builder.Services.AddSingleton<RoomInvitationRepository>();
    builder.Services.AddSingleton<PasswordRecoveryRepository>();
    builder.Services.AddSingleton<MatchRecordRepository>();
    builder.Services.AddSingleton<MatchParticipantRepository>();
    builder.Services.AddSingleton<MatchStateRepository>();
    builder.Services.AddSingleton<ServerStatusService>();
    builder.Services.AddSingleton<AccountRegistration>();
    builder.Services.AddSingleton<AccessRecovery>();
    builder.Services.AddSingleton<AccountService>();
    builder.Services.AddSingleton<RankingService>();
    builder.Services.AddSingleton<SessionRegistry>();
    builder.Services.AddSingleton<RoomRegistry>();
    builder.Services.AddSingleton<RoomService>();
    builder.Services.AddSingleton<SessionService>();
    builder.Services.AddSingleton<FriendsService>();
    builder.Services.AddSingleton<InvitationService>();
    builder.Services.AddSingleton<ReportRecorder>();
    builder.Services.AddSingleton<ChatService>();
    builder.Services.AddSingleton<HistoryService>();
    builder.Services.AddSingleton<MatchService>();
    builder.Services.AddSingleton(avatarStore);
    builder.Services.AddSingleton(serviceProvider => new AvatarUploadRegistry(serviceProvider.GetRequiredService<SessionRegistry>(), TimeProvider.System, avatarUploadIdleLimit));
    builder.Services.AddSingleton<ProfileService>();
    builder.Services.AddSingleton<GameSessionService>();
    builder.Services.AddSingleton<RoomStartup>();
    builder.Services.AddSingleton<ConnectionSupervisor>();

    WebApplication app = builder.Build();

    MatchService matchService;
    try
    {
        matchService = app.Services.GetRequiredService<MatchService>();
        recordingMatchService = matchService;
    }
    catch (ArgumentException exception)
    {
        hostLog.Fatal(StartingBoardFailedMessage, exception);
        return;
    }

    RoomStartup roomStartup = app.Services.GetRequiredService<RoomStartup>();
    TimeSpan roomInactivityLimit;
    int codeLifetimeMinutes;
    ChatCapacity chatCapacity;
    SanctionPolicy sanctionPolicy;
    try
    {
        await roomStartup.CloseLeftoverRoomsAsync();
        roomInactivityLimit = await roomStartup.ReadInactivityLimitAsync();
        codeLifetimeMinutes = await roomStartup.ReadCodeLifetimeMinutesAsync();
        chatCapacity = await roomStartup.ReadChatCapacityAsync();
        sanctionPolicy = await roomStartup.ReadSanctionPolicyAsync();
        await matchService.LoadRulesAsync();
        await matchService.ResumeInterruptedMatchesAsync();
    }
    catch (NpgsqlException exception)
    {
        hostLog.Fatal(StartupDatabaseFailedMessage, exception);
        return;
    }
    catch (InvalidOperationException exception)
    {
        hostLog.Fatal(StoredConfigurationFailedMessage, exception);
        return;
    }

    app.Services.GetRequiredService<AccountRegistration>().Start(codeLifetimeMinutes);
    app.Services.GetRequiredService<AccessRecovery>().Start(codeLifetimeMinutes);
    app.Services.GetRequiredService<ChatService>().Start(chatCapacity);
    app.Services.GetRequiredService<ReportRecorder>().Start(sanctionPolicy);

    if (serverSettings.PublishesServiceMetadata)
    {
        ServiceMetadataBehavior metadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
        metadataBehavior.HttpGetEnabled = true;
        hostLog.InfoFormat(MetadataPublishedMessage, metadataBaseAddress);
    }

    app.UseServiceModel(AddEndpoints);
    ConnectionSupervisor connectionSupervisor = app.Services.GetRequiredService<ConnectionSupervisor>();
    connectionSupervisor.Start(roomInactivityLimit);
    startedSupervisor = connectionSupervisor;

    hostLog.InfoFormat(EmailServerMessage, emailSettings.ChosenHost, emailSettings.ChosenPort);
    hostLog.InfoFormat(ServerListeningMessage, serverSettings.NetTcpPort);
    await app.RunAsync();

    void AddEndpoints(IServiceBuilder serviceBuilder)
    {
        serviceBuilder.AddService<ServerStatusService>(serviceOptions => AddMetadataAddress(serviceOptions, ServiceEndpointPaths.ServerStatus));
        serviceBuilder.AddServiceEndpoint<ServerStatusService, IServerStatusService>(
            new NetTcpBinding(SecurityMode.None),
            ServiceEndpointPaths.ServerStatus);
        serviceBuilder.AddService<AccountService>(serviceOptions => AddMetadataAddress(serviceOptions, ServiceEndpointPaths.Account));
        serviceBuilder.AddServiceEndpoint<AccountService, IAccountService>(
            new NetTcpBinding(SecurityMode.None),
            ServiceEndpointPaths.Account);
        serviceBuilder.AddService<RankingService>(serviceOptions => AddMetadataAddress(serviceOptions, ServiceEndpointPaths.Ranking));
        serviceBuilder.AddServiceEndpoint<RankingService, IRankingService>(
            new NetTcpBinding(SecurityMode.None),
            ServiceEndpointPaths.Ranking);
        serviceBuilder.AddService<GameSessionService>(serviceOptions => AddMetadataAddress(serviceOptions, ServiceEndpointPaths.GameSession));
        serviceBuilder.AddServiceEndpoint<GameSessionService, IGameSessionService>(
            CreateGameSessionBinding(),
            ServiceEndpointPaths.GameSession);
    }

    void AddMetadataAddress(ServiceOptions serviceOptions, string servicePath)
    {
        if (serverSettings.PublishesServiceMetadata)
        {
            var metadataAddress = new Uri(metadataBaseAddress, servicePath);
            serviceOptions.BaseAddresses.Add(metadataAddress);
        }
    }
}
catch (Exception exception)
{
    hostLog.Fatal(UnhandledFailureMessage, exception);
}

if (startedSupervisor is not null)
{
    await startedSupervisor.StopAsync();
}

if (recordingMatchService is not null)
{
    await recordingMatchService.FinishRecordingAsync();
}

void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    if (e.ExceptionObject is Exception unhandledException)
    {
        hostLog.Fatal(UnhandledFailureMessage, unhandledException);
    }
}

static NetTcpBinding CreateGameSessionBinding()
{
    var gameSessionBinding = new NetTcpBinding(SecurityMode.None);
    gameSessionBinding.ReaderQuotas.MaxArrayLength = ProfileLimits.LargestArrayBytes;

    return gameSessionBinding;
}
