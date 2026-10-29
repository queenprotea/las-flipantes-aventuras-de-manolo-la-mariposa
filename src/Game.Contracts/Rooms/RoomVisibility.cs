using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum RoomVisibility
    {
        [EnumMember]
        Public,

        [EnumMember]
        Private,
    }
}
