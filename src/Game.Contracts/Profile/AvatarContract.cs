using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class AvatarContract
    {
        [DataMember]
        public AvatarLookupStatus Status { get; set; }

        [DataMember]
        public byte[]? Image { get; set; }
    }
}
