using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum FriendRequestResponseResult
    {
        [EnumMember]
        Accepted,

        [EnumMember]
        Rejected,

        [EnumMember]
        RequestNoLongerExists,

        [EnumMember]
        DatabaseUnavailable,
    }
}
