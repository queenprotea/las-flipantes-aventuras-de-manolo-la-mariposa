using System;

namespace Torres.EfCoreSpike.Persistence
{
    public sealed class Player
    {
        public int PlayerId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? AvatarReference { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
