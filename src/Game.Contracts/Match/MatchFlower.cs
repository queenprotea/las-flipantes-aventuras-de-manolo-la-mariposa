using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes a flower of the board: its cell, how many levels high it stands and the planter it belongs to.
    /// </summary>
    [DataContract]
    public sealed class MatchFlower
    {
        [DataMember]
        public MatchCell Cell { get; set; } = new MatchCell();

        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public int PlanterNumber { get; set; }
    }
}
