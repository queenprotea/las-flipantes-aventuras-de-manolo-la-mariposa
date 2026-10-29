using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum OpenSessionStatus
    {
        [EnumMember]
        SessionOpened,

        [EnumMember]
        InvalidSessionToken,

        [EnumMember]
        SessionTokenExpired,
    }
}
