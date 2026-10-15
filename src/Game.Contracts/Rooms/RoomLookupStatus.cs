using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum RoomLookupStatus
    {
        [EnumMember]
        InRoom,

        [EnumMember]
        NotInRoom,
    }
}
