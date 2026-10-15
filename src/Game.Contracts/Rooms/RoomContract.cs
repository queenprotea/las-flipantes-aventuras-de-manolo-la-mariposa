using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class RoomContract
    {
        [DataMember]
        public int RoomId { get; set; }

        [DataMember]
        public int HostPlayerId { get; set; }

        [DataMember]
        public int MaxPlayers { get; set; }

        [DataMember]
        public RoomState State { get; set; }

        [DataMember]
        public List<PlayerSummaryContract> Members { get; set; } = new List<PlayerSummaryContract>();
    }
}
