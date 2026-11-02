using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the messages a room chat keeps and the limits a client needs to write and keep messages.
    /// </summary>
    [DataContract]
    public sealed class ChatHistoryResponse
    {
        [DataMember]
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        [DataMember]
        public int MessageMaxLength { get; set; }

        [DataMember]
        public int MessagesKeptPerRoom { get; set; }
    }
}
