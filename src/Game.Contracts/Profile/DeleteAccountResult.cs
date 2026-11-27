using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether the caller's account was deleted, or why not.
    /// </summary>
    [DataContract]
    public enum DeleteAccountResult
    {
        [EnumMember]
        Deleted,

        [EnumMember]
        MatchInProgress,

        [EnumMember]
        DatabaseUnavailable,
    }
}
