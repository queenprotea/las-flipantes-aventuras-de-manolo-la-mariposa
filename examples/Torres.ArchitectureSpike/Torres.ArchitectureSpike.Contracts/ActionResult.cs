using System.Runtime.Serialization;

namespace Torres.ArchitectureSpike.Contracts
{
    [DataContract]
    public enum ActionResult
    {
        [EnumMember]
        Applied,

        [EnumMember]
        NotYourTurn,

        [EnumMember]
        InvalidStepCount,

        [EnumMember]
        MatchFinished,

        [EnumMember]
        NotInMatch,
    }
}
