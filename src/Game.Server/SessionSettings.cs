using System;

using Microsoft.Extensions.Configuration;

namespace Game.Server
{
    public sealed class SessionSettings
    {
        private const string SectionName = "Session";
        private const string SigningKeyVariable = "TORRES_SESSION_KEY";
        private const string InvalidConnectionCheckMessage = "The Session section of appsettings.json needs a positive ConnectionCheckPeriodSeconds and ConnectionCheckMissesToDrop.";
        private const string MissingKeyMessage = "The environment variable TORRES_SESSION_KEY is not set or is not valid base64.";

        public int ConnectionCheckPeriodSeconds { get; set; }

        public int ConnectionCheckMissesToDrop { get; set; }

        public static SessionSettings Read(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            SessionSettings? settings = configuration.GetSection(SectionName).Get<SessionSettings>();
            if ((settings is null) || (settings.ConnectionCheckPeriodSeconds <= 0) || (settings.ConnectionCheckMissesToDrop <= 0))
            {
                throw new InvalidOperationException(InvalidConnectionCheckMessage);
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
