using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the outcome of opening a game session and, when it succeeds, the player's identity, room and connection check period.
    /// </summary>
    [DataContract]
    public sealed class OpenSessionResult
    {
        [DataMember]
        public OpenSessionStatus Status { get; set; }

        [DataMember]
        public SessionIdentity? Identity { get; set; }

        [DataMember]
        public Room? CurrentRoom { get; set; }

        [DataMember]
        public int ConnectionCheckPeriodSeconds { get; set; }
    }
}
