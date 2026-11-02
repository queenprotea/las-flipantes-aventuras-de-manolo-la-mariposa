using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a match can be shown to the caller, or why not.
    /// </summary>
    [DataContract]
    public enum MatchDetailStatus
    {
        [EnumMember]
        Found,

        [EnumMember]
        NotFound,

        [EnumMember]
        NotYours,
    }
}
