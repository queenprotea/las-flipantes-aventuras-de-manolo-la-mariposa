using System;
using System.Globalization;
using System.Threading.Tasks;

using log4net;

using Microsoft.AspNetCore.Builder;

namespace Game.Server
{
    public sealed class ServerHost
    {
        private const string ConfigurationFailedMessage = "The configuration in appsettings.json or in the environment is not valid; the server does not start.";
        private const string EmailServerMessage = "Emails are delivered to {0}:{1}";
        private const string ServerListeningMessage = "Server listening on net.tcp port {0}";

        private readonly string[] _commandLineArguments;
        private readonly ILog _hostLog;

        public ServerHost(string[] commandLineArguments, ILog hostLog)
        {
            _commandLineArguments = commandLineArguments;
            _hostLog = hostLog;
        }

        public async Task RunAsync()
        {
            var hostOptions = new WebApplicationOptions
            {
                Args = _commandLineArguments,
                ContentRootPath = AppContext.BaseDirectory,
            };
            WebApplicationBuilder builder = WebApplication.CreateBuilder(hostOptions);
            HostSettings? hostSettings = ReadSettings(builder);
            if (hostSettings is not null)
            {
                await RunWithSettingsAsync(builder, hostSettings);
            }
        }

        private HostSettings? ReadSettings(WebApplicationBuilder builder)
        {
            HostSettings? hostSettings = null;
            try
            {
                hostSettings = HostSettings.Read(builder.Configuration);
            }
            catch (InvalidOperationException exception)
            {
                _hostLog.Fatal(ConfigurationFailedMessage, exception);
            }

            return hostSettings;
        }

        private async Task RunWithSettingsAsync(WebApplicationBuilder builder, HostSettings hostSettings)
        {
            var serverEndpoints = new ServerEndpoints(hostSettings.Server, hostSettings.Session, _hostLog);
            serverEndpoints.ConfigureHost(builder);
            ServiceRegistration.AddSettingValues(builder.Services, hostSettings);
            ServiceRegistration.AddRepositories(builder.Services, hostSettings.Database);
            ServiceRegistration.AddGameServices(builder.Services);
            WebApplication app = builder.Build();

            var serverStartup = new ServerStartup(app.Services, _hostLog);
            bool isPrepared = await serverStartup.PrepareAsync();
            if (isPrepared)
            {
                serverEndpoints.Publish(app);
                serverStartup.StartSupervisor();
                _hostLog.InfoFormat(CultureInfo.InvariantCulture, EmailServerMessage, hostSettings.Email.ChosenHost, hostSettings.Email.ChosenPort);
                _hostLog.InfoFormat(CultureInfo.InvariantCulture, ServerListeningMessage, hostSettings.Server.NetTcpPort);
                await app.RunAsync();
            }

            await serverStartup.StopAsync();
        }
    }
}
