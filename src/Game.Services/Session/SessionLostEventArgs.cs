using System;

namespace Game.Services
{
    public sealed class SessionLostEventArgs : EventArgs
    {
        public SessionLostEventArgs(int ownerId)
        {
            OwnerId = ownerId;
        }

        public int OwnerId { get; }
    }
}
