using System.Runtime.Serialization;

namespace Torres.ArchitectureSpike.Contracts
{
    [DataContract]
    public sealed class PlayerConnectionNotice
    {
        [DataMember]
        public string PlayerName { get; set; } = string.Empty;

        [DataMember]
        public PlayerConnectionChange Change { get; set; }
    }
}
