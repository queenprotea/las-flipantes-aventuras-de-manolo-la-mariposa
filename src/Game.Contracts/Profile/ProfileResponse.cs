using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class ProfileResponse
    {
        [DataMember]
        public string Username { get; set; } = string.Empty;

        [DataMember]
        public string Email { get; set; } = string.Empty;

        [DataMember]
        public string? AvatarReference { get; set; }
    }
}
