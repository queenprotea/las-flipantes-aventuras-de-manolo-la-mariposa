using System;

namespace Game.Services
{
    public sealed class SessionTimings
    {
        public SessionTimings(TimeSpan connectionCheckPeriod, int connectionCheckMissesToDrop)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(connectionCheckPeriod, TimeSpan.Zero);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(connectionCheckMissesToDrop);

            ConnectionCheckPeriod = connectionCheckPeriod;
            ConnectionCheckMissesToDrop = connectionCheckMissesToDrop;
        }

        public TimeSpan ConnectionCheckPeriod { get; }

        public int ConnectionCheckMissesToDrop { get; }

        public TimeSpan SilenceLimit => ConnectionCheckPeriod * ConnectionCheckMissesToDrop;
    }
}
