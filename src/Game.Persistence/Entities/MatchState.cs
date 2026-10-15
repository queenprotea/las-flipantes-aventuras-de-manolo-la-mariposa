using System;

namespace Game.Persistence
{
    public sealed class MatchState
    {
        public int MatchId { get; set; }

        public short RoundNumber { get; set; }

        public short TurnNumber { get; set; }

        public short ActiveSeatNumber { get; set; }

        public string StateDocument { get; set; } = string.Empty;

        public DateTime SavedAt { get; set; }
    }
}
