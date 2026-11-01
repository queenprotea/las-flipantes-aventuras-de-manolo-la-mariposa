using System;

namespace Game.Services
{
    internal sealed record SessionOwner
    {
        private SessionOwner(int ownerId, string displayName, bool isGuest)
        {
            OwnerId = ownerId;
            DisplayName = displayName;
            IsGuest = isGuest;
        }

        internal int OwnerId { get; }

        internal string DisplayName { get; }

        internal bool IsGuest { get; }

        internal static SessionOwner ForPlayer(int playerId, string username)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(playerId);
            ArgumentException.ThrowIfNullOrWhiteSpace(username);

            var playerOwner = new SessionOwner(playerId, username, false);

            return playerOwner;
        }

        internal static SessionOwner ForGuest(int guestId, string guestAlias)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(guestId, 0);
            ArgumentException.ThrowIfNullOrWhiteSpace(guestAlias);

            var guestOwner = new SessionOwner(guestId, guestAlias, true);

            return guestOwner;
        }
    }
}
