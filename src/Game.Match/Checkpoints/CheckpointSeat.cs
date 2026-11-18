using System.Runtime.Serialization;

using Game.Contracts;

namespace Game.Match
{
    [DataContract]
    internal sealed class CheckpointSeat
    {
        [DataMember(IsRequired = true)]
        public int SeatNumber { get; set; }

        [DataMember(IsRequired = true)]
        public int EntrantId { get; set; }

        [DataMember(IsRequired = true)]
        public int MemberId { get; set; }

        [DataMember(IsRequired = true)]
        public string? DisplayName { get; set; }

        [DataMember(IsRequired = true)]
        public bool IsGuest { get; set; }

        [DataMember(IsRequired = true)]
        public SeatColor Color { get; set; }

        [DataMember(IsRequired = true)]
        public bool IsRetired { get; set; }
    }
}
