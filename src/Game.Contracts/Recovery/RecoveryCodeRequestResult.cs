using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a request for a recovery code was accepted, or why not.
    /// </summary>
    [DataContract]
    public enum RecoveryCodeRequestResult
    {
        [EnumMember]
        CodeSent,

        [EnumMember]
        InvalidEmail,

        [EnumMember]
        TooManyRequests,

        [EnumMember]
        MailDeliveryFailed,

        [EnumMember]
        DatabaseUnavailable,
    }
}
