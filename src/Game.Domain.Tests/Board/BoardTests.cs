using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class BoardTests
    {
        private const int FivePlanters = 5;
        private const int TwoPlanters = 2;
        private const int TwoCellCount = 2;
        private const int LevelOne = 1;
        private const int LevelTwo = 2;

        private static readonly BoardCell _westCell = new BoardCell(2, 1);
        private static readonly BoardCell _middleCell = new BoardCell(2, 2);
        private static readonly BoardCell _eastCell = new BoardCell(2, 3);
        private static readonly BoardCell _southEastCell = new BoardCell(3, 3);

        [Fact]
        public void Test_FindPlanters_WithFiveSeparateFlowers_ReturnsFivePlanters()
        {
            var board = new Board(SampleLayouts.FiveStartingFlowers);

            int planterCount = board.FindPlanters().Count;

            Assert.Equal(FivePlanters, planterCount);
        }

        [Fact]
        public void Test_FindPlanters_WithTwoFlowersSideBySide_ReturnsOnePlanter()
        {
            var board = new Board(new Flower[] { new Flower(_middleCell, LevelOne), new Flower(_eastCell, LevelTwo) });

            IReadOnlyList<Planter> planters = board.FindPlanters();

            Assert.Single(planters);
        }

        [Fact]
        public void Test_FindPlanters_WithTwoFlowersSideBySide_ReturnsAPlanterOfTwoCells()
        {
            var board = new Board(new Flower[] { new Flower(_middleCell, LevelOne), new Flower(_eastCell, LevelTwo) });

            IReadOnlyList<Planter> planters = board.FindPlanters();

            Planter planter = planters.Single();
            Assert.Equal(TwoCellCount, planter.CellCount);
        }

        [Fact]
        public void Test_FindPlanters_WithFlowersTouchingOnACorner_ReturnsTwoPlanters()
        {
            var board = new Board(new Flower[] { new Flower(_middleCell, LevelOne), new Flower(_southEastCell, LevelOne) });

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
            var board = new Board(new Flower[] { new Flower(_westCell, LevelOne), new Flower(_eastCell, LevelOne) });

            int planterCount = board.CountPlantersAround(_middleCell);

            Assert.Equal(TwoPlanters, planterCount);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnGround_PlantsALevelOneFlower()
        {
            var board = new Board(SampleLayouts.FiveStartingFlowers);

            board.AddFlowerLevel(SampleLayouts.GroundCell);

            int flowerLevel = board.GetFlowerLevel(SampleLayouts.GroundCell);
            Assert.Equal(StartingLayout.StartingFlowerLevel, flowerLevel);
        }

        [Fact]
        public void Test_AddFlowerLevel_OnLevelThreeFlower_ThrowsInvalidOperationException()
        {
            var board = new Board(new Flower[] { new Flower(SampleLayouts.CenterFlower, Board.MaximumFlowerLevel) });

            Assert.Throws<InvalidOperationException>(() => board.AddFlowerLevel(SampleLayouts.CenterFlower));
        }
    }
}
