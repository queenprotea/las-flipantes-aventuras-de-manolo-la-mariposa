using System;

namespace Game.Persistence
{
    public sealed class PasswordRecovery
    {
        public int PlayerId { get; set; }

        public string Code { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
