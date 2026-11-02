using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes a member of a room, with their seat color, guest mark and connection state.
    /// </summary>
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
