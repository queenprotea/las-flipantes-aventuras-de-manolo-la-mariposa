using System;

namespace Game.Persistence
{
    public sealed class MatchRecord
    {
        public int MatchId { get; set; }

        public int RoomId { get; set; }

        public string Status { get; set; } = string.Empty;

        public short PlayerCount { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public string? EndReason { get; set; }
    }
}
