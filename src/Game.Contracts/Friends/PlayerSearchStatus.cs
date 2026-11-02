using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a player search found an account.
    /// </summary>
    [DataContract]
    public enum PlayerSearchStatus
    {
        [EnumMember]
        Found,

        [EnumMember]
        NotFound,
    }
}
