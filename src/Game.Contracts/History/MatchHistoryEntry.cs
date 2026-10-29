using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class MatchHistoryEntry
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public DateTime PlayedAt { get; set; }

        [DataMember]
        public int PlayerCount { get; set; }

        [DataMember]
        public List<string> RivalUsernames { get; set; } = new List<string>();

        [DataMember]
        public MatchOutcome Outcome { get; set; }

        [DataMember]
        public int? Placement { get; set; }

        [DataMember]
        public int? Points { get; set; }
    }
}
