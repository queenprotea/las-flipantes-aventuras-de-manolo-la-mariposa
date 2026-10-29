using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class MatchHistoryResponse
    {
        [DataMember]
        public List<MatchHistoryEntry> Entries { get; set; } = new List<MatchHistoryEntry>();
    }
}
