using System.Runtime.Serialization;

namespace Game.Contracts
{
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
