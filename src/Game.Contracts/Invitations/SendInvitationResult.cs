using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a room invitation was sent, or why it was not. EmailNotSent means the invitation was stored but its email could not be delivered.
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
        EmailNotSent,

        [EnumMember]
        DatabaseUnavailable,
    }
}
