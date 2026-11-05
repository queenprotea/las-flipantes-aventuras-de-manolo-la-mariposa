using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes a match as one of its players sees it: the phase, who places and for how long, the seats, the board and the cells where the current piece may go.
    /// </summary>
    [DataContract]
    public sealed class MatchView
    {
        [DataMember]
        public MatchPhase Phase { get; set; }

        [DataMember]
        public int RoundNumber { get; set; }

        [DataMember]
        public int TurnNumber { get; set; }

        [DataMember]
        public int ViewerSeatNumber { get; set; }

        [DataMember]
        public int ActiveSeatNumber { get; set; }

        [DataMember]
        public TimeSpan? PlacementTimeLeft { get; set; }

        [DataMember]
        public List<MatchSeat> Seats { get; set; } = new List<MatchSeat>();

        [DataMember]
        public List<MatchFlower> Flowers { get; set; } = new List<MatchFlower>();

        [DataMember]
        public List<MatchCaterpillar> Caterpillars { get; set; } = new List<MatchCaterpillar>();

        [DataMember]
        public bool HasButterfly { get; set; }

        [DataMember]
        public MatchCell ButterflyCell { get; set; } = new MatchCell();

        [DataMember]
        public List<MatchCell> PlacementTargets { get; set; } = new List<MatchCell>();
    }
}
