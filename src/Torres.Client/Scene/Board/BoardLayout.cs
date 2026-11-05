using System;
using System.Collections.Generic;
using System.Linq;

namespace Torres.Client.Scene
{
    internal sealed class BoardLayout
    {
        internal const int CellsPerSide = 8;

        internal const char GroundSymbol = '.';
        internal const char LevelZeroSymbol = '0';

        private static readonly string _groundRow = new string(GroundSymbol, CellsPerSide);

        private readonly IReadOnlyList<string> _planterRows;
        private readonly IReadOnlyList<string> _levelRows;

        internal BoardLayout(IReadOnlyList<string> planterRows, IReadOnlyList<string> levelRows, IReadOnlyList<BoardCaterpillar> caterpillars)
        {
            _planterRows = planterRows;
            _levelRows = levelRows;
            Caterpillars = caterpillars;
        }

        internal static BoardLayout Empty { get; } = new BoardLayout(Enumerable.Repeat(_groundRow, CellsPerSide).ToList(), Enumerable.Repeat(_groundRow, CellsPerSide).ToList(), Array.Empty<BoardCaterpillar>());

        internal IReadOnlyList<BoardCaterpillar> Caterpillars { get; }

        internal static IEnumerable<BoardCell> EnumerateCells()
        {
            for (var row = 0; row < CellsPerSide; row++)
            {
                for (var column = 0; column < CellsPerSide; column++)
                {
                    var enumeratedCell = new BoardCell(row, column);
                    yield return enumeratedCell;
                }
            }
        }

        internal bool IsPlanter(BoardCell examinedCell)
        {
            bool isPlanter = GetPlanterSymbol(examinedCell) != GroundSymbol;

            return isPlanter;
        }

        internal bool IsSamePlanter(BoardCell firstCell, BoardCell secondCell)
        {
            bool isSamePlanter = GetPlanterSymbol(firstCell) == GetPlanterSymbol(secondCell);

            return isSamePlanter;
        }

        internal int GetFlowerLevel(BoardCell examinedCell)
        {
            int flowerLevel = 0;
            char levelSymbol = _levelRows[examinedCell.Row][examinedCell.Column];
            if (levelSymbol != GroundSymbol)
            {
                flowerLevel = levelSymbol - LevelZeroSymbol;
            }

            return flowerLevel;
        }

        internal bool HasCaterpillar(BoardCell examinedCell)
        {
            bool hasCaterpillar = Caterpillars.Any(caterpillar => caterpillar.Cell == examinedCell);

            return hasCaterpillar;
        }

        private static bool Contains(BoardCell examinedCell)
        {
            bool isRowInside = (examinedCell.Row >= 0) && (examinedCell.Row < CellsPerSide);
            bool isColumnInside = (examinedCell.Column >= 0) && (examinedCell.Column < CellsPerSide);
            bool isInside = isRowInside && isColumnInside;

            return isInside;
        }

        private char GetPlanterSymbol(BoardCell examinedCell)
        {
            char planterSymbol = GroundSymbol;
            if (Contains(examinedCell))
            {
                planterSymbol = _planterRows[examinedCell.Row][examinedCell.Column];
            }

            return planterSymbol;
        }
    }
}
