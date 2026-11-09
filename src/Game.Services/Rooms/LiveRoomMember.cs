using System;

using Game.Contracts;
using Game.Match;

namespace Game.Services
{
    internal sealed class LiveRoomMember
    {
        internal const int InitialProfileVersion = 0;

        private MemberProfile? _heldProfile;

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

        internal string DisplayName { get; private set; }

        internal string? AvatarReference { get; private set; }

        internal int ProfileVersion { get; private set; } = InitialProfileVersion;

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

        internal void ApplyProfile(string username, string? avatarReference)
        {
            DisplayName = username;
            AvatarReference = avatarReference;
            ProfileVersion++;
        }

        internal void HoldProfile(string username, string? avatarReference)
        {
            _heldProfile = new MemberProfile(username, avatarReference);
            ProfileVersion++;
        }

        internal void ApplyHeldProfile()
        {
            if (_heldProfile is null)
            {
                return;
            }

            DisplayName = _heldProfile.Username;
            AvatarReference = _heldProfile.AvatarReference;
            _heldProfile = null;
        }

        internal void ApplyLoadedAvatar(string avatarReference)
        {
            AvatarReference = avatarReference;
        }

        internal bool HasExpiredBefore(DateTime limitUtc)
        {
            bool hasExpired = !IsActive && (InactiveSinceUtc < limitUtc);

            return hasExpired;
        }

        internal MatchEntrant ToMatchEntrant()
        {
            var entrant = new MatchEntrant(OwnerId, MemberId, DisplayName, IsGuest, IsActive, Color);

            return entrant;
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
                AvatarReference = AvatarReference,
            };

            return roomMember;
        }
    }
}
