using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the outcome of confirming a new account and, when it was created, the player's identity and session token.
    /// </summary>
    [DataContract]
    public sealed class RegistrationConfirmationResult
    {
        [DataMember]
        public RegistrationConfirmationStatus Status { get; set; }

        [DataMember]
        public PlayerIdentity? Player { get; set; }

        [DataMember]
        public string? SessionToken { get; set; }
    }
}
