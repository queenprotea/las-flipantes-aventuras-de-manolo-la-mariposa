using System;

using Microsoft.Extensions.Configuration;

namespace Game.Services
{
    public sealed class ServerSettings
    {
        private const string SectionName = "Server";
        private const string InvalidPortMessage = "The Server section of appsettings.json has no valid NetTcpPort.";

        public int NetTcpPort { get; set; }

        public static ServerSettings Read(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            ServerSettings? settings = configuration.GetSection(SectionName).Get<ServerSettings>();
            if ((settings is null) || (settings.NetTcpPort <= 0))
            {
                throw new InvalidOperationException(InvalidPortMessage);
            }

            return settings;
        }
    }
}
