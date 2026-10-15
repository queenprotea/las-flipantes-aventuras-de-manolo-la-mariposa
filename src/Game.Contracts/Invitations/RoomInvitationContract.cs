using System;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class RoomInvitationContract
    {
        [DataMember]
        public int InvitationId { get; set; }

        [DataMember]
        public PlayerSummaryContract Sender { get; set; } = new PlayerSummaryContract();

        [DataMember]
        public DateTime SentAtUtc { get; set; }
    }
}
