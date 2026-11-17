using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes a match as one of its players sees it: the phase, who plays and for how long, how long a seat that loses the connection during the turn of another player is kept, what the active player has left, the seats with their scores, the board, the legal targets, the viewer's own action cards and the final ranking. The viewer sees the cards in its own hand, which of them it can use now, whether it can draw and the flowers that can lose a level; of the other seats it only sees how many cards they hold.
    /// </summary>
    [DataContract]
    public sealed class MatchView
    {
        [DataMember]
        public MatchPhase Phase { get; set; }

        [DataMember]
        public int RoundNumber { get; set; }

        [DataMember]
        public int TurnRoundNumber { get; set; }

        [DataMember]
        public int RoundTurnCount { get; set; }

        [DataMember]
        public int RoundButterflyPoints { get; set; }

        [DataMember]
        public int ViewerSeatNumber { get; set; }

        [DataMember]
        public int ActiveSeatNumber { get; set; }

        [DataMember]
        public TimeSpan? StepTimeLeft { get; set; }

        [DataMember]
        public TimeSpan ReconnectionTime { get; set; }

        [DataMember]
        public int ActionPointsLeft { get; set; }

        [DataMember]
        public int BuildsLeft { get; set; }

        [DataMember]
        public int CaterpillarsPerSeat { get; set; }

        [DataMember]
        public MatchActionCosts ActionCosts { get; set; } = new MatchActionCosts();

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

        [DataMember]
        public List<MatchMoveTarget> MoveTargets { get; set; } = new List<MatchMoveTarget>();

        [DataMember]
        public List<MatchCell> BuildTargets { get; set; } = new List<MatchCell>();

        [DataMember]
        public List<int> RankedSeatNumbers { get; set; } = new List<int>();

        [DataMember]
        public bool WasAbandoned { get; set; }

        [DataMember]
        public List<MatchCard> Hand { get; set; } = new List<MatchCard>();

        [DataMember]
        public int DeckCount { get; set; }

        [DataMember]
        public List<MatchCard> PlayableCards { get; set; } = new List<MatchCard>();

        [DataMember]
        public bool CanDrawCard { get; set; }

        [DataMember]
        public List<MatchCell> LevelRemovalTargets { get; set; } = new List<MatchCell>();
    }
}
