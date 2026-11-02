using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a game session was opened, or why the token was rejected.
    /// </summary>
    [DataContract]
    public enum OpenSessionStatus
    {
        [EnumMember]
        SessionOpened,

        [EnumMember]
        InvalidSessionToken,

        [EnumMember]
        SessionTokenExpired,
    }
}
