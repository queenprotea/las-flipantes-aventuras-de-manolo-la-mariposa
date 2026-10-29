using System;

using Game.Contracts;

namespace Torres.Client.Session
{
    internal sealed class PlayerSession
    {
        internal PlayerIdentity? Player { get; private set; }

        internal Room? CurrentRoom { get; private set; }

        internal bool IsLoggedIn => Player is not null;

        internal void Start(PlayerIdentity player)
        {
            ArgumentNullException.ThrowIfNull(player);

            Player = player;
        }

        internal void EnterRoom(Room room)
        {
            ArgumentNullException.ThrowIfNull(room);

            CurrentRoom = room;
        }

        internal void LeaveRoom()
        {
            CurrentRoom = null;
        }

        internal void End()
        {
            Player = null;
            CurrentRoom = null;
        }
    }
}
