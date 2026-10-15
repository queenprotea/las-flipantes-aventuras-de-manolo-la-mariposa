using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum FriendRemovalResult
    {
        [EnumMember]
        Removed,

        [EnumMember]
        NotFriends,

        [EnumMember]
        DatabaseUnavailable,
    }
}
