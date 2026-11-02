using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the image of an avatar, when it exists.
    /// </summary>
    [DataContract]
    public sealed class AvatarResponse
    {
        [DataMember]
        public AvatarLookupStatus Status { get; set; }

        [DataMember]
        public byte[]? Image { get; set; }
    }
}
