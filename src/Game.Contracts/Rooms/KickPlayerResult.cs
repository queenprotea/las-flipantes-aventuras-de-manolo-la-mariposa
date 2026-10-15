using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum KickPlayerResult
    {
        [EnumMember]
        Kicked,

        [EnumMember]
        NotHost,

        [EnumMember]
        NotInRoom,

        [EnumMember]
        TargetNotInRoom,

        [EnumMember]
        CannotKickSelf,

        [EnumMember]
        DatabaseUnavailable,
    }
}
