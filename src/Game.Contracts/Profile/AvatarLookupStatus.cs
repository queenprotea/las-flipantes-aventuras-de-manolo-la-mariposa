using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum AvatarLookupStatus
    {
        [EnumMember]
        Found,

        [EnumMember]
        NotFound,
    }
}
