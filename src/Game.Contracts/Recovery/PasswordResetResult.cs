using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a password was replaced, or why not.
    /// </summary>
    [DataContract]
    public enum PasswordResetResult
    {
        [EnumMember]
        Reset,

        [EnumMember]
        InvalidCode,

        [EnumMember]
        InvalidPassword,

        [EnumMember]
        DatabaseUnavailable,
    }
}
