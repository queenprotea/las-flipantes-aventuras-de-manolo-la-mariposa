using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Identifies the owner of a game session, with an account or as a guest.
    /// </summary>
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
