using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum MatchDetailStatus
    {
        [EnumMember]
        Found,

        [EnumMember]
        NotFound,

        [EnumMember]
        NotYours,
    }
}
