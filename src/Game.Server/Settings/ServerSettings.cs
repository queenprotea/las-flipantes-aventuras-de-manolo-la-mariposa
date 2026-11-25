using System;

using Microsoft.Extensions.Configuration;

namespace Game.Server
{
    public sealed class ServerSettings
    {
        private const string SectionName = "Server";
        private const string InvalidPortMessage = "The Server section of appsettings.json has no valid NetTcpPort.";
        private const string InvalidMetadataPortMessage = "The Server section of appsettings.json publishes the service metadata but has no valid MetadataHttpPort.";

        public int NetTcpPort { get; set; }

        public bool PublishesServiceMetadata { get; set; }

        public int MetadataHttpPort { get; set; }

        public static ServerSettings Read(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            ServerSettings? settings = configuration.GetSection(SectionName).Get<ServerSettings>();
            if ((settings is null) || (settings.NetTcpPort <= 0))
            {
                throw new InvalidOperationException(InvalidPortMessage);
            }

            if (settings.PublishesServiceMetadata && (settings.MetadataHttpPort <= 0))
            {
                throw new InvalidOperationException(InvalidMetadataPortMessage);
            }

            return settings;
        }
    }
}
