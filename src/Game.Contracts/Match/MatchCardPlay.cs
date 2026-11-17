using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes the use of an action card from the hand: the card and the cells it acts on, in order. Cards 1 and 2 take no cell; cards 3, 4, 6 and 7 take the caterpillar and its destination; card 5 takes the flower that loses its top level and the cell that receives it; card 8 takes one or two flowers.
    /// </summary>
    [DataContract]
    public sealed class MatchCardPlay
    {
        [DataMember]
        public MatchCard Card { get; set; }

        [DataMember]
        public List<MatchCell> AffectedCells { get; set; } = new List<MatchCell>();
    }
}
