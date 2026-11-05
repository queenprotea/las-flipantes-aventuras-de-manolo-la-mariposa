using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a member was removed from a room, or why not.
    /// </summary>
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

        [EnumMember]
        RoomInMatch,
    }
}
