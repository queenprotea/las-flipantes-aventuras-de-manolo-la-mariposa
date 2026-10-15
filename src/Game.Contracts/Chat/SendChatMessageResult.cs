using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum SendChatMessageResult
    {
        [EnumMember]
        Sent,

        [EnumMember]
        EmptyMessage,

        [EnumMember]
        MessageTooLong,

        [EnumMember]
        NotInRoom,

        [EnumMember]
        PlayerSanctioned,

        [EnumMember]
        RateLimited,
    }
}
