using System.Runtime.Serialization;

namespace Game.Match
{
    [DataContract]
    internal sealed class CheckpointRoundScore
    {
        [DataMember(IsRequired = true)]
        public int PlanterPoints { get; set; }

        [DataMember(IsRequired = true)]
        public int ButterflyPoints { get; set; }

        [DataMember(IsRequired = true)]
        public int ScoredPlanterCount { get; set; }
    }
}
