using System;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes an invitation to a room, with who sent it and when.
    /// </summary>
    [DataContract]
    public sealed class RoomInvitation
    {
        [DataMember]
        public int InvitationId { get; set; }

        [DataMember]
        public PlayerSummary Sender { get; set; } = new PlayerSummary();

        [DataMember]
        public DateTime SentAtUtc { get; set; }
    }
}
