using System;

namespace Game.Services
{
    public sealed class AvatarUploadTiming
    {
        public AvatarUploadTiming(TimeSpan idleLimit)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(idleLimit, TimeSpan.Zero);

            IdleLimit = idleLimit;
        }

        public TimeSpan IdleLimit { get; }
    }
}
