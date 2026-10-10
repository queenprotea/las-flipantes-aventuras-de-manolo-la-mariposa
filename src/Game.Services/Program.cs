using System;
using System.IO;

using CoreWCF;
using CoreWCF.Configuration;

using DotNetEnv;

using Game.Contracts;
using Game.Persistence;
using Game.Services;

using log4net;
using log4net.Config;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

const string LogConfigurationFile = "log4net.config";
const string HostLoggerName = "Game.Services.Host";
const string ServerListeningMessage = "Server listening on net.tcp port {0}";

Env.TraversePath().Load();
XmlConfigurator.Configure(
    LogManager.GetRepository(typeof(ServerStatusService).Assembly),
    new FileInfo(Path.Combine(AppContext.BaseDirectory, LogConfigurationFile)));
ILog hostLog = LogManager.GetLogger(typeof(ServerStatusService).Assembly, HostLoggerName);

DatabaseSettings databaseSettings = DatabaseSettings.ReadFromEnvironment();

var hostOptions = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
};
WebApplicationBuilder builder = WebApplication.CreateBuilder(hostOptions);
ServerSettings serverSettings = ServerSettings.Read(builder.Configuration);

builder.WebHost.UseNetTcp(serverSettings.NetTcpPort);
builder.Services.AddServiceModelServices();
builder.Services.AddDbContextFactory<TorresDbContext>(databaseSettings.ConfigureContext);
builder.Services.AddSingleton<PlayerRepository>();
builder.Services.AddSingleton<ServerStatusService>();
builder.Services.AddSingleton<AccountService>();
builder.Services.AddSingleton<RankingRepository>();
builder.Services.AddSingleton<RankingService>();

WebApplication app = builder.Build();
app.UseServiceModel(AddEndpoints);

hostLog.InfoFormat(ServerListeningMessage, serverSettings.NetTcpPort);
await app.RunAsync();

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
}
