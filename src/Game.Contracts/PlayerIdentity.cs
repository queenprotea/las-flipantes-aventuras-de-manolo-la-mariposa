using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class PlayerIdentity
    {
        [DataMember]
        public int PlayerId { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Email { get; set; }
    }
}
