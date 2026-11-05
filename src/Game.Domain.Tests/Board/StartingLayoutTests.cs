using System;
using System.Collections.Generic;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class StartingLayoutTests
    {
        private const int FiveFlowers = 5;
        private const int SecondLevel = 2;

        [Fact]
        public void Test_Constructor_WithFiveSingleCellPlanters_KeepsFiveFlowers()
        {
            IReadOnlyList<Flower> flowers = SampleLayouts.FiveStartingFlowers;

            var layout = new StartingLayout(flowers);

            Assert.Equal(FiveFlowers, layout.Flowers.Count);
        }

        [Fact]
        public void Test_Constructor_WithTwoFlowersSideBySide_ThrowsArgumentException()
        {
            var flowers = new Flower[] { new Flower(new BoardCell(1, 1), 1), new Flower(new BoardCell(1, 2), 1), new Flower(new BoardCell(3, 3), 1), new Flower(new BoardCell(6, 1), 1), new Flower(new BoardCell(6, 6), 1) };

            Assert.Throws<ArgumentException>(() => new StartingLayout(flowers));
        }

        [Fact]
        public void Test_Constructor_WithFlowersTouchingOnACorner_KeepsBothFlowers()
        {
            var flowers = new Flower[] { new Flower(new BoardCell(1, 1), 1), new Flower(new BoardCell(2, 2), 1), new Flower(new BoardCell(4, 4), 1), new Flower(new BoardCell(6, 1), 1), new Flower(new BoardCell(6, 6), 1) };

            var layout = new StartingLayout(flowers);

            Assert.Equal(FiveFlowers, layout.Flowers.Count);
        }

        [Fact]
        public void Test_Constructor_WithASecondLevelFlower_ThrowsArgumentException()
        {
            var flowers = new Flower[] { new Flower(new BoardCell(1, 1), SecondLevel), new Flower(new BoardCell(1, 6), 1), new Flower(new BoardCell(3, 3), 1), new Flower(new BoardCell(6, 1), 1), new Flower(new BoardCell(6, 6), 1) };

            Assert.Throws<ArgumentException>(() => new StartingLayout(flowers));
        }

        [Fact]
        public void Test_Constructor_WithThreeFlowers_ThrowsArgumentOutOfRangeException()
        {
            var flowers = new Flower[] { new Flower(new BoardCell(1, 1), 1), new Flower(new BoardCell(1, 6), 1), new Flower(new BoardCell(3, 3), 1) };

            Assert.Throws<ArgumentOutOfRangeException>(() => new StartingLayout(flowers));
        }

        [Fact]
        public void Test_Constructor_WithAFlowerOutsideTheBoard_ThrowsArgumentOutOfRangeException()
        {
            var flowers = new Flower[] { new Flower(new BoardCell(8, 1), 1), new Flower(new BoardCell(1, 6), 1), new Flower(new BoardCell(3, 3), 1), new Flower(new BoardCell(6, 1), 1), new Flower(new BoardCell(6, 6), 1) };

            Assert.Throws<ArgumentOutOfRangeException>(() => new StartingLayout(flowers));
        }

        [Fact]
        public void Test_Constructor_WithTwoFlowersOnTheSameCell_ThrowsArgumentException()
        {
            var flowers = new Flower[] { new Flower(new BoardCell(1, 1), 1), new Flower(new BoardCell(1, 1), 1), new Flower(new BoardCell(3, 3), 1), new Flower(new BoardCell(6, 1), 1), new Flower(new BoardCell(6, 6), 1) };

            Assert.Throws<ArgumentException>(() => new StartingLayout(flowers));
        }
    }
}
