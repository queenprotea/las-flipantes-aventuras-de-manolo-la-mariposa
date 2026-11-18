using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether the caller took back its seat in a match that waits for its players after a server restart, or why not.
    /// </summary>
    [DataContract]
    public enum ReturnToMatchResult
    {
        [EnumMember]
        Returned,

        [EnumMember]
        NoMatchToResume,

        [EnumMember]
        NotIdentified,
    }
}
