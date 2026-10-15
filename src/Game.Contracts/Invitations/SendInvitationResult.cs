using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum SendInvitationResult
    {
        [EnumMember]
        Sent,

        [EnumMember]
        NotInRoom,

        [EnumMember]
        RoomFull,

        [EnumMember]
        RoomInMatch,

        [EnumMember]
        PlayerNotFound,

        [EnumMember]
        PlayerNotFriend,

        [EnumMember]
        PlayerOffline,

        [EnumMember]
        PlayerAlreadyInRoom,

        [EnumMember]
        InvitationAlreadyPending,

        [EnumMember]
        DatabaseUnavailable,
    }
}
