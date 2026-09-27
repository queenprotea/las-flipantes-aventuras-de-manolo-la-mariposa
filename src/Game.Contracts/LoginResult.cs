using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class LoginResult
    {
        [DataMember]
        public LoginStatus Status { get; set; }

        [DataMember]
        public PlayerIdentity Player { get; set; }
    }
}
