using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class PlayerSearchResponse
    {
        [DataMember]
        public PlayerSearchStatus Status { get; set; }

        [DataMember]
        public PlayerSummary? Player { get; set; }

        [DataMember]
        public FriendRelationship Relationship { get; set; }
    }
}
