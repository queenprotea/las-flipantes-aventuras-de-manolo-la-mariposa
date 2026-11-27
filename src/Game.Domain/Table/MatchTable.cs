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
        public const int FirstTurnRoundNumber = 1;
        public const int ActionPointsPerTurn = 5;
        public const int CaterpillarsPerSeat = 5;
        internal const int MaximumLevelsClimbedPerMove = 1;

        private const int SinglePlanterCount = 1;
        private const string NoPlacementOpenMessage = "There is no placement to expire outside the placements.";
        private const string NoTurnOpenMessage = "There is no turn to expire outside the players' turns.";
        private const string StandingCountMessage = "The snapshot must keep one standing per seat.";
        private const string SeatCardsCountMessage = "The snapshot must keep the cards of every seat.";
        private const int NoCardPoints = 0;

        private readonly Board _board;
        private readonly List<Caterpillar> _caterpillars;
        private readonly MatchRules _rules;
        private readonly Random _chanceSource;
        private readonly List<SeatStanding> _standings;
        private readonly List<int> _pendingButterflySeatNumbers;

        public MatchTable(StartingLayout startingLayout, int seatCount, MatchRules rules, Random chanceSource)
        {
            _board = new Board(startingLayout.Flowers);
            _caterpillars = new List<Caterpillar>();
            _rules = rules;
            _chanceSource = chanceSource;
            _standings = CreateFreshStandings(seatCount);
            _pendingButterflySeatNumbers = new List<int>();
            CardTable = new ActionCardTable(this, _board, Array.Empty<SeatCards>(), seatCount, chanceSource);
            SeatCount = seatCount;
            RoundNumber = FirstRoundNumber;
            TurnRoundNumber = FirstTurnRoundNumber;
            Phase = TablePhase.CaterpillarPlacement;
            ActiveSeatNumber = FirstSeatNumber;
        }

        private MatchTable(MatchSnapshot snapshot, MatchRules rules, Random chanceSource)
        {
            _board = new Board(snapshot.Flowers);
            _caterpillars = snapshot.Caterpillars.ToList();
            _rules = rules;
            _chanceSource = chanceSource;
            _standings = snapshot.Standings.Count == 0 ? CreateFreshStandings(snapshot.SeatCount) : snapshot.Standings.ToList();
            _pendingButterflySeatNumbers = snapshot.PendingButterflySeatNumbers.ToList();
            CardTable = new ActionCardTable(this, _board, snapshot.SeatCards, snapshot.SeatCount, chanceSource);
            FinishCause = snapshot.FinishCause;
            SeatCount = snapshot.SeatCount;
            RoundNumber = snapshot.RoundNumber;
            TurnRoundNumber = snapshot.TurnRoundNumber;
            Phase = snapshot.Phase;
            ActiveSeatNumber = snapshot.ActiveSeatNumber;
            ActionPointsLeft = snapshot.ActionPointsLeft;
            BuildsLeft = snapshot.BuildsLeft;
            ButterflyCell = snapshot.ButterflyCell;
        }

        public int SeatCount { get; }

        public int RoundNumber { get; private set; }

        public int TurnRoundNumber { get; private set; }

        public TablePhase Phase { get; private set; }

        public int ActiveSeatNumber { get; private set; }

        public int ActionPointsLeft { get; private set; }

        public int BuildsLeft { get; private set; }

        public BoardCell? ButterflyCell { get; private set; }

        public FinishCause? FinishCause { get; private set; }

        public int RoundTurnCount => _rules.GetTurnCount(RoundNumber);

        public ActionCosts ActionCosts => _rules.ActionCosts;

        public IReadOnlyList<Caterpillar> Caterpillars => _caterpillars;

        public IReadOnlyList<SeatStanding> Standings => _standings;

        public ActionCardTable CardTable { get; }

        public static MatchTable RestoreFromSnapshot(MatchSnapshot snapshot, MatchRules rules, Random chanceSource)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(snapshot.SeatCount, MinimumSeatCount, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(snapshot.SeatCount, MaximumSeatCount, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfLessThan(snapshot.ActiveSeatNumber, FirstSeatNumber, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(snapshot.ActiveSeatNumber, snapshot.SeatCount, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfLessThan(snapshot.RoundNumber, FirstRoundNumber, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(snapshot.RoundNumber, MatchRules.RoundCount, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfNegative(snapshot.ActionPointsLeft, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfNegative(snapshot.BuildsLeft, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfLessThan(snapshot.TurnRoundNumber, FirstTurnRoundNumber, nameof(snapshot));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(snapshot.TurnRoundNumber, rules.GetTurnCount(snapshot.RoundNumber), nameof(snapshot));
            foreach (Caterpillar restoredCaterpillar in snapshot.Caterpillars)
            {
                Board.ThrowIfOutside(restoredCaterpillar.Cell, nameof(snapshot));
            }

            bool hasStandingPerSeat = (snapshot.Standings.Count == 0) || (snapshot.Standings.Count == snapshot.SeatCount);
            if (!hasStandingPerSeat)
            {
                throw new ArgumentException(StandingCountMessage, nameof(snapshot));
            }

            bool hasCardsPerSeat = (snapshot.SeatCards.Count == 0) || (snapshot.SeatCards.Count == snapshot.SeatCount);
            if (!hasCardsPerSeat)
            {
                throw new ArgumentException(SeatCardsCountMessage, nameof(snapshot));
            }

            var restoredTable = new MatchTable(snapshot, rules, chanceSource);

            return restoredTable;
        }

        public MatchSnapshot TakeSnapshot()
        {
            var snapshot = new MatchSnapshot(Phase, RoundNumber, TurnRoundNumber, SeatCount, ActiveSeatNumber, ActionPointsLeft, BuildsLeft, _board.FindFlowers(), _caterpillars.ToList(), ButterflyCell, _standings.ToList(), CardTable.SeatCards.ToList(), _pendingButterflySeatNumbers.ToList(), FinishCause);

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

        public IReadOnlyList<SeatStanding> FindRanking()
        {
            List<SeatStanding> ranking = FindFinishingOrder().Where(standing => !standing.IsRetired).ToList();

            return ranking;
        }

        public IReadOnlyList<SeatStanding> FindFinishingOrder()
        {
            List<SeatStanding> finishingOrder = _standings.OrderBy(standing => standing.IsRetired).ThenByDescending(standing => standing.FinalScore).ThenByDescending(standing => standing.ScoredPlanterCount).ThenBy(standing => standing.SeatNumber).ToList();

            return finishingOrder;
        }

        public bool IsSeatRetired(int seatNumber)
        {
            bool isRetired = _standings[seatNumber - FirstSeatNumber].IsRetired;

            return isRetired;
        }

        public int CountCaterpillarsLeft(int seatNumber)
        {
            int placedCount = _caterpillars.Count(caterpillar => caterpillar.SeatNumber == seatNumber);
            int caterpillarsLeft = CaterpillarsPerSeat - placedCount;

            return caterpillarsLeft;
        }

        public PlacementOutcome PlaceInitialCaterpillar(int seatNumber, BoardCell destinationCell)
        {
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
            PlacementOutcome placementOutcome = JudgeFlowerPlacement(TablePhase.ButterflyPlacement, seatNumber, destinationCell);
            if (placementOutcome == PlacementOutcome.Placed)
            {
                ButterflyCell = destinationCell;
                _pendingButterflySeatNumbers.Clear();
                StartRoundTurns();
            }

            return placementOutcome;
        }

        public TurnActionOutcome PlaceCaterpillar(int seatNumber, BoardCell destinationCell)
        {
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
            TurnActionOutcome actionOutcome = JudgeTurnOwner(seatNumber);
            if (actionOutcome == TurnActionOutcome.Applied)
            {
                actionOutcome = JudgeMove(seatNumber, move);
            }

            if (actionOutcome == TurnActionOutcome.Applied)
            {
                ActionPointsLeft -= CalculateMoveCost(move);
                RelocateCaterpillar(seatNumber, move.Origin, move.Destination);
            }

            return actionOutcome;
        }

        public TurnActionOutcome AddFlowerLevel(int seatNumber, BoardCell buildCell)
        {
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
                    PassButterflyPlacement();
                    break;

                case TablePhase.PlayerTurn:
                case TablePhase.Finished:
                    throw new InvalidOperationException(NoPlacementOpenMessage);

                default:
                    throw new ArgumentOutOfRangeException(nameof(Phase));
            }
        }

        public void RetireSeat(int seatNumber)
        {
            bool canRetire = (Phase != TablePhase.Finished) && !IsSeatRetired(seatNumber);
            if (!canRetire)
            {
                return;
            }

            int standingIndex = seatNumber - FirstSeatNumber;
            _standings[standingIndex] = _standings[standingIndex] with { IsRetired = true };
            _caterpillars.RemoveAll(caterpillar => caterpillar.SeatNumber == seatNumber);
            _pendingButterflySeatNumbers.Remove(seatNumber);
            if (CountActiveSeats() < MinimumSeatCount)
            {
                FinishMatch(Domain.FinishCause.TooFewSeats);
            }
            else if (seatNumber == ActiveSeatNumber)
            {
                PassRetiredSeat();
            }
        }

        internal void SpendActionPoints(int actionPoints)
        {
            ActionPointsLeft -= actionPoints;
        }

        internal void GrantActionPoints(int actionPoints)
        {
            ActionPointsLeft += actionPoints;
        }

        internal void RelocateCaterpillar(int seatNumber, BoardCell originCell, BoardCell destinationCell)
        {
            int caterpillarIndex = _caterpillars.FindIndex(caterpillar => caterpillar.Cell == originCell);
            _caterpillars[caterpillarIndex] = new Caterpillar(seatNumber, destinationCell);
        }

        internal void AddCardPoints(int seatNumber, int cardPoints)
        {
            int standingIndex = seatNumber - FirstSeatNumber;
            _standings[standingIndex] = _standings[standingIndex] with { CardPoints = _standings[standingIndex].CardPoints + cardPoints };
        }

        private static List<SeatStanding> CreateFreshStandings(int seatCount)
        {
            List<SeatStanding> freshStandings = Enumerable.Range(FirstSeatNumber, seatCount).Select(CreateFreshStanding).ToList();

            return freshStandings;
        }

        private static SeatStanding CreateFreshStanding(int seatNumber)
        {
            var freshStanding = new SeatStanding(seatNumber, Array.Empty<SeatRoundScore>(), false, NoCardPoints);

            return freshStanding;
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

        internal bool IsFreeOfCaterpillars(BoardCell examinedCell)
        {
            bool isFree = _caterpillars.All(caterpillar => caterpillar.Cell != examinedCell);

            return isFree;
        }

        internal bool HasCaterpillarOf(int seatNumber, BoardCell examinedCell)
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

        internal TurnActionOutcome JudgeTurnOwner(int seatNumber)
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

        internal TurnActionOutcome JudgeBuildCell(BoardCell buildCell)
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
            int? nextSeatNumber = FindNextActiveSeatAfter(ActiveSeatNumber);
            if (nextSeatNumber is int placingSeatNumber)
            {
                ActiveSeatNumber = placingSeatNumber;
            }
            else
            {
                OpenButterflyPlacementFor(FindLastPlacerBefore(SeatCount + 1));
            }
        }

        private void PassButterflyPlacement()
        {
            if (RoundNumber == FirstRoundNumber)
            {
                OpenButterflyPlacementFor(FindLastPlacerBefore(ActiveSeatNumber));
            }
            else
            {
                OpenNextButterflyPlacement();
            }
        }

        private void PassRetiredSeat()
        {
            switch (Phase)
            {
                case TablePhase.CaterpillarPlacement:
                    PassCaterpillarPlacement();
                    break;

                case TablePhase.ButterflyPlacement:
                    PassButterflyPlacement();
                    break;

                case TablePhase.PlayerTurn:
                    PassTurn();
                    break;

                case TablePhase.Finished:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Phase));
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
                StartRoundTurns();
            }
        }

        private void OpenNextButterflyPlacement()
        {
            if (_pendingButterflySeatNumbers.Count > 0)
            {
                int nextPlacerSeatNumber = _pendingButterflySeatNumbers[0];
                _pendingButterflySeatNumbers.RemoveAt(0);
                OpenButterflyPlacementFor(nextPlacerSeatNumber);
            }
            else
            {
                StartRoundTurns();
            }
        }

        private void StartRoundTurns()
        {
            TurnRoundNumber = FirstTurnRoundNumber;
            BeginTurnOf(FindFirstActiveSeat());
        }

        private void PassTurn()
        {
            int? nextSeatNumber = FindNextActiveSeatAfter(ActiveSeatNumber);
            if (nextSeatNumber is int playingSeatNumber)
            {
                BeginTurnOf(playingSeatNumber);
            }
            else if (TurnRoundNumber < _rules.GetTurnCount(RoundNumber))
            {
                TurnRoundNumber++;
                BeginTurnOf(FindFirstActiveSeat());
            }
            else
            {
                CloseRound();
            }
        }

        private void CloseRound()
        {
            ScoreRound();
            CardTable.CloseRound();
            if (RoundNumber < MatchRules.RoundCount)
            {
                RoundNumber++;
                OpenRoundButterflyPlacement();
            }
            else
            {
                FinishMatch(Domain.FinishCause.Completed);
            }
        }

        private void ScoreRound()
        {
            var scoreCalculator = new RoundScoreCalculator(_board.FindPlanters(), _caterpillars, ButterflyCell);
            for (var i = 0; i < _standings.Count; i++)
            {
                SeatStanding standing = _standings[i];
                if (!standing.IsRetired)
                {
                    SeatRoundScore roundScore = scoreCalculator.ScoreSeat(standing.SeatNumber, RoundNumber);
                    _standings[i] = standing with { RoundScores = standing.RoundScores.Append(roundScore).ToList() };
                }
            }
        }

        private void OpenRoundButterflyPlacement()
        {
            SeatStanding[] activeStandings = _standings.Where(standing => !standing.IsRetired).ToArray();
            _chanceSource.Shuffle(activeStandings);
            List<int> placerOrder = activeStandings.OrderBy(standing => standing.TotalScore).Select(standing => standing.SeatNumber).ToList();
            _pendingButterflySeatNumbers.Clear();
            _pendingButterflySeatNumbers.AddRange(placerOrder);
            ActionPointsLeft = 0;
            BuildsLeft = 0;
            OpenNextButterflyPlacement();
        }

        private void BeginTurnOf(int seatNumber)
        {
            Phase = TablePhase.PlayerTurn;
            ActiveSeatNumber = seatNumber;
            ActionPointsLeft = ActionPointsPerTurn;
            BuildsLeft = _rules.GetBuildCount(RoundNumber, TurnRoundNumber);
            CardTable.StartTurn();
        }

        private void FinishMatch(FinishCause finishCause)
        {
            Phase = TablePhase.Finished;
            FinishCause = finishCause;
            CardTable.CashInHands();
            ActionPointsLeft = 0;
            BuildsLeft = 0;
            _pendingButterflySeatNumbers.Clear();
        }

        private int? FindNextActiveSeatAfter(int seatNumber)
        {
            int? nextSeatNumber = Enumerable.Range(seatNumber + 1, SeatCount - seatNumber).Where(IsActiveSeat).Cast<int?>().FirstOrDefault();

            return nextSeatNumber;
        }

        private int FindFirstActiveSeat()
        {
            int firstSeatNumber = _standings.First(standing => !standing.IsRetired).SeatNumber;

            return firstSeatNumber;
        }

        private bool IsActiveSeat(int seatNumber)
        {
            bool isActive = !_standings[seatNumber - FirstSeatNumber].IsRetired;

            return isActive;
        }

        private int CountActiveSeats()
        {
            int activeSeatCount = _standings.Count(standing => !standing.IsRetired);

            return activeSeatCount;
        }
    }
}
