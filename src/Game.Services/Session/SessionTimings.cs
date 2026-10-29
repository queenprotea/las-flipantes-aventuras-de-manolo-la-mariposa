using System;

namespace Game.Services
{
    public sealed class SessionTimings
    {
        public SessionTimings(TimeSpan heartbeatPeriod, int heartbeatMissesToDrop)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(heartbeatPeriod, TimeSpan.Zero);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(heartbeatMissesToDrop);

            HeartbeatPeriod = heartbeatPeriod;
            HeartbeatMissesToDrop = heartbeatMissesToDrop;
        }

        public TimeSpan HeartbeatPeriod { get; }

        public int HeartbeatMissesToDrop { get; }

        public TimeSpan SilenceLimit => HeartbeatPeriod * HeartbeatMissesToDrop;
    }
}
