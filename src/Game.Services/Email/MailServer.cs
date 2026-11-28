namespace Game.Services
{
    public sealed class MailServer
    {
        public MailServer(string host, int port, bool usesStartTls, bool checksCertificateRevocation, MailCredentials? credentials)
        {
            Host = host;
            Port = port;
            UsesStartTls = usesStartTls;
            ChecksCertificateRevocation = checksCertificateRevocation;
            Credentials = credentials;
        }

        public string Host { get; }

        public int Port { get; }

        public bool UsesStartTls { get; }

        public bool ChecksCertificateRevocation { get; }

        public MailCredentials? Credentials { get; }
    }
}
