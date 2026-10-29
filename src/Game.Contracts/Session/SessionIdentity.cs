using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class SessionIdentity
    {
        [DataMember]
        public string DisplayName { get; set; } = string.Empty;

        [DataMember]
        public bool IsGuest { get; set; }

        [DataMember]
        public int? PlayerId { get; set; }
    }
}
