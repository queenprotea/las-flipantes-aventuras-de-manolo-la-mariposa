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

        public double FriendshipListRefreshSeconds { get; set; }

        public double InvitationStateRefreshSeconds { get; set; }

        public double SessionCloseTimeoutSeconds { get; set; }

        public double ReconnectionRetrySeconds { get; set; }

        public double ReceiveTimeoutMinutes { get; set; }

        public double SendTimeoutSeconds { get; set; }

        public double StepSilenceMarginSeconds { get; set; }

        internal TimeSpan FriendshipListRefreshPeriod => TimeSpan.FromSeconds(FriendshipListRefreshSeconds);

        internal TimeSpan InvitationStateRefreshPeriod => TimeSpan.FromSeconds(InvitationStateRefreshSeconds);

        internal TimeSpan SessionCloseTimeout => TimeSpan.FromSeconds(SessionCloseTimeoutSeconds);

        internal TimeSpan ReconnectionRetryInterval => TimeSpan.FromSeconds(ReconnectionRetrySeconds);

        internal TimeSpan ReceiveTimeout => TimeSpan.FromMinutes(ReceiveTimeoutMinutes);

        internal TimeSpan SendTimeout => TimeSpan.FromSeconds(SendTimeoutSeconds);

        internal TimeSpan StepSilenceMargin => TimeSpan.FromSeconds(StepSilenceMarginSeconds);

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

            bool isValid = Uri.IsWellFormedUriString(settings.ServerAddress, UriKind.Absolute) && settings.ServicePaths.IsComplete() && settings.HasPositiveTimes();
            ClientSettings? validSettings = isValid ? settings : null;

            return validSettings;
        }

        private bool HasPositiveTimes()
        {
            bool hasPositiveTimes = (FriendshipListRefreshSeconds > 0) && (InvitationStateRefreshSeconds > 0) && (SessionCloseTimeoutSeconds > 0);
            bool hasPositiveConnectionTimes = HasPositiveConnectionTimes();
            bool hasAllPositiveTimes = hasPositiveTimes && hasPositiveConnectionTimes;

            return hasAllPositiveTimes;
        }

        private bool HasPositiveConnectionTimes()
        {
            bool hasPositiveTimeouts = (ReceiveTimeoutMinutes > 0) && (SendTimeoutSeconds > 0);
            bool hasPositiveConnectionTimes = hasPositiveTimeouts && (ReconnectionRetrySeconds > 0) && (StepSilenceMarginSeconds > 0);

            return hasPositiveConnectionTimes;
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
