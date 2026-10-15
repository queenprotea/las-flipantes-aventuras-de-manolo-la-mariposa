using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class MatchHistoryResponseContract
    {
        [DataMember]
        public List<MatchHistoryEntryContract> Entries { get; set; } = new List<MatchHistoryEntryContract>();
    }
}
