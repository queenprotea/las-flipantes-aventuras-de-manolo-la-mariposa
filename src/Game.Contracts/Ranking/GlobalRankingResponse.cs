using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class GlobalRankingResponse
    {
        [DataMember]
        public List<RankingEntry> Entries { get; set; } = new List<RankingEntry>();
    }
}