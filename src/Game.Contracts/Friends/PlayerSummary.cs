using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes a player with an account as other players see them in lists.
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
    }
}
