using System;

namespace Game.Services
{
    public sealed class MailServer
    {
        public MailServer(string host, int port, bool usesStartTls, MailCredentials? credentials)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(host);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(port);

            Host = host;
            Port = port;
            UsesStartTls = usesStartTls;
            Credentials = credentials;
        }

        public string Host { get; }

        public int Port { get; }

        public bool UsesStartTls { get; }

        public MailCredentials? Credentials { get; }
    }
}
