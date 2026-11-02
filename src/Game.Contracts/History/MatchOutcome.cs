using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells how a match ended.
    /// </summary>
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
