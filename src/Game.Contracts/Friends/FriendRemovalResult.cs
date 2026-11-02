using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a friendship was ended, or why it could not be.
    /// </summary>
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
