using System;

namespace Game.Services
{
    public sealed class MaintenanceTiming
    {
        public MaintenanceTiming(TimeSpan period)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(period, TimeSpan.Zero);

            Period = period;
        }

        public TimeSpan Period { get; }
    }
}
