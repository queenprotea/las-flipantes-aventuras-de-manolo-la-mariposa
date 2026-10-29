using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class RoomMember
    {
        [DataMember]
        public int MemberId { get; set; }

        [DataMember]
        public string DisplayName { get; set; } = string.Empty;

        [DataMember]
        public bool IsGuest { get; set; }

        [DataMember]
        public SeatColor Color { get; set; }

        [DataMember]
        public bool IsActive { get; set; }
    }
}
