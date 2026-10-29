using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class Room
    {
        [DataMember]
        public int RoomId { get; set; }

        [DataMember]
        public string Code { get; set; } = string.Empty;

        [DataMember]
        public RoomVisibility Visibility { get; set; }

        [DataMember]
        public RoomState State { get; set; }

        [DataMember]
        public int MaxPlayers { get; set; }

        [DataMember]
        public int HostMemberId { get; set; }

        [DataMember]
        public int ViewerMemberId { get; set; }

        [DataMember]
        public List<RoomMember> Members { get; set; } = new List<RoomMember>();
    }
}
