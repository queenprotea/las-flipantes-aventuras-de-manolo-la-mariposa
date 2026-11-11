using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public sealed class Board
    {
        public const int CellsPerSide = 8;
        public const int MaximumFlowerLevel = 3;

        private const int GroundLevel = 0;
        private const int LowestFlowerLevel = 1;
        private const string RepeatedFlowerMessage = "Two flowers cannot stand on the same cell.";
        private const string FullFlowerMessage = "A flower cannot grow past its maximum level.";

        private readonly int[,] _flowerLevels = new int[CellsPerSide, CellsPerSide];

        public Board(IReadOnlyList<Flower> flowers)
        {
            ArgumentNullException.ThrowIfNull(flowers);

            foreach (Flower flower in flowers)
            {
                PlantFlower(flower, nameof(flowers));
            }
        }

        internal static IReadOnlyList<BoardCell> AllCells { get; } = CreateAllCells();

        public static bool Contains(BoardCell examinedCell)
        {
            bool isRowInside = (examinedCell.Row >= 0) && (examinedCell.Row < CellsPerSide);
            bool isColumnInside = (examinedCell.Column >= 0) && (examinedCell.Column < CellsPerSide);
            bool isInside = isRowInside && isColumnInside;

            return isInside;
        }

        public int GetFlowerLevel(BoardCell examinedCell)
        {
            ThrowIfOutside(examinedCell, nameof(examinedCell));

            int flowerLevel = _flowerLevels[examinedCell.Row, examinedCell.Column];

            return flowerLevel;
        }

        public bool HasFlower(BoardCell examinedCell)
        {
            bool hasFlower = GetFlowerLevel(examinedCell) > GroundLevel;

            return hasFlower;
        }

        public IReadOnlyList<Flower> FindFlowers()
        {
            List<Flower> flowers = FindFlowerCells().Select(ToFlower).ToList();

            return flowers;
        }

        public IReadOnlyList<BoardCell> FindFlowerCells()
        {
            var flowerCells = new List<BoardCell>();
            for (var row = 0; row < CellsPerSide; row++)
            {
                AddFlowerCellsOfRow(row, flowerCells);
            }

            return flowerCells;
        }

        public void AddFlowerLevel(BoardCell growingCell)
        {
            int flowerLevel = GetFlowerLevel(growingCell);
            if (flowerLevel == MaximumFlowerLevel)
            {
                throw new InvalidOperationException(FullFlowerMessage);
            }

            _flowerLevels[growingCell.Row, growingCell.Column] = flowerLevel + 1;
        }

        public int CountPlantersAround(BoardCell examinedCell)
        {
            ThrowIfOutside(examinedCell, nameof(examinedCell));

            List<BoardCell> neighborCells = FindNeighborCells(examinedCell);
            int planterCount = FindPlanters().Count(planter => IsPlanterAround(planter, neighborCells));

            return planterCount;
        }

        public IReadOnlyList<Planter> FindPlanters()
        {
            var visitedCells = new HashSet<BoardCell>();
            var planters = new List<Planter>();
            foreach (BoardCell flowerCell in FindFlowerCells())
            {
                if (visitedCells.Contains(flowerCell))
                {
                    continue;
                }

                planters.Add(CollectPlanter(flowerCell, visitedCells));
            }

            return planters;
        }

        internal static void ThrowIfOutside(BoardCell examinedCell, string parameterName)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(examinedCell.Row, parameterName);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(examinedCell.Row, CellsPerSide, parameterName);
            ArgumentOutOfRangeException.ThrowIfNegative(examinedCell.Column, parameterName);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(examinedCell.Column, CellsPerSide, parameterName);
        }

        internal static List<BoardCell> FindNeighborCells(BoardCell centerCell)
        {
            var sideCells = new BoardCell[] { new BoardCell(centerCell.Row - 1, centerCell.Column), new BoardCell(centerCell.Row + 1, centerCell.Column), new BoardCell(centerCell.Row, centerCell.Column - 1), new BoardCell(centerCell.Row, centerCell.Column + 1) };
            List<BoardCell> neighborCells = sideCells.Where(Contains).ToList();

            return neighborCells;
        }

        private static bool IsPlanterAround(Planter planter, List<BoardCell> neighborCells)
        {
            bool isAround = planter.Flowers.Any(flower => neighborCells.Contains(flower.Cell));

            return isAround;
        }

        private static List<BoardCell> CreateAllCells()
        {
            var allCells = new List<BoardCell>();
            for (var row = 0; row < CellsPerSide; row++)
            {
                AddCellsOfRow(row, allCells);
            }

            return allCells;
        }

        private static void AddCellsOfRow(int row, List<BoardCell> allCells)
        {
            for (var column = 0; column < CellsPerSide; column++)
            {
                allCells.Add(new BoardCell(row, column));
            }
        }

        private void PlantFlower(Flower flower, string parameterName)
        {
            ThrowIfOutside(flower.Cell, parameterName);
            ArgumentOutOfRangeException.ThrowIfLessThan(flower.Level, LowestFlowerLevel, parameterName);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(flower.Level, MaximumFlowerLevel, parameterName);
            if (HasFlower(flower.Cell))
            {
                throw new ArgumentException(RepeatedFlowerMessage, parameterName);
            }

            _flowerLevels[flower.Cell.Row, flower.Cell.Column] = flower.Level;
        }

        private Flower ToFlower(BoardCell flowerCell)
        {
            var flower = new Flower(flowerCell, GetFlowerLevel(flowerCell));

            return flower;
        }

        private void AddFlowerCellsOfRow(int row, List<BoardCell> flowerCells)
        {
            for (var column = 0; column < CellsPerSide; column++)
            {
                var examinedCell = new BoardCell(row, column);
                if (HasFlower(examinedCell))
                {
                    flowerCells.Add(examinedCell);
                }
            }
        }

        private Planter CollectPlanter(BoardCell firstCell, HashSet<BoardCell> visitedCells)
        {
            var planterFlowers = new List<Flower>();
            var pendingCells = new Queue<BoardCell>();
            visitedCells.Add(firstCell);
            pendingCells.Enqueue(firstCell);
            while (pendingCells.Count > 0)
            {
                BoardCell currentCell = pendingCells.Dequeue();
                planterFlowers.Add(ToFlower(currentCell));
                EnqueueFlowerNeighbors(currentCell, visitedCells, pendingCells);
            }

            var planter = new Planter(planterFlowers);

            return planter;
        }

        private void EnqueueFlowerNeighbors(BoardCell centerCell, HashSet<BoardCell> visitedCells, Queue<BoardCell> pendingCells)
        {
            foreach (BoardCell neighborCell in FindNeighborCells(centerCell))
            {
                bool isUnvisitedFlower = HasFlower(neighborCell) && !visitedCells.Contains(neighborCell);
                if (isUnvisitedFlower)
                {
                    visitedCells.Add(neighborCell);
                    pendingCells.Enqueue(neighborCell);
                }
            }
        }
    }
}
