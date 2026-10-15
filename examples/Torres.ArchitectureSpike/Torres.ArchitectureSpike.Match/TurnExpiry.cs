namespace Torres.ArchitectureSpike.Match
{
    internal sealed class TurnExpiry
    {
        private readonly LiveMatch _liveMatch;
        private readonly int _turnNumber;

        internal TurnExpiry(LiveMatch liveMatch, int turnNumber)
        {
            _liveMatch = liveMatch;
            _turnNumber = turnNumber;
        }

        internal void Expire()
        {
            _liveMatch.ExpireTurn(_turnNumber);
        }
    }
}
