using System;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes a pending invitation to a room, with the room code, who sent it and when, in UTC.
    /// </summary>
    [DataContract]
    public sealed class RoomInvitation
    {
        [DataMember]
        public int RoomId { get; set; }

        [DataMember]
        public string RoomCode { get; set; } = string.Empty;

        [DataMember]
        public PlayerSummary Sender { get; set; } = new PlayerSummary();

        [DataMember]
        public DateTime SentAtUtc { get; set; }
    }
}
