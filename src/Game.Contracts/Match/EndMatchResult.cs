using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether the caller's match was ended, or why not.
    /// </summary>
    [DataContract]
    public enum EndMatchResult
    {
        [EnumMember]
        Ended,

        [EnumMember]
        NotInMatch,

        [EnumMember]
        NotIdentified,
    }
}
