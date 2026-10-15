using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum FriendRequestResult
    {
        [EnumMember]
        Sent,

        [EnumMember]
        AlreadyFriends,

        [EnumMember]
        RequestAlreadyPending,

        [EnumMember]
        PlayerNotFound,

        [EnumMember]
        DatabaseUnavailable,
    }
}
