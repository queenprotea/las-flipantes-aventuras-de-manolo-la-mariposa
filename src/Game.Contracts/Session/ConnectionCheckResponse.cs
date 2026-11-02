using System;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Answers a connection check with the server's time.
    /// </summary>
    [DataContract]
    public sealed class ConnectionCheckResponse
    {
        [DataMember]
        public DateTime ServerTimeUtc { get; set; }
    }
}
