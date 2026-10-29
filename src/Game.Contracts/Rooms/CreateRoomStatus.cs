using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum CreateRoomStatus
    {
        [EnumMember]
        Created,

        [EnumMember]
        AlreadyInRoom,

        [EnumMember]
        Banned,

        [EnumMember]
        InvalidVisibility,

        [EnumMember]
        NotIdentified,

        [EnumMember]
        DatabaseUnavailable,
    }
}
