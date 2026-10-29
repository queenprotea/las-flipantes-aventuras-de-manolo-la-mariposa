using System;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class CreateRoomResult
    {
        [DataMember]
        public CreateRoomStatus Status { get; set; }

        [DataMember]
        public Room? Room { get; set; }

        [DataMember]
        public DateTime? BanEndsAtUtc { get; set; }
    }
}
