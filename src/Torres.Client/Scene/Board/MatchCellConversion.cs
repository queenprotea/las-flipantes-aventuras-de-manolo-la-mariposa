using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

namespace Torres.Client.Scene
{
    internal static class MatchCellConversion
    {
        internal static BoardCell ToBoardCell(MatchCell matchCell)
        {
            var boardCell = new BoardCell(matchCell.Row, matchCell.Column);

            return boardCell;
        }

        internal static IReadOnlyList<BoardCell> ToBoardCells(IReadOnlyList<MatchCell> cells)
        {
            List<BoardCell> boardCells = cells.Select(ToBoardCell).ToList();

            return boardCells;
        }

        internal static MatchCell ToMatchCell(BoardCell boardCell)
        {
            var matchCell = new MatchCell { Row = boardCell.Row, Column = boardCell.Column };

            return matchCell;
        }
    }
}
