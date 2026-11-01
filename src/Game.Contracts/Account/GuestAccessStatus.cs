using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum GuestAccessStatus
    {
        [EnumMember]
        Entered,

        [EnumMember]
        AliasRequired,

        [EnumMember]
        AliasTooLong,
    }
}
