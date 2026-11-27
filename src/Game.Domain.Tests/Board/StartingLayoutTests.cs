using System;
using System.Collections.Generic;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class StartingLayoutTests
    {
        private static readonly BoardCell _eastOfNorthWest = new BoardCell(1, 2);
        private static readonly BoardCell _cornerOfNorthWest = new BoardCell(2, 2);
        private static readonly BoardCell _belowBoardCell = new BoardCell(8, 1);

        [Fact]
        public void Test_Constructor_WithFiveSingleCellPlanters_KeepsFiveFlowers()
        {
            IReadOnlyList<Flower> flowers = SampleLayouts.FiveStartingFlowers;

            var layout = new StartingLayout(flowers);

            Assert.Equal(SampleLayouts.FiveFlowers, layout.Flowers.Count);
        }

        [Fact]
        public void Test_Constructor_WithTwoFlowersSideBySide_ThrowsArgumentException()
        {
            var flowers = new Flower[] { new Flower(SampleLayouts.NorthWestFlower, SampleLayouts.LevelOne), new Flower(_eastOfNorthWest, SampleLayouts.LevelOne), new Flower(SampleLayouts.CenterFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.SouthWestFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.SouthEastFlower, SampleLayouts.LevelOne) };

            Assert.Throws<ArgumentException>(() => new StartingLayout(flowers));
        }

        [Fact]
        public void Test_Constructor_WithFlowersTouchingOnACorner_KeepsBothFlowers()
        {
            var flowers = new Flower[] { new Flower(SampleLayouts.NorthWestFlower, SampleLayouts.LevelOne), new Flower(_cornerOfNorthWest, SampleLayouts.LevelOne), new Flower(SampleLayouts.GroundCell, SampleLayouts.LevelOne), new Flower(SampleLayouts.SouthWestFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.SouthEastFlower, SampleLayouts.LevelOne) };

            var layout = new StartingLayout(flowers);

            Assert.Equal(SampleLayouts.FiveFlowers, layout.Flowers.Count);
        }

        [Fact]
        public void Test_Constructor_WithASecondLevelFlower_ThrowsArgumentException()
        {
            var flowers = new Flower[] { new Flower(SampleLayouts.NorthWestFlower, SampleLayouts.LevelTwo), new Flower(SampleLayouts.NorthEastFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.CenterFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.SouthWestFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.SouthEastFlower, SampleLayouts.LevelOne) };

            Assert.Throws<ArgumentException>(() => new StartingLayout(flowers));
        }

        [Fact]
        public void Test_Constructor_WithThreeFlowers_ThrowsArgumentOutOfRangeException()
        {
            var flowers = new Flower[] { new Flower(SampleLayouts.NorthWestFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.NorthEastFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.CenterFlower, SampleLayouts.LevelOne) };

            Assert.Throws<ArgumentOutOfRangeException>(() => new StartingLayout(flowers));
        }

        [Fact]
        public void Test_Constructor_WithAFlowerOutsideTheBoard_ThrowsArgumentOutOfRangeException()
        {
            var flowers = new Flower[] { new Flower(_belowBoardCell, SampleLayouts.LevelOne), new Flower(SampleLayouts.NorthEastFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.CenterFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.SouthWestFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.SouthEastFlower, SampleLayouts.LevelOne) };

            Assert.Throws<ArgumentOutOfRangeException>(() => new StartingLayout(flowers));
        }

        [Fact]
        public void Test_Constructor_WithTwoFlowersOnTheSameCell_ThrowsArgumentException()
        {
            var flowers = new Flower[] { new Flower(SampleLayouts.NorthWestFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.NorthWestFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.CenterFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.SouthWestFlower, SampleLayouts.LevelOne), new Flower(SampleLayouts.SouthEastFlower, SampleLayouts.LevelOne) };

            Assert.Throws<ArgumentException>(() => new StartingLayout(flowers));
        }
    }
}
