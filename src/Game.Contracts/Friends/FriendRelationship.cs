using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes how another player relates to the caller in terms of friendship.
    /// </summary>
    [DataContract]
    public enum FriendRelationship
    {
        [EnumMember]
        None,

        [EnumMember]
        Friend,

        [EnumMember]
        RequestSent,

        [EnumMember]
        RequestReceived,
    }
}
