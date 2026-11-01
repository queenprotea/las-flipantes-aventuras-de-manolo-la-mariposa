using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class GuestAccessResult
    {
        [DataMember]
        public GuestAccessStatus Status { get; set; }

        [DataMember]
        public string? SessionToken { get; set; }
    }
}
