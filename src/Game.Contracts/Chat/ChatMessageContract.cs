using System;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class ChatMessageContract
    {
        [DataMember]
        public int SenderPlayerId { get; set; }

        [DataMember]
        public string SenderUsername { get; set; } = string.Empty;

        [DataMember]
        public string Text { get; set; } = string.Empty;

        [DataMember]
        public DateTime SentAtUtc { get; set; }
    }
}
