using System.Runtime.Serialization;

namespace Torres.ArchitectureSpike.Contracts
{
    [DataContract]
    public enum MatchEventCode
    {
        [EnumMember]
        MatchStarted,

        [EnumMember]
        StepApplied,

        [EnumMember]
        TurnExpired,

        [EnumMember]
        MatchFinished,
    }
}
