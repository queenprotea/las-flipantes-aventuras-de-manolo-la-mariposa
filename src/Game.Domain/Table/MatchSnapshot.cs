using System.Collections.Generic;

namespace Game.Domain
{
    public sealed record MatchSnapshot
    {
        public MatchSnapshot(TablePhase phase, int roundNumber, int turnNumber, int seatCount, int activeSeatNumber, int actionPointsLeft, int buildsLeft, IReadOnlyList<Flower> flowers, IReadOnlyList<Caterpillar> caterpillars, BoardCell? butterflyCell)
        {
            Phase = phase;
            RoundNumber = roundNumber;
            TurnNumber = turnNumber;
            SeatCount = seatCount;
            ActiveSeatNumber = activeSeatNumber;
            ActionPointsLeft = actionPointsLeft;
            BuildsLeft = buildsLeft;
            Flowers = flowers;
            Caterpillars = caterpillars;
            ButterflyCell = butterflyCell;
        }

        public TablePhase Phase { get; }

        public int RoundNumber { get; }

        public int TurnNumber { get; }

        public int SeatCount { get; }

        public int ActiveSeatNumber { get; }

        public int ActionPointsLeft { get; }

        public int BuildsLeft { get; }

        public IReadOnlyList<Flower> Flowers { get; }

        public IReadOnlyList<Caterpillar> Caterpillars { get; }

        public BoardCell? ButterflyCell { get; }
    }
}
