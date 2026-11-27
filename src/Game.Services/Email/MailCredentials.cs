namespace Game.Services
{
    public sealed class MailCredentials
    {
        public MailCredentials(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public string UserName { get; }

        public string Password { get; }
    }
}
