using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes the placement and points of one player with an account in a match.
    /// </summary>
    [DataContract]
    public sealed class MatchResultEntry
    {
        [DataMember]
        public int Placement { get; set; }

        [DataMember]
        public string Username { get; set; } = string.Empty;

        [DataMember]
        public int Points { get; set; }

        [DataMember]
        public bool IsCurrentPlayer { get; set; }

        [DataMember]
        public bool WasWithdrawn { get; set; }
    }
}
