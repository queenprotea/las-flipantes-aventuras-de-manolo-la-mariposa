using System;
using System.Collections.Generic;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class StartingLayoutTests
    {
        private const int FiveFlowers = 5;
        private const int SecondLevel = 2;

        private static readonly Flower[] _sideBySideFlowers = new Flower[] { new Flower(new BoardCell(1, 1), 1), new Flower(new BoardCell(1, 2), 1), new Flower(new BoardCell(3, 3), 1), new Flower(new BoardCell(6, 1), 1), new Flower(new BoardCell(6, 6), 1) };
        private static readonly Flower[] _cornerTouchingFlowers = new Flower[] { new Flower(new BoardCell(1, 1), 1), new Flower(new BoardCell(2, 2), 1), new Flower(new BoardCell(4, 4), 1), new Flower(new BoardCell(6, 1), 1), new Flower(new BoardCell(6, 6), 1) };
        private static readonly Flower[] _flowersWithASecondLevel = new Flower[] { new Flower(new BoardCell(1, 1), SecondLevel), new Flower(new BoardCell(1, 6), 1), new Flower(new BoardCell(3, 3), 1), new Flower(new BoardCell(6, 1), 1), new Flower(new BoardCell(6, 6), 1) };
        private static readonly Flower[] _threeFlowers = new Flower[] { new Flower(new BoardCell(1, 1), 1), new Flower(new BoardCell(1, 6), 1), new Flower(new BoardCell(3, 3), 1) };
        private static readonly Flower[] _flowersWithOneOutside = new Flower[] { new Flower(new BoardCell(8, 1), 1), new Flower(new BoardCell(1, 6), 1), new Flower(new BoardCell(3, 3), 1), new Flower(new BoardCell(6, 1), 1), new Flower(new BoardCell(6, 6), 1) };
        private static readonly Flower[] _flowersSharingACell = new Flower[] { new Flower(new BoardCell(1, 1), 1), new Flower(new BoardCell(1, 1), 1), new Flower(new BoardCell(3, 3), 1), new Flower(new BoardCell(6, 1), 1), new Flower(new BoardCell(6, 6), 1) };

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
            Assert.Throws<ArgumentException>(() => new StartingLayout(_sideBySideFlowers));
        }

        [Fact]
        public void Test_Constructor_WithFlowersTouchingOnACorner_KeepsBothFlowers()
        {
            var layout = new StartingLayout(_cornerTouchingFlowers);

            Assert.Equal(FiveFlowers, layout.Flowers.Count);
        }

        [Fact]
        public void Test_Constructor_WithASecondLevelFlower_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new StartingLayout(_flowersWithASecondLevel));
        }

        [Fact]
        public void Test_Constructor_WithThreeFlowers_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new StartingLayout(_threeFlowers));
        }

        [Fact]
        public void Test_Constructor_WithAFlowerOutsideTheBoard_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new StartingLayout(_flowersWithOneOutside));
        }

        [Fact]
        public void Test_Constructor_WithTwoFlowersOnTheSameCell_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new StartingLayout(_flowersSharingACell));
        }
    }
}
