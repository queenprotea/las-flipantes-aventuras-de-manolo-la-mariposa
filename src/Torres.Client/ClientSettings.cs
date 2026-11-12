using System;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.Text.Json;

namespace Torres.Client
{
    internal sealed class ClientSettings
    {
        private const string SettingsFileName = "appsettings.json";
        private const string LocalSettingsFileName = "appsettings.Local.json";
        private const string MissingSettingsMessage = "The client settings file {0} was not found.";
        private const string InvalidAddressMessage = "The appsettings.json file has no valid ServerAddress.";
        private const string InvalidServicePathsMessage = "The appsettings.json file needs the four ServicePaths: ServerStatus, Account, Ranking and GameSession.";

        public string ServerAddress { get; set; } = string.Empty;

        public ServicePathSettings ServicePaths { get; set; } = new ServicePathSettings();

        public bool LaunchesDoomAfterPlacement { get; set; }

        public string DoomProgramPath { get; set; } = string.Empty;

        public string DoomWadPath { get; set; } = string.Empty;

        internal static ClientSettings Load()
        {
            string settingsPath = Path.Combine(AppContext.BaseDirectory, SettingsFileName);
            if (!File.Exists(settingsPath))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, MissingSettingsMessage, settingsPath));
            }

            ClientSettings? settings = ReadSettingsFile(settingsPath);
            if (settings is null)
            {
                throw new InvalidOperationException(InvalidAddressMessage);
            }

            string localSettingsPath = Path.Combine(AppContext.BaseDirectory, LocalSettingsFileName);
            if (File.Exists(localSettingsPath))
            {
                ClientSettings? localSettings = ReadSettingsFile(localSettingsPath);
                if (!string.IsNullOrWhiteSpace(localSettings?.ServerAddress))
                {
                    settings.ServerAddress = localSettings.ServerAddress;
                }
            }

            if (!Uri.IsWellFormedUriString(settings.ServerAddress, UriKind.Absolute))
            {
                throw new InvalidOperationException(InvalidAddressMessage);
            }

            if (!settings.ServicePaths.IsComplete())
            {
                throw new InvalidOperationException(InvalidServicePathsMessage);
            }

            return settings;
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
