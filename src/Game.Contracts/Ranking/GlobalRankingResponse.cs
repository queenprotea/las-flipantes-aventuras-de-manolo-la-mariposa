using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the global ranking, from the first place down.
    /// </summary>
    [DataContract]
    public sealed class GlobalRankingResponse
    {
        [DataMember]
        public List<RankingEntry> Entries { get; set; } = new List<RankingEntry>();
    }
}