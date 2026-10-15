using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum PasswordResetResult
    {
        [EnumMember]
        Reset,

        [EnumMember]
        InvalidCode,

        [EnumMember]
        InvalidPassword,

        [EnumMember]
        DatabaseUnavailable,
    }
}
