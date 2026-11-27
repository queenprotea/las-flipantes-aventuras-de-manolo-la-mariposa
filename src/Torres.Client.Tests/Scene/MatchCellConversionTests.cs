using System.Collections.Generic;

using Game.Contracts;

using Torres.Client.Scene;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class MatchCellConversionTests
    {
        private const int CellRow = 3;
        private const int CellColumn = 4;

        private static readonly BoardCell _boardCell = new BoardCell(CellRow, CellColumn);

        private readonly MatchCell _matchCell = new MatchCell { Row = CellRow, Column = CellColumn };

        [Fact]
        public void Test_ToBoardCell_OfAMatchCell_KeepsRowAndColumn()
        {
            BoardCell boardCell = MatchCellConversion.ToBoardCell(_matchCell);

            Assert.Equal(_boardCell, boardCell);
        }

        [Fact]
        public void Test_ToBoardCells_OfOneMatchCell_ReturnsItsBoardCell()
        {
            IReadOnlyList<BoardCell> boardCells = MatchCellConversion.ToBoardCells(new List<MatchCell> { _matchCell });

            Assert.Equal(new BoardCell[] { _boardCell }, boardCells);
        }

        [Fact]
        public void Test_ToMatchCell_OfABoardCell_KeepsTheRow()
        {
            MatchCell matchCell = MatchCellConversion.ToMatchCell(_boardCell);

            Assert.Equal(CellRow, matchCell.Row);
        }

        [Fact]
        public void Test_ToMatchCell_OfABoardCell_KeepsTheColumn()
        {
            MatchCell matchCell = MatchCellConversion.ToMatchCell(_boardCell);

            Assert.Equal(CellColumn, matchCell.Column);
        }
    }
}
