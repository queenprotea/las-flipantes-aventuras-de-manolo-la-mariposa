using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the outcome of a profile update and, when it succeeds, the avatar reference the profile has now.
    /// </summary>
    [DataContract]
    public sealed class UpdateProfileResult
    {
        [DataMember]
        public UpdateProfileStatus Status { get; set; }

        [DataMember]
        public string? AvatarReference { get; set; }
    }
}
