using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the outcome of entering as a guest and, when it succeeds, the session token.
    /// </summary>
    [DataContract]
    public sealed class GuestAccessResult
    {
        [DataMember]
        public GuestAccessStatus Status { get; set; }

        [DataMember]
        public string? SessionToken { get; set; }
    }
}
