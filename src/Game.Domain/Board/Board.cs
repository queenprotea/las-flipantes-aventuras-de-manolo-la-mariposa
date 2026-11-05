using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public sealed class Board
    {
        public const int Size = 8;
        public const int MaximumFlowerHeight = 3;

        private const int GroundHeight = 0;
        private const int LowestFlowerHeight = 1;
        private const string RepeatedFlowerMessage = "Two flowers cannot stand on the same cell.";

        private readonly int[,] _heights = new int[Size, Size];

        public Board(IReadOnlyList<Flower> flowers)
        {
            ArgumentNullException.ThrowIfNull(flowers);

            foreach (Flower flower in flowers)
            {
                PlantFlower(flower, nameof(flowers));
            }
        }

        public static bool Contains(BoardCell cell)
        {
            bool isRowInside = (cell.Row >= 0) && (cell.Row < Size);
            bool isColumnInside = (cell.Column >= 0) && (cell.Column < Size);
            bool isInside = isRowInside && isColumnInside;

            return isInside;
        }

        public int GetHeight(BoardCell cell)
        {
            ThrowIfOutside(cell, nameof(cell));

            int height = _heights[cell.Row, cell.Column];

            return height;
        }

        public bool HasFlower(BoardCell cell)
        {
            bool hasFlower = GetHeight(cell) > GroundHeight;

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
            for (var row = 0; row < Size; row++)
            {
                AddFlowerCellsOfRow(row, flowerCells);
            }

            return flowerCells;
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

        internal static void ThrowIfOutside(BoardCell cell, string parameterName)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(cell.Row, parameterName);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(cell.Row, Size, parameterName);
            ArgumentOutOfRangeException.ThrowIfNegative(cell.Column, parameterName);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(cell.Column, Size, parameterName);
        }

        private static List<BoardCell> FindNeighborCells(BoardCell cell)
        {
            var sideCells = new BoardCell[] { new BoardCell(cell.Row - 1, cell.Column), new BoardCell(cell.Row + 1, cell.Column), new BoardCell(cell.Row, cell.Column - 1), new BoardCell(cell.Row, cell.Column + 1) };
            List<BoardCell> neighborCells = sideCells.Where(Contains).ToList();

            return neighborCells;
        }

        private void PlantFlower(Flower flower, string parameterName)
        {
            ThrowIfOutside(flower.Cell, parameterName);
            ArgumentOutOfRangeException.ThrowIfLessThan(flower.Height, LowestFlowerHeight, parameterName);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(flower.Height, MaximumFlowerHeight, parameterName);
            if (HasFlower(flower.Cell))
            {
                throw new ArgumentException(RepeatedFlowerMessage, parameterName);
            }

            _heights[flower.Cell.Row, flower.Cell.Column] = flower.Height;
        }

        private Flower ToFlower(BoardCell flowerCell)
        {
            var flower = new Flower(flowerCell, GetHeight(flowerCell));

            return flower;
        }

        private void AddFlowerCellsOfRow(int row, List<BoardCell> flowerCells)
        {
            for (var column = 0; column < Size; column++)
            {
                var cell = new BoardCell(row, column);
                if (HasFlower(cell))
                {
                    flowerCells.Add(cell);
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
                BoardCell cell = pendingCells.Dequeue();
                planterFlowers.Add(ToFlower(cell));
                EnqueueFlowerNeighbors(cell, visitedCells, pendingCells);
            }

            var planter = new Planter(planterFlowers);

            return planter;
        }

        private void EnqueueFlowerNeighbors(BoardCell cell, HashSet<BoardCell> visitedCells, Queue<BoardCell> pendingCells)
        {
            foreach (BoardCell neighborCell in FindNeighborCells(cell))
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
