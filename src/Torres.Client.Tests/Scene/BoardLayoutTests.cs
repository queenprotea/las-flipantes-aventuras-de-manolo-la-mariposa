using System.Linq;

using Torres.Client.Scene;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class BoardLayoutTests
    {
        private const int CellsOnTheBoard = 64;
        private const int NoFlowerLevel = 0;

        private readonly BoardLayout _layout = SampleBoards.CreateLayout();

        [Fact]
        public void Test_EnumerateCells_OfTheBoard_ReturnsEveryCell()
        {
            int cellCount = BoardLayout.EnumerateCells().Count();

            Assert.Equal(CellsOnTheBoard, cellCount);
        }

        [Fact]
        public void Test_IsPlanter_OfAPlanterCell_ReturnsTrue()
        {
            bool isPlanter = _layout.IsPlanter(SampleBoards.CellOfSecondPlanter);

            Assert.True(isPlanter);
        }

        [Fact]
        public void Test_IsPlanter_OfAGroundCell_ReturnsFalse()
        {
            bool isPlanter = _layout.IsPlanter(SampleBoards.GroundCell);

            Assert.False(isPlanter);
        }

        [Fact]
        public void Test_IsPlanter_OfACellOutsideTheBoard_ReturnsFalse()
        {
            bool isPlanter = _layout.IsPlanter(SampleBoards.OutsideCell);

            Assert.False(isPlanter);
        }

        [Fact]
        public void Test_IsSamePlanter_OfTwoCellsOfOnePlanter_ReturnsTrue()
        {
            bool isSamePlanter = _layout.IsSamePlanter(SampleBoards.FlowerOfFirstPlanter, SampleBoards.LevelZeroCellOfFirstPlanter);

            Assert.True(isSamePlanter);
        }

        [Fact]
        public void Test_IsSamePlanter_OfCellsOfTwoPlanters_ReturnsFalse()
        {
            bool isSamePlanter = _layout.IsSamePlanter(SampleBoards.FlowerOfFirstPlanter, SampleBoards.CellOfSecondPlanter);

            Assert.False(isSamePlanter);
        }

        [Fact]
        public void Test_GetFlowerLevel_OfAFlower_ReturnsItsLevel()
        {
            int flowerLevel = _layout.GetFlowerLevel(SampleBoards.FlowerOfFirstPlanter);

            Assert.Equal(SampleBoards.FlowerLevelTwo, flowerLevel);
        }

        [Fact]
        public void Test_GetFlowerLevel_OfAGroundCell_ReturnsZero()
        {
            int flowerLevel = _layout.GetFlowerLevel(SampleBoards.GroundCell);

            Assert.Equal(NoFlowerLevel, flowerLevel);
        }

        [Fact]
        public void Test_GetFlowerLevel_OfAPlanterCellAtLevelZero_ReturnsZero()
        {
            int flowerLevel = _layout.GetFlowerLevel(SampleBoards.LevelZeroCellOfFirstPlanter);

            Assert.Equal(NoFlowerLevel, flowerLevel);
        }

        [Fact]
        public void Test_GetPlanterNumber_OfTheSecondPlanter_ReturnsTwo()
        {
            int planterNumber = _layout.GetPlanterNumber(SampleBoards.CellOfSecondPlanter);

            Assert.Equal(SampleBoards.SecondPlanterNumber, planterNumber);
        }

        [Fact]
        public void Test_HasCaterpillar_OfACellWithACaterpillar_ReturnsTrue()
        {
            bool hasCaterpillar = _layout.HasCaterpillar(SampleBoards.GroundCaterpillarCell);

            Assert.True(hasCaterpillar);
        }

        [Fact]
        public void Test_HasCaterpillar_OfAnEmptyCell_ReturnsFalse()
        {
            bool hasCaterpillar = _layout.HasCaterpillar(SampleBoards.GroundCell);

            Assert.False(hasCaterpillar);
        }

        [Fact]
        public void Test_IsPlanter_OfTheEmptyLayout_ReturnsFalse()
        {
            bool isPlanter = BoardLayout.Empty.IsPlanter(SampleBoards.CellOfSecondPlanter);

            Assert.False(isPlanter);
        }
    }
}
