using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Match
{
    [DataContract]
    internal sealed class CheckpointStanding
    {
        [DataMember(IsRequired = true)]
        public int SeatNumber { get; set; }

        [DataMember(IsRequired = true)]
        public List<CheckpointRoundScore>? RoundScores { get; set; }

        [DataMember(IsRequired = true)]
        public bool IsRetired { get; set; }

        [DataMember(IsRequired = true)]
        public int CardPoints { get; set; }
    }
}
