using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the caller's friends and the friend requests the caller received and sent.
    /// </summary>
    [DataContract]
    public sealed class FriendsOverviewResponse
    {
        [DataMember]
        public List<PlayerSummary> Friends { get; set; } = new List<PlayerSummary>();

        [DataMember]
        public List<PlayerSummary> ReceivedRequests { get; set; } = new List<PlayerSummary>();

        [DataMember]
        public List<PlayerSummary> SentRequests { get; set; } = new List<PlayerSummary>();
    }
}
