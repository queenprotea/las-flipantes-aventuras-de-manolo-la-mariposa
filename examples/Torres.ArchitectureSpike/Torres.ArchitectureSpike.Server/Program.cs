using System;
using System.IO;

using CoreWCF;
using CoreWCF.Configuration;

using log4net;
using log4net.Config;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Torres.ArchitectureSpike.Contracts;
using Torres.ArchitectureSpike.Match;
using Torres.ArchitectureSpike.Server;
using Torres.ArchitectureSpike.Services;

const string LogConfigurationFile = "log4net.config";
const string HostLoggerName = "Torres.ArchitectureSpike.Host";
const string HostOpenedMessage = "Architecture spike started on net.tcp port {0}, path {1}. Press Ctrl+C to stop.";

XmlConfigurator.Configure(
    LogManager.GetRepository(typeof(SessionRegistry).Assembly),
    new FileInfo(Path.Combine(AppContext.BaseDirectory, LogConfigurationFile)));
ILog hostLog = LogManager.GetLogger(typeof(SessionRegistry).Assembly, HostLoggerName);

var hostOptions = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
};
WebApplicationBuilder builder = WebApplication.CreateBuilder(hostOptions);
ServerSettings serverSettings = ServerSettings.Read(builder.Configuration);
MatchSettings matchSettings = MatchSettingsReader.Read(builder.Configuration);

builder.WebHost.UseNetTcp(serverSettings.NetTcpPort);
builder.Services.AddServiceModelServices();
builder.Services.AddSingleton(matchSettings);
builder.Services.AddSingleton<IMatchClock, SystemMatchClock>();
builder.Services.AddSingleton<SessionRegistry>();
builder.Services.AddSingleton<IMatchNotificationReceiver>(ResolveNotificationReceiver);
builder.Services.AddSingleton<MatchCoordinator>();
builder.Services.AddSingleton<GameSessionService>();

WebApplication app = builder.Build();
app.UseServiceModel(AddEndpoints);
app.Lifetime.ApplicationStarted.Register(HostOnOpened);

await app.RunAsync();

void HostOnOpened()
{
    hostLog.InfoFormat(HostOpenedMessage, serverSettings.NetTcpPort, ServiceEndpointPaths.GameSession);
}

static IMatchNotificationReceiver ResolveNotificationReceiver(IServiceProvider serviceProvider)
{
    SessionRegistry sessionRegistry = serviceProvider.GetRequiredService<SessionRegistry>();

    return sessionRegistry;
}

static void AddEndpoints(IServiceBuilder serviceBuilder)
{
    serviceBuilder.AddService<GameSessionService>();
    serviceBuilder.AddServiceEndpoint<GameSessionService, IGameSessionService>(
        new NetTcpBinding(SecurityMode.None),
        ServiceEndpointPaths.GameSession);
}
