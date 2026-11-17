using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    internal sealed class ActionCardEffects
    {
        private const int MoveCellCount = 2;
        private const int LowestRemovableLevel = 2;
        private const int LeapLength = 2;
        private const int MostRemovedLevels = 2;

        private readonly MatchTable _matchTable;
        private readonly Board _board;

        internal ActionCardEffects(MatchTable matchTable, Board board)
        {
            _matchTable = matchTable;
            _board = board;
        }

        internal TurnActionOutcome Apply(int seatNumber, CardPlay cardPlay)
        {
            IReadOnlyList<BoardCell> playedCells = cardPlay.AffectedCells;
            TurnActionOutcome actionOutcome = cardPlay.Card switch
            {
                ActionCard.DoubleClimb => MoveCaterpillar(seatNumber, playedCells, JudgeDoubleClimb),
                ActionCard.GroundLeap => MoveCaterpillar(seatNumber, playedCells, JudgeGroundLeap),
                ActionCard.Relocation => MoveCaterpillar(seatNumber, playedCells, JudgeRelocation),
                ActionCard.PlanterCrossing => MoveCaterpillar(seatNumber, playedCells, JudgePlanterCrossing),
                ActionCard.FlowerLevelMove => MoveFlowerLevel(playedCells),
                ActionCard.FlowerLevelRemoval => RemoveFlowerLevels(playedCells),
                ActionCard.SixActionPoints or ActionCard.SevenActionPoints => throw new ArgumentOutOfRangeException(nameof(cardPlay)),
                _ => throw new ArgumentOutOfRangeException(nameof(cardPlay)),
            };

            return actionOutcome;
        }

        internal IReadOnlyList<BoardCell> FindLevelRemovalTargets()
        {
            List<BoardCell> removalTargets = _board.FindFlowerCells().Where(IsRemovableLevel).ToList();

            return removalTargets;
        }

        private static bool IsStraightLeap(BoardCell origin, BoardCell destination)
        {
            int rowDistance = Math.Abs(destination.Row - origin.Row);
            int columnDistance = Math.Abs(destination.Column - origin.Column);
            bool isLeapAcrossRows = (rowDistance == LeapLength) && (columnDistance == 0);
            bool isLeapAcrossColumns = (rowDistance == 0) && (columnDistance == LeapLength);
            bool isStraightLeap = isLeapAcrossRows || isLeapAcrossColumns;

            return isStraightLeap;
        }

        private static BoardCell FindJumpedCell(BoardCell origin, BoardCell destination)
        {
            int rowStep = (destination.Row - origin.Row) / LeapLength;
            int columnStep = (destination.Column - origin.Column) / LeapLength;
            var jumpedCell = new BoardCell(origin.Row + rowStep, origin.Column + columnStep);

            return jumpedCell;
        }

        private TurnActionOutcome MoveCaterpillar(int seatNumber, IReadOnlyList<BoardCell> playedCells, Func<BoardCell, BoardCell, TurnActionOutcome> cardMoveRule)
        {
            TurnActionOutcome actionOutcome = JudgeCaterpillarMove(seatNumber, playedCells);
            if (actionOutcome == TurnActionOutcome.Applied)
            {
                actionOutcome = cardMoveRule(playedCells[0], playedCells[1]);
            }

            if (actionOutcome == TurnActionOutcome.Applied)
            {
                _matchTable.RelocateCaterpillar(seatNumber, playedCells[0], playedCells[1]);
            }

            return actionOutcome;
        }

        private TurnActionOutcome JudgeCaterpillarMove(int seatNumber, IReadOnlyList<BoardCell> playedCells)
        {
            TurnActionOutcome actionOutcome = TurnActionOutcome.Applied;
            if ((playedCells.Count != MoveCellCount) || (playedCells[0] == playedCells[1]))
            {
                actionOutcome = TurnActionOutcome.WrongCardTarget;
            }
            else if (!_matchTable.HasCaterpillarOf(seatNumber, playedCells[0]))
            {
                actionOutcome = TurnActionOutcome.NotYourCaterpillar;
            }
            else if (!_matchTable.IsFreeOfCaterpillars(playedCells[1]))
            {
                actionOutcome = TurnActionOutcome.CellOccupied;
            }

            return actionOutcome;
        }

        private TurnActionOutcome JudgeDoubleClimb(BoardCell origin, BoardCell destination)
        {
            bool isClimbFromOneToThree = (_board.GetFlowerLevel(origin) == Board.LowestFlowerLevel) && (_board.GetFlowerLevel(destination) == Board.MaximumFlowerLevel);
            TurnActionOutcome actionOutcome = TurnActionOutcome.Applied;
            if (!isClimbFromOneToThree)
            {
                actionOutcome = TurnActionOutcome.WrongCardTarget;
            }
            else if (!Board.FindNeighborCells(origin).Contains(destination))
            {
                actionOutcome = TurnActionOutcome.NotNeighborCell;
            }

            return actionOutcome;
        }

        private TurnActionOutcome JudgeGroundLeap(BoardCell origin, BoardCell destination)
        {
            bool isGroundLeap = IsStraightLeap(origin, destination) && !_board.HasFlower(origin) && !_board.HasFlower(destination);
            TurnActionOutcome actionOutcome = TurnActionOutcome.WrongCardTarget;
            if (isGroundLeap && IsCaterpillarOnGround(FindJumpedCell(origin, destination)))
            {
                actionOutcome = TurnActionOutcome.Applied;
            }

            return actionOutcome;
        }

        private TurnActionOutcome JudgeRelocation(BoardCell origin, BoardCell destination)
        {
            TurnActionOutcome actionOutcome = TurnActionOutcome.Applied;
            if (CalculateClimb(origin, destination) > MatchTable.MaximumLevelsClimbedPerMove)
            {
                actionOutcome = TurnActionOutcome.ClimbTooHigh;
            }

            return actionOutcome;
        }

        private TurnActionOutcome JudgePlanterCrossing(BoardCell origin, BoardCell destination)
        {
            TurnActionOutcome actionOutcome = TurnActionOutcome.Applied;
            if (!_board.HasFlower(destination))
            {
                actionOutcome = TurnActionOutcome.NotAFlower;
            }
            else if (!IsReachableThroughPlanter(origin, destination))
            {
                actionOutcome = TurnActionOutcome.WrongCardTarget;
            }
            else if (CalculateClimb(origin, destination) > MatchTable.MaximumLevelsClimbedPerMove)
            {
                actionOutcome = TurnActionOutcome.ClimbTooHigh;
            }

            return actionOutcome;
        }

        private bool IsReachableThroughPlanter(BoardCell origin, BoardCell destination)
        {
            List<BoardCell> planterCells = _board.FindPlanterOf(destination)?.Flowers.Select(flower => flower.Cell).ToList() ?? new List<BoardCell>();
            bool isReachable;
            if (_board.HasFlower(origin))
            {
                isReachable = planterCells.Contains(origin);
            }
            else
            {
                isReachable = Board.FindNeighborCells(origin).Any(planterCells.Contains);
            }

            return isReachable;
        }

        private bool IsCaterpillarOnGround(BoardCell examinedCell)
        {
            bool isCaterpillarOnGround = !_board.HasFlower(examinedCell) && !_matchTable.IsFreeOfCaterpillars(examinedCell);

            return isCaterpillarOnGround;
        }

        private int CalculateClimb(BoardCell origin, BoardCell destination)
        {
            int levelsClimbed = _board.GetFlowerLevel(destination) - _board.GetFlowerLevel(origin);

            return levelsClimbed;
        }

        private TurnActionOutcome MoveFlowerLevel(IReadOnlyList<BoardCell> playedCells)
        {
            TurnActionOutcome actionOutcome = TurnActionOutcome.WrongCardTarget;
            if ((playedCells.Count == MoveCellCount) && (playedCells[0] != playedCells[1]))
            {
                actionOutcome = JudgeRemovableLevel(playedCells[0]);
            }

            if (actionOutcome == TurnActionOutcome.Applied)
            {
                actionOutcome = _matchTable.JudgeBuildCell(playedCells[1]);
            }

            if (actionOutcome == TurnActionOutcome.Applied)
            {
                _board.RemoveFlowerLevel(playedCells[0]);
                _board.AddFlowerLevel(playedCells[1]);
            }

            return actionOutcome;
        }

        private TurnActionOutcome RemoveFlowerLevels(IReadOnlyList<BoardCell> playedCells)
        {
            bool hasValidCellCount = (playedCells.Count > 0) && (playedCells.Count <= MostRemovedLevels) && (playedCells.Distinct().Count() == playedCells.Count);
            TurnActionOutcome actionOutcome = TurnActionOutcome.WrongCardTarget;
            if (hasValidCellCount)
            {
                actionOutcome = playedCells.Select(JudgeRemovableLevel).FirstOrDefault(cellOutcome => cellOutcome != TurnActionOutcome.Applied, TurnActionOutcome.Applied);
            }

            if (actionOutcome == TurnActionOutcome.Applied)
            {
                foreach (BoardCell flowerCell in playedCells)
                {
                    _board.RemoveFlowerLevel(flowerCell);
                }
            }

            return actionOutcome;
        }

        private TurnActionOutcome JudgeRemovableLevel(BoardCell flowerCell)
        {
            TurnActionOutcome actionOutcome = TurnActionOutcome.Applied;
            if (_board.GetFlowerLevel(flowerCell) < LowestRemovableLevel)
            {
                actionOutcome = TurnActionOutcome.WrongCardTarget;
            }
            else if (!_matchTable.IsFreeOfCaterpillars(flowerCell))
            {
                actionOutcome = TurnActionOutcome.CellOccupied;
            }

            return actionOutcome;
        }

        private bool IsRemovableLevel(BoardCell flowerCell)
        {
            bool isRemovable = JudgeRemovableLevel(flowerCell) == TurnActionOutcome.Applied;

            return isRemovable;
        }
    }
}
