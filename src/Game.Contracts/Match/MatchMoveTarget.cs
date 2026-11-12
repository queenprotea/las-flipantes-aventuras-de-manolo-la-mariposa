using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes a move the viewer may make in their turn and the action points it costs.
    /// </summary>
    [DataContract]
    public sealed class MatchMoveTarget
    {
        [DataMember]
        public MatchCell Origin { get; set; } = new MatchCell();

        [DataMember]
        public MatchCell Destination { get; set; } = new MatchCell();

        [DataMember]
        public int ActionPointCost { get; set; }
    }
}
