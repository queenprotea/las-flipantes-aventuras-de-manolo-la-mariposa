using System;

namespace Game.Services
{
    public sealed class SessionLostEventArgs : EventArgs
    {
        public SessionLostEventArgs(int playerId)
        {
            PlayerId = playerId;
        }

        public int PlayerId { get; }
    }
}
