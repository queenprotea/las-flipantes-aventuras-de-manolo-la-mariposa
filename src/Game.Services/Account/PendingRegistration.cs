using System;

namespace Game.Services
{
    public sealed class PendingRegistration
    {
        public PendingRegistration(string username, string email, string passwordHash, string code, DateTimeOffset expiresAt)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Code = code;
            ExpiresAt = expiresAt;
        }

        public string Username { get; }

        public string Email { get; }

        public string PasswordHash { get; }

        public string Code { get; }

        public DateTimeOffset ExpiresAt { get; }

        public int FailedAttempts { get; private set; }

        internal void RecordFailedAttempt()
        {
            FailedAttempts++;
        }
    }
}
