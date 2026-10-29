using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum KickMemberResult
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
    }
}
