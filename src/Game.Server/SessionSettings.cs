using System;

using Microsoft.Extensions.Configuration;

namespace Game.Server
{
    public sealed class SessionSettings
    {
        private const string SectionName = "Session";
        private const string SigningKeyVariable = "TORRES_SESSION_KEY";
        private const string InvalidHeartbeatMessage = "The Session section of appsettings.json needs a positive HeartbeatPeriodSeconds and HeartbeatMissesToDrop.";
        private const string MissingKeyMessage = "The environment variable TORRES_SESSION_KEY is not set or is not valid base64.";

        public int HeartbeatPeriodSeconds { get; set; }

        public int HeartbeatMissesToDrop { get; set; }

        public static SessionSettings Read(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            SessionSettings? settings = configuration.GetSection(SectionName).Get<SessionSettings>();
            if ((settings is null) || (settings.HeartbeatPeriodSeconds <= 0) || (settings.HeartbeatMissesToDrop <= 0))
            {
                throw new InvalidOperationException(InvalidHeartbeatMessage);
            }

            return settings;
        }

        public static byte[] ReadSigningKey()
        {
            string encodedKey = Environment.GetEnvironmentVariable(SigningKeyVariable) ?? string.Empty;
            byte[] keyBuffer = new byte[encodedKey.Length];
            bool isValidKey = Convert.TryFromBase64String(encodedKey, keyBuffer, out int keyLength) && (keyLength > 0);
            if (!isValidKey)
            {
                throw new InvalidOperationException(MissingKeyMessage);
            }

            byte[] signingKey = keyBuffer[..keyLength];

            return signingKey;
        }
    }
}
