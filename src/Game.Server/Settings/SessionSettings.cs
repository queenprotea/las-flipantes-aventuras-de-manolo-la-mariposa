using System;
using System.Globalization;

using Game.Services;

using Microsoft.Extensions.Configuration;

namespace Game.Server
{
    public sealed class SessionSettings
    {
        private const string SectionName = "Session";
        private const string SigningKeyVariable = "TORRES_SESSION_KEY";
        private const string InvalidTimesMessage = "The Session section of appsettings.json needs a positive MaintenancePeriodSeconds, ReceiveTimeoutMinutes and SendTimeoutSeconds.";
        private const string MissingKeyMessage = "The environment variable TORRES_SESSION_KEY is not set, is not valid base64 or is shorter than {0} bytes.";

        public int MaintenancePeriodSeconds { get; set; }

        public int ReceiveTimeoutMinutes { get; set; }

        public int SendTimeoutSeconds { get; set; }

        public TimeSpan MaintenancePeriod => TimeSpan.FromSeconds(MaintenancePeriodSeconds);

        public TimeSpan ReceiveTimeout => TimeSpan.FromMinutes(ReceiveTimeoutMinutes);

        public TimeSpan SendTimeout => TimeSpan.FromSeconds(SendTimeoutSeconds);

        public static SessionSettings Read(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            SessionSettings? settings = configuration.GetSection(SectionName).Get<SessionSettings>();
            if ((settings is null) || !settings.HasPositiveTimes())
            {
                throw new InvalidOperationException(InvalidTimesMessage);
            }

            return settings;
        }

        public static byte[] ReadSigningKey()
        {
            string encodedKey = Environment.GetEnvironmentVariable(SigningKeyVariable) ?? string.Empty;
            byte[] keyBuffer = new byte[encodedKey.Length];
            bool isValidKey = Convert.TryFromBase64String(encodedKey, keyBuffer, out int keyLength) && (keyLength >= SessionTokenIssuer.ShortestSigningKeyBytes);
            if (!isValidKey)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, MissingKeyMessage, SessionTokenIssuer.ShortestSigningKeyBytes));
            }

            byte[] signingKey = keyBuffer[..keyLength];

            return signingKey;
        }

        private bool HasPositiveTimes()
        {
            bool hasPositiveTimes = (MaintenancePeriodSeconds > 0) && (ReceiveTimeoutMinutes > 0) && (SendTimeoutSeconds > 0);

            return hasPositiveTimes;
        }
    }
}
