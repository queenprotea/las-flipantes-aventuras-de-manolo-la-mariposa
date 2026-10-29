using System;

using Game.Contracts;

namespace Game.Services
{
    internal sealed class LiveRoomMember
    {
        internal LiveRoomMember(int memberId, SessionOwner owner, SeatColor color)
        {
            MemberId = memberId;
            PlayerId = owner.PlayerId;
            DisplayName = owner.DisplayName;
            Color = color;
        }

        internal int MemberId { get; }

        internal int PlayerId { get; }

        internal string DisplayName { get; }

        internal SeatColor Color { get; }

        internal bool IsActive { get; private set; } = true;

        internal DateTime? InactiveSinceUtc { get; private set; }

        internal void MarkInactive(DateTime nowUtc)
        {
            IsActive = false;
            InactiveSinceUtc = nowUtc;
        }

        internal void MarkActive()
        {
            IsActive = true;
            InactiveSinceUtc = null;
        }

        internal bool HasExpiredBefore(DateTime limitUtc)
        {
            bool hasExpired = !IsActive && (InactiveSinceUtc < limitUtc);

            return hasExpired;
        }

        internal RoomMember ToContract()
        {
            var roomMember = new RoomMember
            {
                MemberId = MemberId,
                DisplayName = DisplayName,
                IsGuest = false,
                Color = Color,
                IsActive = IsActive,
            };

            return roomMember;
        }
    }
}
