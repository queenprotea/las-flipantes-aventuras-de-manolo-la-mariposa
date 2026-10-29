using System;
using System.IO;

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

builder.WebHost.UseNetTcp(serverSettings.NetTcpPort);
builder.Services.AddServiceModelServices();
builder.Services.AddDbContextFactory<TorresDbContext>(databaseSettings.ConfigureContext);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(new SessionTimings(TimeSpan.FromSeconds(sessionSettings.HeartbeatPeriodSeconds), sessionSettings.HeartbeatMissesToDrop));
builder.Services.AddSingleton(new SessionTokenIssuer(sessionSigningKey, TimeProvider.System));
builder.Services.AddSingleton<PlayerRepository>();
builder.Services.AddSingleton<RankingRepository>();
builder.Services.AddSingleton<RoomRecordRepository>();
builder.Services.AddSingleton<SanctionRepository>();
builder.Services.AddSingleton<SystemParameterRepository>();
builder.Services.AddSingleton<ServerStatusService>();
builder.Services.AddSingleton<AccountService>();
builder.Services.AddSingleton<RankingService>();
builder.Services.AddSingleton<SessionRegistry>();
builder.Services.AddSingleton<RoomRegistry>();
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<SessionService>();
builder.Services.AddSingleton<GameSessionService>();
builder.Services.AddSingleton<RoomStartup>();
builder.Services.AddSingleton<ConnectionSupervisor>();

WebApplication app = builder.Build();

RoomStartup roomStartup = app.Services.GetRequiredService<RoomStartup>();
TimeSpan roomInactivityLimit;
try
{
    await roomStartup.CloseLeftoverRoomsAsync();
    roomInactivityLimit = await roomStartup.ReadInactivityLimitAsync();
}
catch (NpgsqlException exception)
{
    hostLog.Fatal(StartupDatabaseFailedMessage, exception);
    return;
}

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
        new NetTcpBinding(SecurityMode.None),
        ServiceEndpointPaths.GameSession);
}
