using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class MatchResultEntry
    {
        [DataMember]
        public int Placement { get; set; }

        [DataMember]
        public string Username { get; set; } = string.Empty;

        [DataMember]
        public int Points { get; set; }

        [DataMember]
        public bool IsCurrentPlayer { get; set; }
    }
}
