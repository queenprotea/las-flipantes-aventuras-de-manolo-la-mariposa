using System.Collections.Generic;

namespace Game.Domain
{
    public sealed record MatchSnapshot
    {
        public MatchSnapshot(TablePhase phase, int roundNumber, int turnRoundNumber, int seatCount, int activeSeatNumber, int actionPointsLeft, int buildsLeft, IReadOnlyList<Flower> flowers, IReadOnlyList<Caterpillar> caterpillars, BoardCell? butterflyCell, IReadOnlyList<SeatStanding> standings, IReadOnlyList<int> pendingButterflySeatNumbers, FinishCause? finishCause)
        {
            Phase = phase;
            RoundNumber = roundNumber;
            TurnRoundNumber = turnRoundNumber;
            SeatCount = seatCount;
            ActiveSeatNumber = activeSeatNumber;
            ActionPointsLeft = actionPointsLeft;
            BuildsLeft = buildsLeft;
            Flowers = flowers;
            Caterpillars = caterpillars;
            ButterflyCell = butterflyCell;
            Standings = standings;
            PendingButterflySeatNumbers = pendingButterflySeatNumbers;
            FinishCause = finishCause;
        }

        public TablePhase Phase { get; }

        public int RoundNumber { get; }

        public int TurnRoundNumber { get; }

        public int SeatCount { get; }

        public int ActiveSeatNumber { get; }

        public int ActionPointsLeft { get; }

        public int BuildsLeft { get; }

        public IReadOnlyList<Flower> Flowers { get; }

        public IReadOnlyList<Caterpillar> Caterpillars { get; }

        public BoardCell? ButterflyCell { get; }

        public IReadOnlyList<SeatStanding> Standings { get; }

        public IReadOnlyList<int> PendingButterflySeatNumbers { get; }

        public FinishCause? FinishCause { get; }
    }
}
