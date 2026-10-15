using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum UsernameAvailability
    {
        [EnumMember]
        Available,

        [EnumMember]
        Taken,

        [EnumMember]
        InvalidFormat,

        [EnumMember]
        DatabaseUnavailable,
    }
}
