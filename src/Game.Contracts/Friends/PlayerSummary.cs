using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes a player with an account as other players see them in lists. Whether the player has an
    /// open game session is only told for an accepted friend; for anyone else it always reads as offline.
    /// </summary>
    [DataContract]
    public sealed class PlayerSummary
    {
        [DataMember]
        public int PlayerId { get; set; }

        [DataMember]
        public string Username { get; set; } = string.Empty;

        [DataMember]
        public string? AvatarReference { get; set; }

        [DataMember]
        public bool IsOnline { get; set; }
    }
}
