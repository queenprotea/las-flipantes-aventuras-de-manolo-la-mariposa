using System.Runtime.Serialization;

namespace Game.Match
{
    [DataContract]
    internal sealed class CheckpointFlower
    {
        [DataMember(IsRequired = true)]
        public CheckpointCell? Cell { get; set; }

        [DataMember(IsRequired = true)]
        public int Level { get; set; }
    }
}
