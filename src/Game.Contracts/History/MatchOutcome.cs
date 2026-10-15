using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum MatchOutcome
    {
        [EnumMember]
        Finished,

        [EnumMember]
        Abandoned,

        [EnumMember]
        Interrupted,
    }
}
