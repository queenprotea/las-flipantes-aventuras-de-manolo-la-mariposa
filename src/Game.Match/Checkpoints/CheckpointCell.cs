using System.Runtime.Serialization;

namespace Game.Match
{
    [DataContract]
    internal sealed class CheckpointCell
    {
        [DataMember(IsRequired = true)]
        public int Row { get; set; }

        [DataMember(IsRequired = true)]
        public int Column { get; set; }
    }
}
