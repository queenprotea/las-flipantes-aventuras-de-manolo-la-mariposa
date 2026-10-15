using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class PendingInvitationsContract
    {
        [DataMember]
        public List<RoomInvitationContract> Invitations { get; set; } = new List<RoomInvitationContract>();
    }
}
