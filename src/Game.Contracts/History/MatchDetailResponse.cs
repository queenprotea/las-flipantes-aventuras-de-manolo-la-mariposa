using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the result of one match, with the placement and points of each player with an account. PlayedAt is when the match finished, in UTC; an interrupted match has no duration and no results. GuestCount is how many players left no result row: guests, or accounts deleted afterwards.
    /// </summary>
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

        [DataMember]
        public int GuestCount { get; set; }
    }
}
