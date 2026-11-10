using System;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the answer to a room invitation: the room entered when it was accepted, and when the ban ends, in UTC, when a ban kept the caller out.
    /// </summary>
    [DataContract]
    public sealed class RespondInvitationResult
    {
        [DataMember]
        public RespondInvitationStatus Status { get; set; }

        [DataMember]
        public Room? Room { get; set; }

        [DataMember]
        public DateTime? BanEndsAtUtc { get; set; }
    }
}
