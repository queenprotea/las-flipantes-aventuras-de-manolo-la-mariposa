using System;

using Game.Contracts;

namespace Game.Match
{
    public sealed record MatchEntrant
    {
        public MatchEntrant(int entrantId, int memberId, string displayName, bool isGuest, bool isActive, SeatColor color)
        {
            ArgumentNullException.ThrowIfNull(displayName);

            EntrantId = entrantId;
            MemberId = memberId;
            DisplayName = displayName;
            IsGuest = isGuest;
            IsActive = isActive;
            Color = color;
        }

        public int EntrantId { get; }

        public int MemberId { get; }

        public string DisplayName { get; }

        public bool IsGuest { get; }

        public bool IsActive { get; }

        public SeatColor Color { get; }
    }
}
