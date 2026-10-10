namespace Game.Persistence
{
    public sealed class PlayerRanking
    {
        public int PlayerId { get; set; }

        public string Username { get; set; } = string.Empty;

        public long MatchesPlayed { get; set; }

        public long Wins { get; set; }

        public long TotalScore { get; set; }
    }
}
