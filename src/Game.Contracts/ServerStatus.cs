using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum ServerStatus
    {
        [EnumMember]
        Ready,

        [EnumMember]
        DatabaseUnavailable,
    }
}
