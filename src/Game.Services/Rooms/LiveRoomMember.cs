using System;

using Game.Contracts;

namespace Game.Services
{
    internal sealed class LiveRoomMember
    {
        internal LiveRoomMember(int memberId, SessionOwner owner, SeatColor color)
        {
            MemberId = memberId;
            OwnerId = owner.OwnerId;
            DisplayName = owner.DisplayName;
            IsGuest = owner.IsGuest;
            Color = color;
        }

        internal int MemberId { get; }

        internal int OwnerId { get; }

        internal string DisplayName { get; }

        internal bool IsGuest { get; }

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
                IsGuest = IsGuest,
                Color = Color,
                IsActive = IsActive,
            };

            return roomMember;
        }
    }
}
