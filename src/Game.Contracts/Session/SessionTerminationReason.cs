using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public enum SessionTerminationReason
    {
        [EnumMember]
        AccountSignedInElsewhere,
    }
}
