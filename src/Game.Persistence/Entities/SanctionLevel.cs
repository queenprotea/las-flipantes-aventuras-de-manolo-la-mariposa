using System;

namespace Game.Persistence
{
    public sealed class SanctionLevel
    {
        public short Level { get; set; }

        public TimeSpan BanDuration { get; set; }
    }
}
