using System.Collections.Generic;
using System.Runtime.Serialization;

using Game.Domain;

namespace Game.Match
{
    [DataContract]
    internal sealed class CheckpointSeatCards
    {
        [DataMember(IsRequired = true)]
        public int SeatNumber { get; set; }

        [DataMember(IsRequired = true)]
        public List<ActionCard>? Hand { get; set; }

        [DataMember(IsRequired = true)]
        public List<ActionCard>? Deck { get; set; }

        [DataMember(IsRequired = true)]
        public bool HasUsedPointsCard { get; set; }
    }
}
