using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells how many action points each turn action costs in this match, as the rules catalogue sets them.
    /// </summary>
    [DataContract]
    public sealed class MatchActionCosts
    {
        [DataMember]
        public int CaterpillarPlacement { get; set; }

        [DataMember]
        public int Movement { get; set; }

        [DataMember]
        public int LevelClimb { get; set; }

        [DataMember]
        public int FlowerLevelAddition { get; set; }
    }
}
