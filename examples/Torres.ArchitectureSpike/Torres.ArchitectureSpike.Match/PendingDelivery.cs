using Torres.ArchitectureSpike.Contracts;

namespace Torres.ArchitectureSpike.Match
{
    internal sealed class PendingDelivery
    {
        internal PendingDelivery(string playerName, MatchUpdate update)
        {
            PlayerName = playerName;
            Update = update;
        }

        internal string PlayerName { get; }

        internal MatchUpdate Update { get; }
    }
}
