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

using Npgsql;

const int NetTcpPort = 8000;
const string StatusPath = "/status";
const string LogConfigurationFile = "log4net.config";

Env.TraversePath().Load();
XmlConfigurator.Configure(
    LogManager.GetRepository(typeof(ServerStatusService).Assembly),
    new FileInfo(Path.Combine(AppContext.BaseDirectory, LogConfigurationFile)));
ILog log = LogManager.GetLogger(typeof(ServerStatusService).Assembly, "Game.Services.Host");

await using NpgsqlDataSource dataSource = DatabaseSettings.CreateDataSource();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseNetTcp(NetTcpPort);
builder.Services.AddServiceModelServices();
builder.Services.AddSingleton(dataSource);
builder.Services.AddSingleton<PlayerRepository>();
builder.Services.AddSingleton<ServerStatusService>();

WebApplication app = builder.Build();
app.UseServiceModel(AddEndpoints);

log.InfoFormat("Server listening on net.tcp://localhost:{0}{1}", NetTcpPort, StatusPath);
await app.RunAsync();

static void AddEndpoints(IServiceBuilder serviceBuilder)
{
    serviceBuilder.AddService<ServerStatusService>();
    serviceBuilder.AddServiceEndpoint<ServerStatusService, IServerStatusService>(
        new NetTcpBinding(SecurityMode.None),
        StatusPath);
}
