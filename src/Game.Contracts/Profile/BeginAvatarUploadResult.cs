using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether an avatar upload started, or why it was rejected.
    /// </summary>
    [DataContract]
    public enum BeginAvatarUploadResult
    {
        [EnumMember]
        Started,

        [EnumMember]
        AvatarTooLarge,

        [EnumMember]
        ThumbnailInvalid,

        [EnumMember]
        RoomInMatch,
    }
}
