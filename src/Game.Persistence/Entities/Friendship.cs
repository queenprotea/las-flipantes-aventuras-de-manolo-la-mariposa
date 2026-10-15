using System;

namespace Game.Persistence
{
    public sealed class Friendship
    {
        public int RequesterId { get; set; }

        public int AddresseeId { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime RequestedAt { get; set; }

        public DateTime? RespondedAt { get; set; }
    }
}
