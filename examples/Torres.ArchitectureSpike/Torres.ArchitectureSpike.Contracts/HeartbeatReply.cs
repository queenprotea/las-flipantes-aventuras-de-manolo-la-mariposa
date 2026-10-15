using System;
using System.Runtime.Serialization;

namespace Torres.ArchitectureSpike.Contracts
{
    [DataContract]
    public sealed class HeartbeatReply
    {
        [DataMember]
        public DateTime ServerTimeUtc { get; set; }
    }
}
