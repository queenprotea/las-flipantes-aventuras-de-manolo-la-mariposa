using System;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Answers a connection check with the server's time and a renewed session token, which lets the client open the session again if the connection is lost.
    /// </summary>
    [DataContract]
    public sealed class ConnectionCheckResponse
    {
        [DataMember]
        public DateTime ServerTimeUtc { get; set; }

        [DataMember]
        public string SessionToken { get; set; } = string.Empty;
    }
}
