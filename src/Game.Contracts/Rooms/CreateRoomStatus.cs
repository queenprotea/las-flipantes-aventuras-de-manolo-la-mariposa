using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a room was created, or why not.
    /// </summary>
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
