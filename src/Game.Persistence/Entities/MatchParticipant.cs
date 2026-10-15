namespace Game.Persistence
{
    public sealed class MatchParticipant
    {
        public int MatchId { get; set; }

        public short SeatNumber { get; set; }

        public int PlayerId { get; set; }

        public int? FinalScore { get; set; }

        public short? FinalPosition { get; set; }

        public bool WasWithdrawn { get; set; }
    }
}
