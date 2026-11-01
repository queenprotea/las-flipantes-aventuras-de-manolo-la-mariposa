using System;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class ConnectionCheckResponse
    {
        [DataMember]
        public DateTime ServerTimeUtc { get; set; }
    }
}
