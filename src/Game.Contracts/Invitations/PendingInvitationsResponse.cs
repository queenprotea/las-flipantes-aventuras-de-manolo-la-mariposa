using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the room invitations the caller has not answered.
    /// </summary>
    [DataContract]
    public sealed class PendingInvitationsResponse
    {
        [DataMember]
        public List<RoomInvitation> Invitations { get; set; } = new List<RoomInvitation>();
    }
}
