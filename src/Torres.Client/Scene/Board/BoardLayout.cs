using System.Collections.Generic;
using System.Linq;

namespace Torres.Client.Scene
{
    // Each row is written with one character per cell: a letter names the planter, a digit is the flower level and a dot is ground.
    internal sealed class BoardLayout
    {
        internal const int Size = 8;

        private const char GroundSymbol = '.';
        private const char LevelZeroSymbol = '0';

        private readonly IReadOnlyList<string> _planterRows;
        private readonly IReadOnlyList<string> _levelRows;

        internal BoardLayout(IReadOnlyList<string> planterRows, IReadOnlyList<string> levelRows, IReadOnlyList<BoardCaterpillar> caterpillars)
        {
            _planterRows = planterRows;
            _levelRows = levelRows;
            Caterpillars = caterpillars;
        }

        internal IReadOnlyList<BoardCaterpillar> Caterpillars { get; }

        internal static IEnumerable<BoardCell> EnumerateCells()
        {
            for (var row = 0; row < Size; row++)
            {
                for (var column = 0; column < Size; column++)
                {
                    var cell = new BoardCell(row, column);
                    yield return cell;
                }
            }
        }

        internal bool IsPlanter(BoardCell cell)
        {
            bool isPlanter = GetPlanterSymbol(cell) != GroundSymbol;

            return isPlanter;
        }

        internal bool IsSamePlanter(BoardCell cell, BoardCell otherCell)
        {
            bool isSamePlanter = GetPlanterSymbol(cell) == GetPlanterSymbol(otherCell);

            return isSamePlanter;
        }

        internal int GetFlowerLevel(BoardCell cell)
        {
            int flowerLevel = 0;
            char levelSymbol = _levelRows[cell.Row][cell.Column];
            if (levelSymbol != GroundSymbol)
            {
                flowerLevel = levelSymbol - LevelZeroSymbol;
            }

            return flowerLevel;
        }

        internal bool HasCaterpillar(BoardCell cell)
        {
            bool hasCaterpillar = Caterpillars.Any(caterpillar => caterpillar.Cell == cell);

            return hasCaterpillar;
        }

        private static bool Contains(BoardCell cell)
        {
            bool isRowInside = (cell.Row >= 0) && (cell.Row < Size);
            bool isColumnInside = (cell.Column >= 0) && (cell.Column < Size);
            bool isInside = isRowInside && isColumnInside;

            return isInside;
        }

        private char GetPlanterSymbol(BoardCell cell)
        {
            char planterSymbol = GroundSymbol;
            if (Contains(cell))
            {
                planterSymbol = _planterRows[cell.Row][cell.Column];
            }

            return planterSymbol;
        }
    }
}
