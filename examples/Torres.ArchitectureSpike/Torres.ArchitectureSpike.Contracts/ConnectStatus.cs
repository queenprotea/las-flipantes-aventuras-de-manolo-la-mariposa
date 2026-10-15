using System.Runtime.Serialization;

namespace Torres.ArchitectureSpike.Contracts
{
    [DataContract]
    public enum ConnectStatus
    {
        [EnumMember]
        Connected,

        [EnumMember]
        InvalidName,

        [EnumMember]
        NameInUse,
    }
}
