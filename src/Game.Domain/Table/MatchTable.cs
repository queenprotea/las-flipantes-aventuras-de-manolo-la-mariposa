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
        public const int ActionPointsPerTurn = 5;
        public const int CaterpillarsPerSeat = 5;

        private const int MaximumLevelsClimbedPerMove = 1;
        private const int SinglePlanterCount = 1;
        private const string NoPlacementOpenMessage = "There is no placement to expire outside the placements.";
        private const string NoTurnOpenMessage = "There is no turn to expire outside the players' turns.";

        private readonly Board _board;
        private readonly List<Caterpillar> _caterpillars;
        private readonly MatchRules _rules;

        public MatchTable(StartingLayout startingLayout, int seatCount, MatchRules rules)
        {
            ArgumentNullException.ThrowIfNull(startingLayout);
            ArgumentOutOfRangeException.ThrowIfLessThan(seatCount, MinimumSeatCount);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(seatCount, MaximumSeatCount);
            ArgumentNullException.ThrowIfNull(rules);

            _board = new Board(startingLayout.Flowers);
            _caterpillars = new List<Caterpillar>();
            _rules = rules;
            SeatCount = seatCount;
            RoundNumber = FirstRoundNumber;
            TurnNumber = FirstTurnNumber;
            Phase = TablePhase.CaterpillarPlacement;
            ActiveSeatNumber = FirstSeatNumber;
        }

        private MatchTable(MatchSnapshot snapshot, MatchRules rules)
        {
            _board = new Board(snapshot.Flowers);
            _caterpillars = snapshot.Caterpillars.ToList();
            _rules = rules;
            SeatCount = snapshot.SeatCount;
            RoundNumber = snapshot.RoundNumber;
            TurnNumber = snapshot.TurnNumber;
            Phase = snapshot.Phase;
            ActiveSeatNumber = snapshot.ActiveSeatNumber;
            ActionPointsLeft = snapshot.ActionPointsLeft;
            BuildsLeft = snapshot.BuildsLeft;
            ButterflyCell = snapshot.ButterflyCell;
        }

        public int SeatCount { get; }

        public int RoundNumber { get; private set; }

        public int TurnNumber { get; private set; }

        public TablePhase Phase { get; private set; }

        public int ActiveSeatNumber { get; private set; }

        public int ActionPointsLeft { get; private set; }

        public int BuildsLeft { get; private set; }

        public BoardCell? ButterflyCell { get; private set; }

        public IReadOnlyList<Caterpillar> Caterpillars => _caterpillars;

        public static MatchTable RestoreFromSnapshot(MatchSnapshot snapshot, MatchRules rules)
        {
            ArgumentNullException.ThrowIfNull(snapshot);
            ArgumentNullException.ThrowIfNull(rules);
            ArgumentOutOfRangeException.ThrowIfLessThan(snapshot.SeatCount, MinimumSeatCount, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(snapshot.SeatCount, MaximumSeatCount, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfLessThan(snapshot.ActiveSeatNumber, FirstSeatNumber, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(snapshot.ActiveSeatNumber, snapshot.SeatCount, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfLessThan(snapshot.RoundNumber, FirstRoundNumber, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(snapshot.RoundNumber, MatchRules.RoundCount, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfNegative(snapshot.ActionPointsLeft, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfNegative(snapshot.BuildsLeft, nameof(snapshot));

            var restoredTable = new MatchTable(snapshot, rules);

            return restoredTable;
        }

        public MatchSnapshot TakeSnapshot()
        {
            var snapshot = new MatchSnapshot(Phase, RoundNumber, TurnNumber, SeatCount, ActiveSeatNumber, ActionPointsLeft, BuildsLeft, _board.FindFlowers(), _caterpillars.ToList(), ButterflyCell);

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
                TablePhase.PlayerTurn => FindTurnPlacementTargets(),
                TablePhase.Finished => Array.Empty<BoardCell>(),
                _ => throw new ArgumentOutOfRangeException(nameof(Phase)),
            };

            return placementTargets;
        }

        public IReadOnlyList<MoveTarget> FindMoveTargets(BoardCell originCell)
        {
            Board.ThrowIfOutside(originCell, nameof(originCell));

            var moveTargets = new List<MoveTarget>();
            if ((Phase == TablePhase.PlayerTurn) && HasCaterpillarOf(ActiveSeatNumber, originCell))
            {
                List<CaterpillarMove> legalMoves = Board.FindNeighborCells(originCell).Select(neighborCell => new CaterpillarMove(originCell, neighborCell)).Where(IsLegalMoveOfActiveSeat).ToList();
                moveTargets.AddRange(legalMoves.Select(ToMoveTarget));
            }

            return moveTargets;
        }

        public IReadOnlyList<BoardCell> FindBuildTargets()
        {
            IReadOnlyList<BoardCell> buildTargets = Array.Empty<BoardCell>();
            bool canBuild = (Phase == TablePhase.PlayerTurn) && (JudgeBuildAllowance() == TurnActionOutcome.Applied);
            if (canBuild)
            {
                buildTargets = Board.AllCells.Where(IsBuildableCell).ToList();
            }

            return buildTargets;
        }

        public int CountCaterpillarsLeft(int seatNumber)
        {
            ThrowIfSeatNumberOutOfRange(seatNumber);

            int placedCount = _caterpillars.Count(caterpillar => caterpillar.SeatNumber == seatNumber);
            int caterpillarsLeft = CaterpillarsPerSeat - placedCount;

            return caterpillarsLeft;
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

        public TurnActionOutcome PlaceCaterpillar(int seatNumber, BoardCell destinationCell)
        {
            ThrowIfSeatNumberOutOfRange(seatNumber);
            Board.ThrowIfOutside(destinationCell, nameof(destinationCell));

            TurnActionOutcome actionOutcome = JudgeTurnOwner(seatNumber);
            if (actionOutcome == TurnActionOutcome.Applied)
            {
                actionOutcome = JudgeCaterpillarPlacement(seatNumber, destinationCell);
            }

            if (actionOutcome == TurnActionOutcome.Applied)
            {
                _caterpillars.Add(new Caterpillar(seatNumber, destinationCell));
                ActionPointsLeft -= _rules.ActionCosts.CaterpillarPlacement;
            }

            return actionOutcome;
        }

        public TurnActionOutcome MoveCaterpillar(int seatNumber, CaterpillarMove move)
        {
            ThrowIfSeatNumberOutOfRange(seatNumber);
            Board.ThrowIfOutside(move.Origin, nameof(move));
            Board.ThrowIfOutside(move.Destination, nameof(move));

            TurnActionOutcome actionOutcome = JudgeTurnOwner(seatNumber);
            if (actionOutcome == TurnActionOutcome.Applied)
            {
                actionOutcome = JudgeMove(seatNumber, move);
            }

            if (actionOutcome == TurnActionOutcome.Applied)
            {
                ActionPointsLeft -= CalculateMoveCost(move);
                int caterpillarIndex = _caterpillars.FindIndex(caterpillar => caterpillar.Cell == move.Origin);
                _caterpillars[caterpillarIndex] = new Caterpillar(seatNumber, move.Destination);
            }

            return actionOutcome;
        }

        public TurnActionOutcome AddFlowerLevel(int seatNumber, BoardCell buildCell)
        {
            ThrowIfSeatNumberOutOfRange(seatNumber);
            Board.ThrowIfOutside(buildCell, nameof(buildCell));

            TurnActionOutcome actionOutcome = JudgeTurnOwner(seatNumber);
            if (actionOutcome == TurnActionOutcome.Applied)
            {
                actionOutcome = JudgeBuildAllowance();
            }

            if (actionOutcome == TurnActionOutcome.Applied)
            {
                actionOutcome = JudgeBuildCell(buildCell);
            }

            if (actionOutcome == TurnActionOutcome.Applied)
            {
                _board.AddFlowerLevel(buildCell);
                ActionPointsLeft -= _rules.ActionCosts.FlowerLevelAddition;
                BuildsLeft--;
            }

            return actionOutcome;
        }

        public TurnActionOutcome EndTurn(int seatNumber)
        {
            ThrowIfSeatNumberOutOfRange(seatNumber);

            TurnActionOutcome actionOutcome = JudgeTurnOwner(seatNumber);
            if (actionOutcome == TurnActionOutcome.Applied)
            {
                PassTurn();
            }

            return actionOutcome;
        }

        public void ExpireTurn()
        {
            if (Phase != TablePhase.PlayerTurn)
            {
                throw new InvalidOperationException(NoTurnOpenMessage);
            }

            PassTurn();
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
                case TablePhase.Finished:
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

        private bool IsFreeOfCaterpillars(BoardCell examinedCell)
        {
            bool isFree = _caterpillars.All(caterpillar => caterpillar.Cell != examinedCell);

            return isFree;
        }

        private bool HasCaterpillarOf(int seatNumber, BoardCell examinedCell)
        {
            bool hasCaterpillar = _caterpillars.Contains(new Caterpillar(seatNumber, examinedCell));

            return hasCaterpillar;
        }

        private IReadOnlyList<BoardCell> FindTurnPlacementTargets()
        {
            IReadOnlyList<BoardCell> placementTargets = Array.Empty<BoardCell>();
            bool canPlace = (CountCaterpillarsLeft(ActiveSeatNumber) > 0) && (ActionPointsLeft >= _rules.ActionCosts.CaterpillarPlacement);
            if (canPlace)
            {
                placementTargets = _board.FindFlowerCells().Where(IsFreeOfCaterpillars).ToList();
            }

            return placementTargets;
        }

        private TurnActionOutcome JudgeTurnOwner(int seatNumber)
        {
            TurnActionOutcome actionOutcome = TurnActionOutcome.Applied;
            if (Phase != TablePhase.PlayerTurn)
            {
                actionOutcome = TurnActionOutcome.WrongPhase;
            }
            else if (seatNumber != ActiveSeatNumber)
            {
                actionOutcome = TurnActionOutcome.NotYourTurn;
            }

            return actionOutcome;
        }

        private TurnActionOutcome JudgeCaterpillarPlacement(int seatNumber, BoardCell destinationCell)
        {
            TurnActionOutcome actionOutcome = TurnActionOutcome.Applied;
            if (CountCaterpillarsLeft(seatNumber) == 0)
            {
                actionOutcome = TurnActionOutcome.NoCaterpillarsLeft;
            }
            else if (!_board.HasFlower(destinationCell))
            {
                actionOutcome = TurnActionOutcome.NotAFlower;
            }
            else if (!IsFreeOfCaterpillars(destinationCell))
            {
                actionOutcome = TurnActionOutcome.CellOccupied;
            }
            else if (ActionPointsLeft < _rules.ActionCosts.CaterpillarPlacement)
            {
                actionOutcome = TurnActionOutcome.NotEnoughActionPoints;
            }

            return actionOutcome;
        }

        private TurnActionOutcome JudgeMove(int seatNumber, CaterpillarMove move)
        {
            TurnActionOutcome actionOutcome = TurnActionOutcome.Applied;
            if (!HasCaterpillarOf(seatNumber, move.Origin))
            {
                actionOutcome = TurnActionOutcome.NotYourCaterpillar;
            }
            else if (!Board.FindNeighborCells(move.Origin).Contains(move.Destination))
            {
                actionOutcome = TurnActionOutcome.NotNeighborCell;
            }
            else if (!IsFreeOfCaterpillars(move.Destination))
            {
                actionOutcome = TurnActionOutcome.CellOccupied;
            }
            else if (CalculateLevelChange(move) > MaximumLevelsClimbedPerMove)
            {
                actionOutcome = TurnActionOutcome.ClimbTooHigh;
            }
            else if (ActionPointsLeft < CalculateMoveCost(move))
            {
                actionOutcome = TurnActionOutcome.NotEnoughActionPoints;
            }

            return actionOutcome;
        }

        private bool IsLegalMoveOfActiveSeat(CaterpillarMove move)
        {
            bool isLegal = JudgeMove(ActiveSeatNumber, move) == TurnActionOutcome.Applied;

            return isLegal;
        }

        private MoveTarget ToMoveTarget(CaterpillarMove move)
        {
            var moveTarget = new MoveTarget(move.Destination, CalculateMoveCost(move));

            return moveTarget;
        }

        private int CalculateLevelChange(CaterpillarMove move)
        {
            int levelChange = _board.GetFlowerLevel(move.Destination) - _board.GetFlowerLevel(move.Origin);

            return levelChange;
        }

        private int CalculateMoveCost(CaterpillarMove move)
        {
            int levelsClimbed = Math.Max(0, CalculateLevelChange(move));
            int moveCost = _rules.ActionCosts.Movement + (levelsClimbed * _rules.ActionCosts.LevelClimb);

            return moveCost;
        }

        private TurnActionOutcome JudgeBuildAllowance()
        {
            TurnActionOutcome actionOutcome = TurnActionOutcome.Applied;
            if (BuildsLeft == 0)
            {
                actionOutcome = TurnActionOutcome.NoBuildsLeft;
            }
            else if (ActionPointsLeft < _rules.ActionCosts.FlowerLevelAddition)
            {
                actionOutcome = TurnActionOutcome.NotEnoughActionPoints;
            }

            return actionOutcome;
        }

        private TurnActionOutcome JudgeBuildCell(BoardCell buildCell)
        {
            TurnActionOutcome actionOutcome = TurnActionOutcome.Applied;
            if (!IsFreeOfCaterpillars(buildCell))
            {
                actionOutcome = TurnActionOutcome.CellOccupied;
            }
            else if (_board.GetFlowerLevel(buildCell) == Board.MaximumFlowerLevel)
            {
                actionOutcome = TurnActionOutcome.FlowerAtMaximumLevel;
            }
            else if (!_board.HasFlower(buildCell))
            {
                actionOutcome = JudgeNewFlower(buildCell);
            }

            return actionOutcome;
        }

        private TurnActionOutcome JudgeNewFlower(BoardCell groundCell)
        {
            int plantersAround = _board.CountPlantersAround(groundCell);
            TurnActionOutcome actionOutcome = TurnActionOutcome.Applied;
            if (plantersAround == 0)
            {
                actionOutcome = TurnActionOutcome.NotNextToPlanter;
            }
            else if (plantersAround > SinglePlanterCount)
            {
                actionOutcome = TurnActionOutcome.JoinsPlanters;
            }

            return actionOutcome;
        }

        private bool IsBuildableCell(BoardCell examinedCell)
        {
            bool isBuildable = JudgeBuildCell(examinedCell) == TurnActionOutcome.Applied;

            return isBuildable;
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
            BeginTurnOf(FirstSeatNumber);
        }

        private void PassTurn()
        {
            if (ActiveSeatNumber < SeatCount)
            {
                BeginTurnOf(ActiveSeatNumber + 1);
            }
            else if (TurnNumber < _rules.GetTurnCount(RoundNumber))
            {
                TurnNumber++;
                BeginTurnOf(FirstSeatNumber);
            }
            else if (RoundNumber < MatchRules.RoundCount)
            {
                RoundNumber++;
                TurnNumber = FirstTurnNumber;
                BeginTurnOf(FirstSeatNumber);
            }
            else
            {
                FinishMatch();
            }
        }

        private void BeginTurnOf(int seatNumber)
        {
            Phase = TablePhase.PlayerTurn;
            ActiveSeatNumber = seatNumber;
            ActionPointsLeft = ActionPointsPerTurn;
            BuildsLeft = _rules.GetBuildCount(RoundNumber, TurnNumber);
        }

        private void FinishMatch()
        {
            Phase = TablePhase.Finished;
            ActionPointsLeft = 0;
            BuildsLeft = 0;
        }
    }
}
