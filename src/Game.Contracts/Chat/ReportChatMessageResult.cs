using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a chat message was reported, or why the report was rejected.
    /// </summary>
    [DataContract]
    public enum ReportChatMessageResult
    {
        [EnumMember]
        Reported,

        [EnumMember]
        NotIdentified,

        [EnumMember]
        ReporterIsGuest,

        [EnumMember]
        NotInRoom,

        [EnumMember]
        MessageNotFound,

        [EnumMember]
        AuthorIsGuest,

        [EnumMember]
        OwnMessage,

        [EnumMember]
        DatabaseUnavailable,
    }
}
