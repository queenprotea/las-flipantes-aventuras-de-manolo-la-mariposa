using System;

using Game.Contracts;

namespace Torres.Client.Session
{
    internal sealed class PlayerSession
    {
        internal PlayerIdentity? Player { get; private set; }

        internal string? GuestAlias { get; private set; }

        internal Room? CurrentRoom { get; private set; }

        internal bool HasAccount => Player is not null;

        internal bool IsGuest => GuestAlias is not null;

        internal bool IsLoggedIn => HasAccount || IsGuest;

        internal string DisplayName => Player?.Username ?? GuestAlias ?? string.Empty;

        internal void Start(PlayerIdentity player)
        {
            ArgumentNullException.ThrowIfNull(player);

            Player = player;
            GuestAlias = null;
        }

        internal void StartAsGuest(string guestAlias)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(guestAlias);

            Player = null;
            GuestAlias = guestAlias;
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
            GuestAlias = null;
            CurrentRoom = null;
        }
    }
}
