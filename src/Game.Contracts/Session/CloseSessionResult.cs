using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Confirms that a game session was closed.
    /// </summary>
    [DataContract]
    public enum CloseSessionResult
    {
        [EnumMember]
        SessionClosed,
    }
}
