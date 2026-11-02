using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a profile was updated, or which change was rejected.
    /// </summary>
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
