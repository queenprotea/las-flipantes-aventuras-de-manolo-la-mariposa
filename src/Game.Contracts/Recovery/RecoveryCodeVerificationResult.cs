using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a recovery code is valid.
    /// </summary>
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
