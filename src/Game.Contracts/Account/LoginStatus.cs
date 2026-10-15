using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum LoginStatus
    {
        [EnumMember]
        LoggedIn,

        [EnumMember]
        InvalidCredentials,

        [EnumMember]
        DatabaseUnavailable,
    }
}
