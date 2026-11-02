using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the matches of the caller's history.
    /// </summary>
    [DataContract]
    public sealed class MatchHistoryResponse
    {
        [DataMember]
        public List<MatchHistoryEntry> Entries { get; set; } = new List<MatchHistoryEntry>();
    }
}
