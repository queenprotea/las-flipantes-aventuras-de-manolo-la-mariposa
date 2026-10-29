using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
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
