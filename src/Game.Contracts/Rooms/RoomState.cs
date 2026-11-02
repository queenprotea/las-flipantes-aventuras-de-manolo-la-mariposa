using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells in which stage of its life a room is.
    /// </summary>
    [DataContract]
    public enum RoomState
    {
        [EnumMember]
        Waiting,

        [EnumMember]
        InMatch,

        [EnumMember]
        Closed,
    }
}
