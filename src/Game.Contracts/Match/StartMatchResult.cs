using System;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Carries the outcome of starting a match and, when an active member is banned, who it is and how long the ban still lasts.
    /// </summary>
    [DataContract]
    public sealed class StartMatchResult
    {
        [DataMember]
        public StartMatchStatus Status { get; set; }

        [DataMember]
        public string BannedDisplayName { get; set; } = string.Empty;

        [DataMember]
        public bool IsBanPermanent { get; set; }

        [DataMember]
        public TimeSpan BanTimeLeft { get; set; }
    }
}
