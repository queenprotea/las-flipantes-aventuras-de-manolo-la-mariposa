using System;
using System.IO;
using System.ServiceModel;
using System.Text.Json;
using System.Threading.Tasks;

namespace Torres.Client
{
    internal sealed class ClientSettings
    {
        private const string SettingsFileName = "appsettings.json";
        private const string LocalSettingsFileName = "appsettings.Local.json";

        public string ServerAddress { get; set; } = string.Empty;

        public ServicePathSettings ServicePaths { get; set; } = new ServicePathSettings();

        public bool LaunchesDoomAfterPlacement { get; set; }

        public string DoomProgramPath { get; set; } = string.Empty;

        public string DoomWadPath { get; set; } = string.Empty;

        internal static Task<ClientSettings?> ReadAsync()
        {
            Task<ClientSettings?> settingsReading = Task.Run(Read);

            return settingsReading;
        }

        private static ClientSettings? Read()
        {
            string settingsPath = Path.Combine(AppContext.BaseDirectory, SettingsFileName);
            ClientSettings? settings = File.Exists(settingsPath) ? ReadSettingsFile(settingsPath) : null;
            if (settings is null)
            {
                return null;
            }

            string localSettingsPath = Path.Combine(AppContext.BaseDirectory, LocalSettingsFileName);
            ClientSettings? localSettings = File.Exists(localSettingsPath) ? ReadSettingsFile(localSettingsPath) : null;
            if (!string.IsNullOrWhiteSpace(localSettings?.ServerAddress))
            {
                settings.ServerAddress = localSettings.ServerAddress;
            }

            bool isValid = Uri.IsWellFormedUriString(settings.ServerAddress, UriKind.Absolute) && settings.ServicePaths.IsComplete();
            ClientSettings? validSettings = isValid ? settings : null;

            return validSettings;
        }

        private static ClientSettings? ReadSettingsFile(string settingsPath)
        {
            string settingsJson = File.ReadAllText(settingsPath);
            ClientSettings? settings = JsonSerializer.Deserialize<ClientSettings>(settingsJson);

            return settings;
        }

        internal EndpointAddress CreateEndpointAddress(string servicePath)
        {
            var serviceUri = new Uri(new Uri(ServerAddress), servicePath);
            var endpointAddress = new EndpointAddress(serviceUri);

            return endpointAddress;
        }
    }
}
