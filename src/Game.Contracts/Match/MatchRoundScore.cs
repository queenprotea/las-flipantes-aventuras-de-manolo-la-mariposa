using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes the points a seat earned when a round closed, split between its planters and the butterfly.
    /// </summary>
    [DataContract]
    public sealed class MatchRoundScore
    {
        [DataMember]
        public int PlanterPoints { get; set; }

        [DataMember]
        public int ButterflyPoints { get; set; }
    }
}
