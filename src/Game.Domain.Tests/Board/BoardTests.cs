using System;
using System.Collections.Generic;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class BoardTests
    {
        private const int FivePlanters = 5;
        private const int TwoPlanters = 2;
        private const int TwoCellCount = 2;

        [Fact]
        public void Test_FindPlanters_WithFiveSeparateFlowers_ReturnsFivePlanters()
        {
            var board = new Board(SampleLayouts.FiveStartingFlowers);

            int planterCount = board.FindPlanters().Count;

            Assert.Equal(FivePlanters, planterCount);
        }

        [Fact]
        public void Test_FindPlanters_WithTwoFlowersSideBySide_ReturnsOnePlanterOfTwoCells()
        {
            var board = new Board(new Flower[] { new Flower(new BoardCell(2, 2), 1), new Flower(new BoardCell(2, 3), 2) });

            IReadOnlyList<Planter> planters = board.FindPlanters();

            Planter planter = Assert.Single(planters);
            Assert.Equal(TwoCellCount, planter.CellCount);
        }

        [Fact]
        public void Test_FindPlanters_WithFlowersTouchingOnACorner_ReturnsTwoPlanters()
        {
            var board = new Board(new Flower[] { new Flower(new BoardCell(2, 2), 1), new Flower(new BoardCell(3, 3), 1) });

            int planterCount = board.FindPlanters().Count;

            Assert.Equal(TwoPlanters, planterCount);
        }

        [Fact]
        public void Test_HasFlower_OnGround_ReturnsFalse()
        {
            var board = new Board(SampleLayouts.FiveStartingFlowers);

            bool hasFlower = board.HasFlower(SampleLayouts.GroundCell);

            Assert.False(hasFlower);
        }

        [Fact]
        public void Test_Contains_WithRowEight_ReturnsFalse()
        {
            BoardCell outsideCell = SampleLayouts.OutsideCell;

            bool isInside = Board.Contains(outsideCell);

            Assert.False(isInside);
        }

        [Fact]
        public void Test_CountPlantersAround_OfCellBetweenTwoPlanters_ReturnsTwo()
        {
            var board = new Board(new Flower[] { new Flower(new BoardCell(2, 1), 1), new Flower(new BoardCell(2, 3), 1) });

            int planterCount = board.CountPlantersAround(new BoardCell(2, 2));

            Assert.Equal(TwoPlanters, planterCount);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnGround_PlantsALevelOneFlower()
        {
            var board = new Board(SampleLayouts.FiveStartingFlowers);

            board.AddFlowerLevel(SampleLayouts.GroundCell);

            Assert.Equal(StartingLayout.StartingFlowerLevel, board.GetFlowerLevel(SampleLayouts.GroundCell));
        }

        [Fact]
        public void Test_AddFlowerLevel_OnLevelThreeFlower_ThrowsInvalidOperationException()
        {
            var board = new Board(new Flower[] { new Flower(SampleLayouts.CenterFlower, Board.MaximumFlowerLevel) });

            Assert.Throws<InvalidOperationException>(() => board.AddFlowerLevel(SampleLayouts.CenterFlower));
        }
    }
}
