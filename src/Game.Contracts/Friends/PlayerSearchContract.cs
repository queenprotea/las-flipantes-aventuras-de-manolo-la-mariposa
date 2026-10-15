using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class PlayerSearchContract
    {
        [DataMember]
        public PlayerSearchStatus Status { get; set; }

        [DataMember]
        public PlayerSummaryContract? Player { get; set; }

        [DataMember]
        public FriendRelationship Relationship { get; set; }
    }
}
