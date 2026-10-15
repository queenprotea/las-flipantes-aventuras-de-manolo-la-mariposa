using System;

namespace Game.Persistence
{
    public sealed class RoomInvitation
    {
        public int RoomId { get; set; }

        public int InviterPlayerId { get; set; }

        public int InvitedPlayerId { get; set; }

        public string Channel { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
