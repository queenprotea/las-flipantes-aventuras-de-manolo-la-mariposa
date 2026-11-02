using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the outcome of a player search and, when it finds someone, the player and their relationship with the caller.
    /// </summary>
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
