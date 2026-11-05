using System;

using Game.Contracts;

namespace Game.Match
{
    public sealed record MatchEntrant
    {
        public MatchEntrant(int entrantId, int memberId, string displayName, bool isGuest, bool isActive, SeatColor seatColor)
        {
            ArgumentNullException.ThrowIfNull(displayName);

            EntrantId = entrantId;
            MemberId = memberId;
            DisplayName = displayName;
            IsGuest = isGuest;
            IsActive = isActive;
            SeatColor = seatColor;
        }

        public int EntrantId { get; }

        public int MemberId { get; }

        public string DisplayName { get; }

        public bool IsGuest { get; }

        public bool IsActive { get; }

        public SeatColor SeatColor { get; }
    }
}
