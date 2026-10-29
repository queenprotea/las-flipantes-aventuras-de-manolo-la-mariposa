using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class RoomSummary
    {
        [DataMember]
        public int RoomId { get; set; }

        [DataMember]
        public string Code { get; set; } = string.Empty;

        [DataMember]
        public RoomVisibility Visibility { get; set; }

        [DataMember]
        public string HostDisplayName { get; set; } = string.Empty;

        [DataMember]
        public int PlayerCount { get; set; }

        [DataMember]
        public int MaxPlayers { get; set; }

        [DataMember]
        public RoomState State { get; set; }
    }
}
