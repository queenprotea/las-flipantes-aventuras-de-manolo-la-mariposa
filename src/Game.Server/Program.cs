using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CoreWCF;
using CoreWCF.Configuration;

using DotNetEnv;

using Game.Contracts;
using Game.Persistence;
using Game.Server;
using Game.Services;

using log4net;
using log4net.Config;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

const string LogConfigurationFile = "log4net.config";
const string HostLoggerName = "Game.Server.Host";
const string ServerListeningMessage = "Server listening on net.tcp port {0}";
const string StartupDatabaseFailedMessage = "The database could not be used while the server started; the server does not start.";
const string AvatarFolderFailedMessage = "The avatar folder could not be created; the server does not start.";

Env.TraversePath().Load();
XmlConfigurator.Configure(
    LogManager.GetRepository(typeof(ServerStatusService).Assembly),
    new FileInfo(Path.Combine(AppContext.BaseDirectory, LogConfigurationFile)));
ILog hostLog = LogManager.GetLogger(typeof(ServerStatusService).Assembly, HostLoggerName);

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
MatchBoardSettings matchBoardSettings = MatchBoardSettings.ReadFromConfiguration(builder.Configuration);
List<MatchCell> startingFlowerCells = matchBoardSettings.StartingFlowers.Select(startingFlower => new MatchCell { Row = startingFlower.Row, Column = startingFlower.Column }).ToList();

builder.WebHost.UseNetTcp(serverSettings.NetTcpPort);
builder.Services.AddServiceModelServices();
builder.Services.AddDbContextFactory<TorresDbContext>(databaseSettings.ConfigureContext);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(new SessionTimings(TimeSpan.FromSeconds(sessionSettings.ConnectionCheckPeriodSeconds), sessionSettings.ConnectionCheckMissesToDrop));
builder.Services.AddSingleton(new SessionTokenIssuer(sessionSigningKey, TimeProvider.System));
builder.Services.AddSingleton(new HistoryPaging(historySettings.MatchesPerPage));
builder.Services.AddSingleton(new StartingBoard(startingFlowerCells));
builder.Services.AddSingleton<GuestIdIssuer>();
builder.Services.AddSingleton<PlayerRepository>();
builder.Services.AddSingleton<RankingRepository>();
builder.Services.AddSingleton<RoomRecordRepository>();
builder.Services.AddSingleton<SanctionRepository>();
builder.Services.AddSingleton<SanctionLevelRepository>();
builder.Services.AddSingleton<ReportRepository>();
builder.Services.AddSingleton<SystemParameterRepository>();
builder.Services.AddSingleton<SystemParameterReader>();
builder.Services.AddSingleton<FriendshipRepository>();
builder.Services.AddSingleton<RoomInvitationRepository>();
builder.Services.AddSingleton<MatchRecordRepository>();
builder.Services.AddSingleton<MatchParticipantRepository>();
builder.Services.AddSingleton<ServerStatusService>();
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

RoomStartup roomStartup = app.Services.GetRequiredService<RoomStartup>();
TimeSpan roomInactivityLimit;
ChatCapacity chatCapacity;
SanctionPolicy sanctionPolicy;
try
{
    await roomStartup.CloseLeftoverRoomsAsync();
    roomInactivityLimit = await roomStartup.ReadInactivityLimitAsync();
    chatCapacity = await roomStartup.ReadChatCapacityAsync();
    sanctionPolicy = await roomStartup.ReadSanctionPolicyAsync();
}
catch (NpgsqlException exception)
{
    hostLog.Fatal(StartupDatabaseFailedMessage, exception);
    return;
}

app.Services.GetRequiredService<ChatService>().Start(chatCapacity);
app.Services.GetRequiredService<ReportRecorder>().Start(sanctionPolicy);

app.Services.GetRequiredService<MatchService>();
app.UseServiceModel(AddEndpoints);
ConnectionSupervisor connectionSupervisor = app.Services.GetRequiredService<ConnectionSupervisor>();
connectionSupervisor.Start(roomInactivityLimit);

hostLog.InfoFormat(ServerListeningMessage, serverSettings.NetTcpPort);
await app.RunAsync();
await connectionSupervisor.StopAsync();

static void AddEndpoints(IServiceBuilder serviceBuilder)
{
    serviceBuilder.AddService<ServerStatusService>();
    serviceBuilder.AddServiceEndpoint<ServerStatusService, IServerStatusService>(
        new NetTcpBinding(SecurityMode.None),
        ServiceEndpointPaths.ServerStatus);
    serviceBuilder.AddService<AccountService>();
    serviceBuilder.AddServiceEndpoint<AccountService, IAccountService>(
        new NetTcpBinding(SecurityMode.None),
        ServiceEndpointPaths.Account);
    serviceBuilder.AddService<RankingService>();
    serviceBuilder.AddServiceEndpoint<RankingService, IRankingService>(
        new NetTcpBinding(SecurityMode.None),
        ServiceEndpointPaths.Ranking);
    serviceBuilder.AddService<GameSessionService>();
    serviceBuilder.AddServiceEndpoint<GameSessionService, IGameSessionService>(
        CreateGameSessionBinding(),
        ServiceEndpointPaths.GameSession);
}

static NetTcpBinding CreateGameSessionBinding()
{
    var gameSessionBinding = new NetTcpBinding(SecurityMode.None);
    gameSessionBinding.ReaderQuotas.MaxArrayLength = ProfileLimits.LargestArrayBytes;

    return gameSessionBinding;
}
