using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class GlobalRankingResponseContract
    {
        [DataMember]
        public List<RankingEntryContract> Entries { get; set; } = new List<RankingEntryContract>();
    }
}