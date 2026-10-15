using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum EmailAvailability
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
