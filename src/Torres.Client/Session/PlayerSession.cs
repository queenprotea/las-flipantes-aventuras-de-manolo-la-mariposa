using System;

using Game.Contracts;

namespace Torres.Client.Session
{
    internal sealed class PlayerSession
    {
        internal PlayerIdentity? Player { get; private set; }

        internal string? GuestAlias { get; private set; }

        internal Room? CurrentRoom { get; private set; }

        internal MatchView? CurrentMatchView { get; private set; }

        internal DateTime? PlacementEndsAtUtc { get; private set; }

        internal RoomChatLog RoomChat { get; } = new RoomChatLog();

        internal MatchHistorySelection MatchHistory { get; } = new MatchHistorySelection();

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
            RoomChat.Follow(room.RoomId);
        }

        internal void ApplyMatchChange(MatchChange matchChange)
        {
            ArgumentNullException.ThrowIfNull(matchChange);

            CurrentMatchView = matchChange.MatchView;
            PlacementEndsAtUtc = null;
            if (matchChange.MatchView.PlacementTimeLeft is TimeSpan placementTimeLeft)
            {
                PlacementEndsAtUtc = matchChange.ReceivedAtUtc + placementTimeLeft;
            }
        }

        internal void LeaveMatch()
        {
            CurrentMatchView = null;
            PlacementEndsAtUtc = null;
        }

        internal void LeaveRoom()
        {
            CurrentRoom = null;
            CurrentMatchView = null;
            PlacementEndsAtUtc = null;
            RoomChat.Clear();
        }

        internal void End()
        {
            Player = null;
            GuestAlias = null;
            CurrentRoom = null;
            CurrentMatchView = null;
            PlacementEndsAtUtc = null;
            RoomChat.Clear();
            MatchHistory.Clear();
        }
    }
}
