using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class PendingInvitationsResponse
    {
        [DataMember]
        public List<RoomInvitation> Invitations { get; set; } = new List<RoomInvitation>();
    }
}
