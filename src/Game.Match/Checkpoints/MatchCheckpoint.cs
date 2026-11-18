using System.Collections.Generic;
using System.Runtime.Serialization;

using Game.Domain;

namespace Game.Match
{
    /// <summary>
    /// The state of a match at the start of its current step, with its seats, as it is saved so that the match can go on after a server restart.
    /// </summary>
    [DataContract]
    internal sealed class MatchCheckpoint
    {
        [DataMember(IsRequired = true)]
        public string? RoomCode { get; set; }

        [DataMember(IsRequired = true)]
        public List<CheckpointSeat>? Seats { get; set; }

        [DataMember(IsRequired = true)]
        public TablePhase Phase { get; set; }

        [DataMember(IsRequired = true)]
        public int RoundNumber { get; set; }

        [DataMember(IsRequired = true)]
        public int TurnRoundNumber { get; set; }

        [DataMember(IsRequired = true)]
        public int ActiveSeatNumber { get; set; }

        [DataMember(IsRequired = true)]
        public int ActionPointsLeft { get; set; }

        [DataMember(IsRequired = true)]
        public int BuildsLeft { get; set; }

        [DataMember(IsRequired = true)]
        public List<CheckpointFlower>? Flowers { get; set; }

        [DataMember(IsRequired = true)]
        public List<CheckpointCaterpillar>? Caterpillars { get; set; }

        [DataMember]
        public CheckpointCell? ButterflyCell { get; set; }

        [DataMember(IsRequired = true)]
        public List<CheckpointStanding>? Standings { get; set; }

        [DataMember(IsRequired = true)]
        public List<CheckpointSeatCards>? SeatCards { get; set; }

        [DataMember(IsRequired = true)]
        public List<int>? PendingButterflySeatNumbers { get; set; }
    }
}
