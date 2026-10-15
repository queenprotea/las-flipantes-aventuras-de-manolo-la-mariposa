using System.Runtime.Serialization;

namespace Game.Contracts
{
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
        DatabaseUnavailable,
    }
}
