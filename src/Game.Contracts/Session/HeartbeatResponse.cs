using System;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class HeartbeatResponse
    {
        [DataMember]
        public DateTime ServerTimeUtc { get; set; }
    }
}
