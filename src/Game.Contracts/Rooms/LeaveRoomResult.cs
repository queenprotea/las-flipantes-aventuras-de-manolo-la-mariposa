using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether the caller left a room.
    /// </summary>
    [DataContract]
    public enum LeaveRoomResult
    {
        [EnumMember]
        Left,

        [EnumMember]
        NotInRoom,

        [EnumMember]
        RoomInMatch,
    }
}
