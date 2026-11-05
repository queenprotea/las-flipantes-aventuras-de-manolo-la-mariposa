using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public sealed class MatchTable
    {
        public const int MinimumSeatCount = 2;
        public const int MaximumSeatCount = 4;
        public const int FirstSeatNumber = 1;
        public const int FirstRoundNumber = 1;
        public const int FirstTurnNumber = 1;

        private const string NoPlacementOpenMessage = "There is no placement to expire once the turns have started.";

        private readonly Board _board;
        private readonly List<Caterpillar> _caterpillars;

        public MatchTable(StartingLayout startingLayout, int seatCount)
        {
            ArgumentNullException.ThrowIfNull(startingLayout);
            ArgumentOutOfRangeException.ThrowIfLessThan(seatCount, MinimumSeatCount);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(seatCount, MaximumSeatCount);

            _board = new Board(startingLayout.Flowers);
            _caterpillars = new List<Caterpillar>();
            SeatCount = seatCount;
            RoundNumber = FirstRoundNumber;
            TurnNumber = FirstTurnNumber;
            Phase = TablePhase.CaterpillarPlacement;
            ActiveSeatNumber = FirstSeatNumber;
        }

        private MatchTable(MatchSnapshot snapshot)
        {
            _board = new Board(snapshot.Flowers);
            _caterpillars = snapshot.Caterpillars.ToList();
            SeatCount = snapshot.SeatCount;
            RoundNumber = snapshot.RoundNumber;
            TurnNumber = snapshot.TurnNumber;
            Phase = snapshot.Phase;
            ActiveSeatNumber = snapshot.ActiveSeatNumber;
            ButterflyCell = snapshot.ButterflyCell;
        }

        public int SeatCount { get; }

        public int RoundNumber { get; }

        public int TurnNumber { get; }

        public TablePhase Phase { get; private set; }

        public int ActiveSeatNumber { get; private set; }

        public BoardCell? ButterflyCell { get; private set; }

        public IReadOnlyList<Caterpillar> Caterpillars => _caterpillars;

        public static MatchTable RestoreFromSnapshot(MatchSnapshot snapshot)
        {
            ArgumentNullException.ThrowIfNull(snapshot);
            ArgumentOutOfRangeException.ThrowIfLessThan(snapshot.SeatCount, MinimumSeatCount, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(snapshot.SeatCount, MaximumSeatCount, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfLessThan(snapshot.ActiveSeatNumber, FirstSeatNumber, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(snapshot.ActiveSeatNumber, snapshot.SeatCount, nameof(snapshot));

            var restoredTable = new MatchTable(snapshot);

            return restoredTable;
        }

        public MatchSnapshot TakeSnapshot()
        {
            var snapshot = new MatchSnapshot(Phase, RoundNumber, TurnNumber, SeatCount, ActiveSeatNumber, _board.FindFlowers(), _caterpillars.ToList(), ButterflyCell);

            return snapshot;
        }

        public IReadOnlyList<Flower> FindFlowers()
        {
            IReadOnlyList<Flower> flowers = _board.FindFlowers();

            return flowers;
        }

        public IReadOnlyList<Planter> FindPlanters()
        {
            IReadOnlyList<Planter> planters = _board.FindPlanters();

            return planters;
        }

        public IReadOnlyList<BoardCell> FindPlacementTargets()
        {
            IReadOnlyList<BoardCell> flowerCells = _board.FindFlowerCells();
            IReadOnlyList<BoardCell> placementTargets = Phase switch
            {
                TablePhase.CaterpillarPlacement => flowerCells.Where(IsFreeOfCaterpillars).ToList(),
                TablePhase.ButterflyPlacement => flowerCells,
                TablePhase.PlayerTurn => Array.Empty<BoardCell>(),
                _ => throw new ArgumentOutOfRangeException(nameof(Phase)),
            };

            return placementTargets;
        }

        public PlacementOutcome PlaceInitialCaterpillar(int seatNumber, BoardCell destinationCell)
        {
            ThrowIfSeatNumberOutOfRange(seatNumber);
            Board.ThrowIfOutside(destinationCell, nameof(destinationCell));

            PlacementOutcome placementOutcome = JudgeFlowerPlacement(TablePhase.CaterpillarPlacement, seatNumber, destinationCell);
            if ((placementOutcome == PlacementOutcome.Placed) && !IsFreeOfCaterpillars(destinationCell))
            {
                placementOutcome = PlacementOutcome.FlowerOccupied;
            }

            if (placementOutcome == PlacementOutcome.Placed)
            {
                _caterpillars.Add(new Caterpillar(seatNumber, destinationCell));
                PassCaterpillarPlacement();
            }

            return placementOutcome;
        }

        public PlacementOutcome PlaceButterfly(int seatNumber, BoardCell destinationCell)
        {
            ThrowIfSeatNumberOutOfRange(seatNumber);
            Board.ThrowIfOutside(destinationCell, nameof(destinationCell));

            PlacementOutcome placementOutcome = JudgeFlowerPlacement(TablePhase.ButterflyPlacement, seatNumber, destinationCell);
            if (placementOutcome == PlacementOutcome.Placed)
            {
                ButterflyCell = destinationCell;
                StartFirstTurn();
            }

            return placementOutcome;
        }

        public void ExpirePlacement()
        {
            switch (Phase)
            {
                case TablePhase.CaterpillarPlacement:
                    PassCaterpillarPlacement();
                    break;

                case TablePhase.ButterflyPlacement:
                    OpenButterflyPlacementFor(FindLastPlacerBefore(ActiveSeatNumber));
                    break;

                case TablePhase.PlayerTurn:
                    throw new InvalidOperationException(NoPlacementOpenMessage);

                default:
                    throw new ArgumentOutOfRangeException(nameof(Phase));
            }
        }

        private void ThrowIfSeatNumberOutOfRange(int seatNumber)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(seatNumber, FirstSeatNumber);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(seatNumber, SeatCount);
        }

        private PlacementOutcome JudgeFlowerPlacement(TablePhase expectedPhase, int seatNumber, BoardCell destinationCell)
        {
            PlacementOutcome placementOutcome = PlacementOutcome.Placed;
            if (Phase != expectedPhase)
            {
                placementOutcome = PlacementOutcome.WrongPhase;
            }
            else if (seatNumber != ActiveSeatNumber)
            {
                placementOutcome = PlacementOutcome.NotYourPlacement;
            }
            else if (!_board.HasFlower(destinationCell))
            {
                placementOutcome = PlacementOutcome.NotAFlower;
            }

            return placementOutcome;
        }

        private bool IsFreeOfCaterpillars(BoardCell flowerCell)
        {
            bool isFree = _caterpillars.All(caterpillar => caterpillar.Cell != flowerCell);

            return isFree;
        }

        private void PassCaterpillarPlacement()
        {
            if (ActiveSeatNumber == SeatCount)
            {
                OpenButterflyPlacementFor(FindLastPlacerBefore(SeatCount + 1));
            }
            else
            {
                ActiveSeatNumber++;
            }
        }

        private int? FindLastPlacerBefore(int seatNumber)
        {
            int? lastPlacerSeatNumber = _caterpillars.Where(caterpillar => caterpillar.SeatNumber < seatNumber).Max(caterpillar => (int?)caterpillar.SeatNumber);

            return lastPlacerSeatNumber;
        }

        private void OpenButterflyPlacementFor(int? placerSeatNumber)
        {
            if (placerSeatNumber is int butterflyPlacerSeatNumber)
            {
                Phase = TablePhase.ButterflyPlacement;
                ActiveSeatNumber = butterflyPlacerSeatNumber;
            }
            else
            {
                StartFirstTurn();
            }
        }

        private void StartFirstTurn()
        {
            Phase = TablePhase.PlayerTurn;
            ActiveSeatNumber = FirstSeatNumber;
        }
    }
}
