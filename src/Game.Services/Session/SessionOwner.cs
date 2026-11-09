using System;

namespace Game.Services
{
    internal sealed record SessionOwner
    {
        private const string GuestRenameMessage = "A guest owner cannot take a username.";

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

        internal SessionOwner WithUsername(string username)
        {
            if (IsGuest)
            {
                throw new InvalidOperationException(GuestRenameMessage);
            }

            SessionOwner renamedOwner = ForPlayer(OwnerId, username);

            return renamedOwner;
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
