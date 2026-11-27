namespace Game.Services
{
    public sealed class MailSender
    {
        public MailSender(string address, string displayName)
        {
            Address = address;
            DisplayName = displayName;
        }

        public string Address { get; }

        public string DisplayName { get; }
    }
}
