using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Availability of the server as reported by <see cref="IServerStatusService"/>.
    /// </summary>
    [DataContract]
    public enum ServerStatus
    {
        /// <summary>
        /// The server and its database are available.
        /// </summary>
        [EnumMember]
        Ready,

        /// <summary>
        /// The server is running but cannot read its database.
        /// </summary>
        [EnumMember]
        DatabaseUnavailable,
    }
}
