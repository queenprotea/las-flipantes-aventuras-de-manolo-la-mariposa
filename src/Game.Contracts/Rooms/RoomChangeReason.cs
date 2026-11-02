using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells what changed in a room.
    /// </summary>
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
