using System;

namespace Torres.ArchitectureSpike.Match
{
    public interface IMatchClock
    {
        IDisposable Schedule(TimeSpan dueTime, Action elapsedAction);
    }
}
