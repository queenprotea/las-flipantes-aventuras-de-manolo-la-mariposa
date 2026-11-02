using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether the server can serve requests.
    /// </summary>
    [DataContract]
    public enum ServerStatus
    {
        [EnumMember]
        Ready,

        [EnumMember]
        DatabaseUnavailable,
    }
}
