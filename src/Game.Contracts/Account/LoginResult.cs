using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the outcome of a login and, when it succeeds, the player's identity and session token.
    /// </summary>
    [DataContract]
    public sealed class LoginResult
    {
        [DataMember]
        public LoginStatus Status { get; set; }

        [DataMember]
        public PlayerIdentity? Player { get; set; }

        [DataMember]
        public string? SessionToken { get; set; }
    }
}
