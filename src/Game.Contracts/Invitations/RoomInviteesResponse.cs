using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the identifiers of the accounts with a pending invitation to the caller's room.
    /// </summary>
    [DataContract]
    public sealed class RoomInviteesResponse
    {
        [DataMember]
        public List<int> InvitedPlayerIds { get; set; } = new List<int>();
    }
}
