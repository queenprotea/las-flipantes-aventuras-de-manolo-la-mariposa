using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a profile was updated, or which part of the change was rejected.
    /// </summary>
    [DataContract]
    public enum UpdateProfileStatus
    {
        [EnumMember]
        Updated,

        [EnumMember]
        UsernameTaken,

        [EnumMember]
        InvalidUsername,

        [EnumMember]
        AvatarIncomplete,

        [EnumMember]
        RoomInMatch,

        [EnumMember]
        DatabaseUnavailable,

        [EnumMember]
        StorageUnavailable,
    }
}
