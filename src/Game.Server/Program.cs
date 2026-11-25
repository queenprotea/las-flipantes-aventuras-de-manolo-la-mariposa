using System;
using System.IO;
using System.Threading.Tasks;

using DotNetEnv;

using Game.Services;

using log4net;
using log4net.Config;

namespace Game.Server
{
    internal static class Program
    {
        private const string LogConfigurationFile = "log4net.config";
        private const string HostLoggerName = "Game.Server.Host";
        private const string UnhandledFailureMessage = "An unexpected failure stopped the server.";

        private static readonly ILog _hostLog = LogManager.GetLogger(typeof(ServerStatusService).Assembly, HostLoggerName);

        public static async Task Main(string[] args)
        {
            Env.TraversePath().Load();
            ConfigureLogging();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            try
            {
                var serverHost = new ServerHost(args, _hostLog);
                await serverHost.RunAsync();
            }
            catch (Exception exception)
            {
                _hostLog.Fatal(UnhandledFailureMessage, exception);
            }
        }

        private static void ConfigureLogging()
        {
            var configurationFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, LogConfigurationFile));
            XmlConfigurator.Configure(LogManager.GetRepository(typeof(ServerStatusService).Assembly), configurationFile);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception unhandledException)
            {
                _hostLog.Fatal(UnhandledFailureMessage, unhandledException);
            }
        }
    }
}
