using System.Runtime.Serialization;

namespace Torres.ArchitectureSpike.Contracts
{
    [DataContract]
    public enum PlayerConnectionChange
    {
        [EnumMember]
        Connected,

        [EnumMember]
        Closed,

        [EnumMember]
        Faulted,
    }
}
