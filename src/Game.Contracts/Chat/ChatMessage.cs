using System;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes one message of a room chat, with a copy of its author's name and its place in the room's sequence.
    /// </summary>
    [DataContract]
    public sealed class ChatMessage
    {
        [DataMember]
        public int RoomId { get; set; }

        [DataMember]
        public int MessageId { get; set; }

        [DataMember]
        public int SenderMemberId { get; set; }

        [DataMember]
        public string SenderDisplayName { get; set; } = string.Empty;

        [DataMember]
        public bool SenderIsGuest { get; set; }

        [DataMember]
        public string Text { get; set; } = string.Empty;

        [DataMember]
        public DateTime SentAtUtc { get; set; }
    }
}
