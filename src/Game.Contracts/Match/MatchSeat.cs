using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes a seat of the match: its number, who sits there, the guest mark and the color kept from the room.
    /// </summary>
    [DataContract]
    public sealed class MatchSeat
    {
        [DataMember]
        public int SeatNumber { get; set; }

        [DataMember]
        public string DisplayName { get; set; } = string.Empty;

        [DataMember]
        public bool IsGuest { get; set; }

        [DataMember]
        public SeatColor Color { get; set; }
    }
}
