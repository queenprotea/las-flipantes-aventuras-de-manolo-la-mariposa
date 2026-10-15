using System;

namespace Game.Persistence
{
    public sealed class Room
    {
        public int RoomId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Visibility { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? ClosedAt { get; set; }
    }
}
