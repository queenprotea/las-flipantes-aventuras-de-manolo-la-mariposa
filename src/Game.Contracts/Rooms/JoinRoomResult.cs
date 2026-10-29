using System;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class JoinRoomResult
    {
        [DataMember]
        public JoinRoomStatus Status { get; set; }

        [DataMember]
        public Room? Room { get; set; }

        [DataMember]
        public DateTime? BanEndsAtUtc { get; set; }
    }
}
