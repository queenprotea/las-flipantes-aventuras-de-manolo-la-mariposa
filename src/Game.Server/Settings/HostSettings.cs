using System.Collections.Generic;

using Game.Contracts;
using Game.Persistence;
using Game.Services;

using Microsoft.Extensions.Configuration;

namespace Game.Server
{
    public sealed class HostSettings
    {
        private HostSettings(IConfiguration configuration)
        {
            Database = DatabaseSettings.ReadFromEnvironment();
            SessionSigningKey = SessionSettings.ReadSigningKey();
            Server = ServerSettings.Read(configuration);
            Session = SessionSettings.Read(configuration);
            History = HistorySettings.Read(configuration);
            Avatar = AvatarSettings.Read(configuration);
            Email = EmailSettings.Read(configuration);
            EmailSender = Email.CreateSender();
            StartingFlowerCells = MatchBoardSettings.ReadFromConfiguration(configuration).ToStartingCells();
        }

        public DatabaseSettings Database { get; }

        public byte[] SessionSigningKey { get; }

        public ServerSettings Server { get; }

        public SessionSettings Session { get; }

        public HistorySettings History { get; }

        public AvatarSettings Avatar { get; }

        public EmailSettings Email { get; }

        public EmailSender EmailSender { get; }

        public IReadOnlyList<MatchCell> StartingFlowerCells { get; }

        public static HostSettings Read(IConfiguration configuration)
        {
            var hostSettings = new HostSettings(configuration);

            return hostSettings;
        }
    }
}
