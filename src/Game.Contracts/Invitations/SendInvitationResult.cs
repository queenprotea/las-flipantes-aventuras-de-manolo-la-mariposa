using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a room invitation was sent, or why it was not.
    /// </summary>
    [DataContract]
    public enum SendInvitationResult
    {
        [EnumMember]
        Sent,

        [EnumMember]
        NotInRoom,

        [EnumMember]
        PlayerNotFound,

        [EnumMember]
        AlreadyInThisRoom,

        [EnumMember]
        InvitationAlreadyPending,

        [EnumMember]
        DatabaseUnavailable,
    }
}
