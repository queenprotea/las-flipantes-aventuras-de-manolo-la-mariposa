using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells how a friend request was answered, or why it could not be.
    /// </summary>
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
