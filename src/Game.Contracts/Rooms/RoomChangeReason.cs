using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum RoomChangeReason
    {
        [EnumMember]
        MemberJoined,

        [EnumMember]
        MemberLeft,

        [EnumMember]
        MemberKicked,

        [EnumMember]
        HostChanged,

        [EnumMember]
        YouWereKicked,

        [EnumMember]
        RoomClosed,

        [EnumMember]
        MatchStarted,

        [EnumMember]
        MemberWentInactive,

        [EnumMember]
        MemberReturned,

        [EnumMember]
        MemberTimedOut,
    }
}
