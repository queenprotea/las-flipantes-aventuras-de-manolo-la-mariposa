using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

namespace Torres.Client.Scene
{
    internal static class MatchCellConversion
    {
        internal static BoardCell ToBoardCell(MatchCell cell)
        {
            var boardCell = new BoardCell(cell.Row, cell.Column);

            return boardCell;
        }

        internal static IReadOnlyList<BoardCell> ToBoardCells(IReadOnlyList<MatchCell> cells)
        {
            List<BoardCell> boardCells = cells.Select(ToBoardCell).ToList();

            return boardCells;
        }

        internal static MatchCell ToMatchCell(BoardCell cell)
        {
            var matchCell = new MatchCell { Row = cell.Row, Column = cell.Column };

            return matchCell;
        }
    }
}
