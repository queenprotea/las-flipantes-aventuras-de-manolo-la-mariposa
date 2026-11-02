using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells how a room invitation was answered, or why it could not be.
    /// </summary>
    [DataContract]
    public enum RespondInvitationStatus
    {
        [EnumMember]
        Accepted,

        [EnumMember]
        Rejected,

        [EnumMember]
        InvitationNoLongerExists,

        [EnumMember]
        RoomFull,

        [EnumMember]
        RoomInMatch,

        [EnumMember]
        AlreadyInRoom,

        [EnumMember]
        DatabaseUnavailable,
    }
}
