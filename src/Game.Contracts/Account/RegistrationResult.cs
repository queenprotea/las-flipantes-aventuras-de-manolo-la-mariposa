using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether the verification code of a new account was sent, or which rule the registration broke.
    /// </summary>
    [DataContract]
    public enum RegistrationResult
    {
        [EnumMember]
        CodeSent,

        [EnumMember]
        UsernameTaken,

        [EnumMember]
        EmailTaken,

        [EnumMember]
        InvalidEmail,

        [EnumMember]
        InvalidPassword,

        [EnumMember]
        InvalidUsername,

        [EnumMember]
        TooManyRequests,

        [EnumMember]
        MailDeliveryFailed,

        [EnumMember]
        DatabaseUnavailable,
    }
}
