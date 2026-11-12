using Game.Contracts;
using Game.Domain;

namespace Game.Match
{
    internal static class MatchCellConversion
    {
        internal static BoardCell ToBoardCell(MatchCell matchCell)
        {
            var boardCell = new BoardCell(matchCell.Row, matchCell.Column);

            return boardCell;
        }

        internal static CaterpillarMove ToCaterpillarMove(MatchMove matchMove)
        {
            var caterpillarMove = new CaterpillarMove(ToBoardCell(matchMove.Origin), ToBoardCell(matchMove.Destination));

            return caterpillarMove;
        }

        internal static MatchCell ToMatchCell(BoardCell boardCell)
        {
            var matchCell = new MatchCell { Row = boardCell.Row, Column = boardCell.Column };

            return matchCell;
        }
    }
}
