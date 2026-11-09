using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a part of an avatar upload was accepted, or why it was rejected.
    /// </summary>
    [DataContract]
    public enum SendAvatarPartResult
    {
        [EnumMember]
        Accepted,

        [EnumMember]
        NoUploadInProgress,

        [EnumMember]
        OutOfRange,

        [EnumMember]
        AvatarInvalidFormat,

        [EnumMember]
        ThumbnailInvalid,

        [EnumMember]
        RoomInMatch,
    }
}
