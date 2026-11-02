using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether the caller entered a room, or why not.
    /// </summary>
    [DataContract]
    public enum JoinRoomStatus
    {
        [EnumMember]
        Joined,

        [EnumMember]
        AlreadyInRoom,

        [EnumMember]
        RoomNotFound,

        [EnumMember]
        RoomFull,

        [EnumMember]
        RoomInMatch,

        [EnumMember]
        Banned,

        [EnumMember]
        NotIdentified,

        [EnumMember]
        DatabaseUnavailable,
    }
}
