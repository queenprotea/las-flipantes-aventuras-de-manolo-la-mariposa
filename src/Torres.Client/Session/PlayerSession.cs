using System;
using System.Linq;

using Game.Contracts;

using Torres.Client.Connection;

namespace Torres.Client.Session
{
    internal sealed class PlayerSession
    {
        internal PlayerIdentity? Player { get; private set; }

        internal string? GuestAlias { get; private set; }

        internal string? AvatarReference { get; private set; }

        internal Room? CurrentRoom { get; private set; }

        internal MatchView? CurrentMatchView { get; private set; }

        internal string? MatchRoomCode { get; private set; }

        internal DateTime? StepEndsAt { get; private set; }

        internal DateTime? ReconnectionEndsAt { get; private set; }

        internal TimeSpan StepTimeLeft => FindTimeLeft(StepEndsAt);

        internal TimeSpan ReconnectionTimeLeft => FindTimeLeft(ReconnectionEndsAt);

        internal bool HasForfeitedMatch { get; private set; }

        internal RoomChatLog RoomChat { get; } = new RoomChatLog();

        internal MatchHistorySelection MatchHistory { get; } = new MatchHistorySelection();

        internal RoomInvitationInbox Invitations { get; } = new RoomInvitationInbox();

        internal bool HasAccount => Player is not null;

        internal bool IsGuest => GuestAlias is not null;

        internal bool IsLoggedIn => HasAccount || IsGuest;

        internal string DisplayName => Player?.Username ?? GuestAlias ?? string.Empty;

        internal bool IsPlayingMatch => (CurrentMatchView is MatchView matchView) && (matchView.Phase != MatchPhase.Finished) && !IsViewerRetired(matchView);

        internal void Start(PlayerIdentity player)
        {
            Player = player;
            GuestAlias = null;
            AvatarReference = null;
        }

        internal void StartAsGuest(string guestAlias)
        {
            Player = null;
            GuestAlias = guestAlias;
            AvatarReference = null;
        }

        internal void ApplyProfile(string username, string? avatarReference)
        {
            if (Player is not null)
            {
                Player.Username = username;
            }

            AvatarReference = avatarReference;
        }

        internal void EnterRoom(Room room)
        {
            CurrentRoom = room;
            RoomChat.Follow(room.RoomId);
        }

        internal void EnterMatchWithoutRoom(string roomCode)
        {
            CurrentRoom = null;
            MatchRoomCode = roomCode;
            RoomChat.Clear();
        }

        internal void ApplyMatchChange(MatchChange matchChange)
        {
            if (matchChange.Kind == MatchChangeKind.Started)
            {
                HasForfeitedMatch = false;
            }

            CurrentMatchView = matchChange.MatchView;
            MatchRoomCode = matchChange.MatchView.RoomCode;
            StepEndsAt = null;
            if (matchChange.MatchView.StepTimeLeft is TimeSpan stepTimeLeft)
            {
                StepEndsAt = matchChange.ReceivedAt + stepTimeLeft;
            }
        }

        internal void LoseConnection(DateTime lostAt)
        {
            ReconnectionEndsAt = null;
            if (CurrentMatchView is MatchView matchView)
            {
                bool isViewerStep = (matchView.ActiveSeatNumber == matchView.ViewerSeatNumber) || (matchView.Phase == MatchPhase.AwaitingPlayers);
                ReconnectionEndsAt = isViewerStep ? StepEndsAt : lostAt + matchView.ReconnectionTime;
            }
        }

        internal void RecoverConnection()
        {
            ReconnectionEndsAt = null;
        }

        internal void ForfeitMatch()
        {
            HasForfeitedMatch = true;
            LeaveMatch();
        }

        internal void LeaveMatch()
        {
            CurrentMatchView = null;
            MatchRoomCode = null;
            StepEndsAt = null;
            ReconnectionEndsAt = null;
        }

        internal void LeaveRoom()
        {
            CurrentRoom = null;
            CurrentMatchView = null;
            MatchRoomCode = null;
            StepEndsAt = null;
            ReconnectionEndsAt = null;
            HasForfeitedMatch = false;
            RoomChat.Clear();
        }

        internal static bool HasViewerReturned(MatchView matchView)
        {
            bool hasReturned = (matchView.Phase == MatchPhase.AwaitingPlayers) && matchView.Seats.Any(seat => (seat.SeatNumber == matchView.ViewerSeatNumber) && !seat.IsDisconnected && !seat.IsRetired);

            return hasReturned;
        }

        internal static bool IsViewerRetired(MatchView matchView)
        {
            bool isRetired = matchView.Seats.Any(seat => (seat.SeatNumber == matchView.ViewerSeatNumber) && seat.IsRetired);

            return isRetired;
        }

        internal void End()
        {
            Player = null;
            GuestAlias = null;
            AvatarReference = null;
            CurrentRoom = null;
            CurrentMatchView = null;
            MatchRoomCode = null;
            StepEndsAt = null;
            ReconnectionEndsAt = null;
            HasForfeitedMatch = false;
            RoomChat.Clear();
            MatchHistory.Clear();
            Invitations.Clear();
        }

        private static TimeSpan FindTimeLeft(DateTime? endsAt)
        {
            TimeSpan timeLeft = TimeSpan.Zero;
            if (endsAt is DateTime foundEndsAt)
            {
                timeLeft = foundEndsAt - DateTime.UtcNow;
            }

            return timeLeft;
        }
    }
}
