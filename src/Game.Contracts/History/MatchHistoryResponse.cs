using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries one page of the caller's history, the number of that page and how many pages there are.
    /// </summary>
    [DataContract]
    public sealed class MatchHistoryResponse
    {
        [DataMember]
        public List<MatchHistoryEntry> Entries { get; set; } = new List<MatchHistoryEntry>();

        [DataMember]
        public int PageNumber { get; set; }

        [DataMember]
        public int PageCount { get; set; }
    }
}
