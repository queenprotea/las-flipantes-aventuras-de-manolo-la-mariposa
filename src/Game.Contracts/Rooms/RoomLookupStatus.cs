using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether the caller is in a room.
    /// </summary>
    [DataContract]
    public enum RoomLookupStatus
    {
        [EnumMember]
        InRoom,

        [EnumMember]
        NotInRoom,
    }
}
