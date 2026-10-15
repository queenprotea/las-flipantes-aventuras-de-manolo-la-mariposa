using Torres.ArchitectureSpike.Contracts;

namespace Torres.ArchitectureSpike.Match
{
    public interface IMatchNotificationReceiver
    {
        void Deliver(string playerName, MatchUpdate update);
    }
}
