using System.Runtime.Serialization;

namespace Game.Contracts
{
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
