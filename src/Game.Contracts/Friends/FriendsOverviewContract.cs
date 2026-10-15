using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class FriendsOverviewContract
    {
        [DataMember]
        public List<PlayerSummaryContract> Friends { get; set; } = new List<PlayerSummaryContract>();

        [DataMember]
        public List<PlayerSummaryContract> ReceivedRequests { get; set; } = new List<PlayerSummaryContract>();

        [DataMember]
        public List<PlayerSummaryContract> SentRequests { get; set; } = new List<PlayerSummaryContract>();
    }
}
