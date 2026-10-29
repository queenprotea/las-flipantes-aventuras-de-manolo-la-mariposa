using System;
using System.Runtime.Serialization;

namespace Game.Contracts
{
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
