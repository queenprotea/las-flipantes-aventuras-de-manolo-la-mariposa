using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether an email can be used for a new account, or why not.
    /// </summary>
    [DataContract]
    public enum EmailAvailability
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
