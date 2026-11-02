using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether an avatar was found.
    /// </summary>
    [DataContract]
    public enum AvatarLookupStatus
    {
        [EnumMember]
        Found,

        [EnumMember]
        NotFound,
    }
}
