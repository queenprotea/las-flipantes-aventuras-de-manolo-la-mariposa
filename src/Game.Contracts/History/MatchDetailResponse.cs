using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class MatchDetailResponse
    {
        [DataMember]
        public MatchDetailStatus Status { get; set; }

        [DataMember]
        public DateTime PlayedAt { get; set; }

        [DataMember]
        public int PlayerCount { get; set; }

        [DataMember]
        public int? DurationSeconds { get; set; }

        [DataMember]
        public MatchOutcome Outcome { get; set; }

        [DataMember]
        public List<MatchResultEntry> Results { get; set; } = new List<MatchResultEntry>();
    }
}
