using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes a caterpillar on the board: the seat it belongs to and its cell.
    /// </summary>
    [DataContract]
    public sealed class MatchCaterpillar
    {
        [DataMember]
        public int SeatNumber { get; set; }

        [DataMember]
        public MatchCell Cell { get; set; } = new MatchCell();
    }
}
