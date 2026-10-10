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
        private const string MissingSettingsMessage = "The client settings file {0} was not found.";
        private const string InvalidAddressMessage = "The appsettings.json file has no valid ServerAddress.";

        public string ServerAddress { get; set; } = string.Empty;

        internal static ClientSettings Load()
        {
            string settingsPath = Path.Combine(AppContext.BaseDirectory, SettingsFileName);
            if (!File.Exists(settingsPath))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, MissingSettingsMessage, settingsPath));
            }

            string settingsJson = File.ReadAllText(settingsPath);
            ClientSettings? settings = JsonSerializer.Deserialize<ClientSettings>(settingsJson);
            if ((settings is null) || !Uri.IsWellFormedUriString(settings.ServerAddress, UriKind.Absolute))
            {
                throw new InvalidOperationException(InvalidAddressMessage);
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
