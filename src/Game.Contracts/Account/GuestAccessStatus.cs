using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a guest could enter, or why the alias was rejected.
    /// </summary>
    [DataContract]
    public enum GuestAccessStatus
    {
        [EnumMember]
        Entered,

        [EnumMember]
        AliasRequired,

        [EnumMember]
        AliasTooLong,
    }
}
