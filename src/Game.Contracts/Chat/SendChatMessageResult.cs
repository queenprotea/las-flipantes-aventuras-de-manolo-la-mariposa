using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a chat message was sent, or why it was rejected.
    /// </summary>
    [DataContract]
    public enum SendChatMessageResult
    {
        [EnumMember]
        Sent,

        [EnumMember]
        EmptyMessage,

        [EnumMember]
        MessageTooLong,

        [EnumMember]
        NotInRoom,

        [EnumMember]
        NotIdentified,
    }
}
