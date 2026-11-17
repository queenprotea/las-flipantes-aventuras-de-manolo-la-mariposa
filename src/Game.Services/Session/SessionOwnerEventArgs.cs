using System;

namespace Game.Services
{
    public sealed class SessionOwnerEventArgs : EventArgs
    {
        public SessionOwnerEventArgs(int ownerId)
        {
            OwnerId = ownerId;
        }

        public int OwnerId { get; }
    }
}
