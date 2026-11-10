using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a login succeeded, or why it was rejected.
    /// </summary>
    [DataContract]
    public enum LoginStatus
    {
        [EnumMember]
        LoggedIn,

        [EnumMember]
        InvalidCredentials,

        [EnumMember]
        AccountBanned,

        [EnumMember]
        DatabaseUnavailable,
    }
}
