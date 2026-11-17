using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes a seat of the match: its number, who sits there, the guest mark, the color kept from the room, the caterpillars still in hand, the points of each closed round, the cards it holds, the points earned with kept cards, the final score, whether the player was retired and whether the player lost the connection and is still inside the reconnection window.
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

        [DataMember]
        public int CaterpillarsLeft { get; set; }

        [DataMember]
        public List<MatchRoundScore> RoundScores { get; set; } = new List<MatchRoundScore>();

        [DataMember]
        public int TotalScore { get; set; }

        [DataMember]
        public bool IsRetired { get; set; }

        [DataMember]
        public bool IsDisconnected { get; set; }

        [DataMember]
        public int HandCount { get; set; }

        [DataMember]
        public int CardPoints { get; set; }

        [DataMember]
        public int FinalScore { get; set; }
    }
}
