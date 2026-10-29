using System.Runtime.Serialization;

namespace Game.Contracts
{
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
