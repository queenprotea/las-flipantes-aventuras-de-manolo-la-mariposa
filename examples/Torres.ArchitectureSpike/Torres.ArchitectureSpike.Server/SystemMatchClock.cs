using System;
using System.Threading;

using Torres.ArchitectureSpike.Match;

namespace Torres.ArchitectureSpike.Server
{
    public sealed class SystemMatchClock : IMatchClock
    {
        public IDisposable Schedule(TimeSpan dueTime, Action elapsedAction)
        {
            ArgumentNullException.ThrowIfNull(elapsedAction);

            var timer = new Timer(TimerOnElapsed, elapsedAction, dueTime, Timeout.InfiniteTimeSpan);

            return timer;
        }

        private static void TimerOnElapsed(object? state)
        {
            if (state is Action elapsedAction)
            {
                elapsedAction();
            }
        }
    }
}
