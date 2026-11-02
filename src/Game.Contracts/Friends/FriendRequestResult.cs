using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a friend request was sent, or why it was not.
    /// </summary>
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
