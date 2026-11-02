using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells why the server ended a game session.
    /// </summary>
    [DataContract]
    public enum SessionTerminationReason
    {
        [EnumMember]
        AccountSignedInElsewhere,
    }
}
