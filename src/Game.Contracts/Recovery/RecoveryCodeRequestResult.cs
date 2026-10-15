using System.Runtime.Serialization;

namespace Game.Contracts
{
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
