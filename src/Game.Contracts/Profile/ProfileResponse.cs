using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes the profile of the caller's account.
    /// </summary>
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
