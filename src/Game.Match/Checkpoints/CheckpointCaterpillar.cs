using System.Runtime.Serialization;

namespace Game.Match
{
    [DataContract]
    internal sealed class CheckpointCaterpillar
    {
        [DataMember(IsRequired = true)]
        public int SeatNumber { get; set; }

        [DataMember(IsRequired = true)]
        public CheckpointCell? Cell { get; set; }
    }
}
