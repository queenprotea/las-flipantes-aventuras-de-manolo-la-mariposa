using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum UpdateProfileResult
    {
        [EnumMember]
        Updated,

        [EnumMember]
        UsernameTaken,

        [EnumMember]
        InvalidUsername,

        [EnumMember]
        AvatarTooLarge,

        [EnumMember]
        AvatarInvalidFormat,

        [EnumMember]
        DatabaseUnavailable,
    }
}
