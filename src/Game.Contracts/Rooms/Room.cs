using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes a room as one of its members sees it, with its members, its host and the member who is looking.
    /// </summary>
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
