using System;

namespace Game.Services
{
    public sealed class MailCredentials
    {
        public MailCredentials(string userName, string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userName);
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            UserName = userName;
            Password = password;
        }

        public string UserName { get; }

        public string Password { get; }
    }
}
