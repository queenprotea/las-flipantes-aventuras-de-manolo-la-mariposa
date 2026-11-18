using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes one member's place in the room ranking: the points and wins gathered in the matches finished in that room.
    /// </summary>
    [DataContract]
    public sealed class RoomRankingEntry
    {
        [DataMember]
        public int MemberId { get; set; }

        [DataMember]
        public string DisplayName { get; set; } = string.Empty;

        [DataMember]
        public bool IsGuest { get; set; }

        [DataMember]
        public SeatColor Color { get; set; }

        [DataMember]
        public int Points { get; set; }

        [DataMember]
        public int Wins { get; set; }
    }
}
