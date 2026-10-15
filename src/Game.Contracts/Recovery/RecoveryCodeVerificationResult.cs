using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum RecoveryCodeVerificationResult
    {
        [EnumMember]
        Valid,

        [EnumMember]
        InvalidCode,

        [EnumMember]
        DatabaseUnavailable,
    }
}
