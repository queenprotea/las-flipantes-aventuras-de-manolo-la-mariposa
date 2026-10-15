using System.Runtime.Serialization;

namespace Game.Contracts
{
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
