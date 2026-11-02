using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether an account was created, or which rule the registration broke.
    /// </summary>
    [DataContract]
    public enum RegistrationResult
    {
        [EnumMember]
        Created,

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
        DatabaseUnavailable,
    }
}
