using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a username can be used, or why not.
    /// </summary>
    [DataContract]
    public enum UsernameAvailability
    {
        [EnumMember]
        Available,

        [EnumMember]
        Taken,

        [EnumMember]
        InvalidFormat,

        [EnumMember]
        DatabaseUnavailable,
    }
}
