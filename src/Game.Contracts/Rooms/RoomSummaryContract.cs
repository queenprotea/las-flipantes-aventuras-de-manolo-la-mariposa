using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class RoomSummaryContract
    {
        [DataMember]
        public int RoomId { get; set; }

        [DataMember]
        public PlayerSummaryContract Host { get; set; } = new PlayerSummaryContract();

        [DataMember]
        public int PlayerCount { get; set; }

        [DataMember]
        public int MaxPlayers { get; set; }

        [DataMember]
        public RoomState State { get; set; }
    }
}
