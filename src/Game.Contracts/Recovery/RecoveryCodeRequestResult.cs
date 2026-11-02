using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a recovery code was sent, or why not.
    /// </summary>
    [DataContract]
    public enum RecoveryCodeRequestResult
    {
        [EnumMember]
        CodeSent,

        [EnumMember]
        InvalidEmail,

        [EnumMember]
        MailDeliveryFailed,

        [EnumMember]
        DatabaseUnavailable,
    }
}
