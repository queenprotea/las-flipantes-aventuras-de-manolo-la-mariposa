using System;

namespace Game.Services
{
    public sealed class AvatarUploadTiming
    {
        public AvatarUploadTiming(TimeSpan idleLimit)
        {
            IdleLimit = idleLimit;
        }

        public TimeSpan IdleLimit { get; }
    }
}
