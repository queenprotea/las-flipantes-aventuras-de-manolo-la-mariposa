using System.Runtime.Serialization;

namespace Torres.ArchitectureSpike.Contracts
{
    [DataContract]
    public enum MatchEntryStatus
    {
        [EnumMember]
        WaitingForOpponent,

        [EnumMember]
        MatchStarted,

        [EnumMember]
        AlreadyInMatch,

        [EnumMember]
        NotConnected,
    }
}
