using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether an account was created from its verification code, or why not.
    /// </summary>
    [DataContract]
    public enum RegistrationConfirmationStatus
    {
        [EnumMember]
        Created,

        [EnumMember]
        InvalidCode,

        [EnumMember]
        UsernameTaken,

        [EnumMember]
        EmailTaken,

        [EnumMember]
        DatabaseUnavailable,
    }
}
