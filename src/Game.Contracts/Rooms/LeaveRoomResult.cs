using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum LeaveRoomResult
    {
        [EnumMember]
        Left,

        [EnumMember]
        NotInRoom,

        [EnumMember]
        DatabaseUnavailable,
    }
}
