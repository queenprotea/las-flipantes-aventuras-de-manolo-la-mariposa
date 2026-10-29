using System.Runtime.Serialization;

namespace Game.Contracts
{
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
