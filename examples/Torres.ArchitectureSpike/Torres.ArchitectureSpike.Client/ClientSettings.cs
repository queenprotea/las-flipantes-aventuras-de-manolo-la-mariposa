using System;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.Text.Json;

namespace Torres.ArchitectureSpike.Client
{
    internal sealed class ClientSettings
    {
        private const string SettingsFileName = "appsettings.json";
        private const string MissingSettingsMessage = "The client settings file {0} was not found.";
        private const string InvalidSettingsMessage = "The appsettings.json file needs a valid ServerAddress and a positive NotificationWaitSeconds.";

        public string ServerAddress { get; set; } = string.Empty;

        public int NotificationWaitSeconds { get; set; }

        internal TimeSpan NotificationWait => TimeSpan.FromSeconds(NotificationWaitSeconds);

        internal static ClientSettings Load()
        {
            string settingsPath = Path.Combine(AppContext.BaseDirectory, SettingsFileName);
            if (!File.Exists(settingsPath))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, MissingSettingsMessage, settingsPath));
            }

            string settingsJson = File.ReadAllText(settingsPath);
            ClientSettings? settings = JsonSerializer.Deserialize<ClientSettings>(settingsJson);
            if ((settings is null) || !Uri.IsWellFormedUriString(settings.ServerAddress, UriKind.Absolute) || (settings.NotificationWaitSeconds <= 0))
            {
                throw new InvalidOperationException(InvalidSettingsMessage);
            }

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
