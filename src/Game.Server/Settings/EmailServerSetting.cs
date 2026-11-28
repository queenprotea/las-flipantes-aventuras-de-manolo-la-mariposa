namespace Game.Server
{
    public sealed class EmailServerSetting
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public bool UsesStartTls { get; set; }

        public bool ChecksCertificateRevocation { get; set; }
    }
}
