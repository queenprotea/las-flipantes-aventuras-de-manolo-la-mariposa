using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum PlayerSearchStatus
    {
        [EnumMember]
        Found,

        [EnumMember]
        NotFound,
    }
}
