using Game.Contracts;
using Game.Domain;

namespace Game.Match
{
    internal static class MatchCellConversion
    {
        internal static BoardCell ToBoardCell(MatchCell cell)
        {
            var boardCell = new BoardCell(cell.Row, cell.Column);

            return boardCell;
        }

        internal static MatchCell ToMatchCell(BoardCell cell)
        {
            var matchCell = new MatchCell { Row = cell.Row, Column = cell.Column };

            return matchCell;
        }
    }
}
