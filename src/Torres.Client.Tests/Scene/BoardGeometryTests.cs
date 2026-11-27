using Microsoft.Xna.Framework;

using Torres.Client.Scene;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class BoardGeometryTests
    {
        private const int Precision = 4;
        private const int FlowerLevelOne = 1;
        private const float LevelOneFlowerTop = 0.79f;
        private const float LevelTwoFlowerTop = 1.13f;
        private const float NoHeadingDegrees = 0f;
        private const float ForwardHeadingDegrees = 90f;
        private const float ForwardDepth = -1f;

        private static readonly Vector3 _northWestCellCenter = new Vector3(-3.5f, 0f, -3.5f);
        private static readonly Vector3 _pointOverSecondPlanter = new Vector3(-0.5f, 0f, -0.5f);
        private static readonly Vector3 _pointOutsideTheBoard = new Vector3(10f, 0f, 10f);

        private readonly BoardLayout _layout = SampleBoards.CreateLayout();

        [Fact]
        public void Test_GetCellCenter_OfTheNorthWestCell_ReturnsItsWorldCenter()
        {
            Vector3 cellCenter = BoardGeometry.GetCellCenter(SampleBoards.FlowerOfFirstPlanter);

            Assert.Equal(_northWestCellCenter, cellCenter);
        }

        [Fact]
        public void Test_FindCellAt_OverACell_ReturnsThatCell()
        {
            BoardCell? foundCell = BoardGeometry.FindCellAt(_pointOverSecondPlanter);

            Assert.Equal(SampleBoards.CellOfSecondPlanter, foundCell);
        }

        [Fact]
        public void Test_FindCellAt_OutsideTheBoard_ReturnsNull()
        {
            BoardCell? foundCell = BoardGeometry.FindCellAt(_pointOutsideTheBoard);

            Assert.Null(foundCell);
        }

        [Fact]
        public void Test_GetSurfaceHeight_OfAPlanterCell_ReturnsThePlanterTop()
        {
            float surfaceHeight = BoardGeometry.GetSurfaceHeight(_layout, SampleBoards.CellOfSecondPlanter);

            Assert.Equal(BoardGeometry.PlanterTop, surfaceHeight);
        }

        [Fact]
        public void Test_GetSurfaceHeight_OfAGroundCell_ReturnsTheGrassTop()
        {
            float surfaceHeight = BoardGeometry.GetSurfaceHeight(_layout, SampleBoards.GroundCell);

            Assert.Equal(BoardGeometry.GrassTop, surfaceHeight);
        }

        [Fact]
        public void Test_FindGroundHeightBelow_OverAPlanter_ReturnsTheSoilTop()
        {
            float groundHeight = BoardGeometry.FindGroundHeightBelow(_layout, _pointOverSecondPlanter);

            Assert.Equal(BoardGeometry.SoilTop, groundHeight);
        }

        [Fact]
        public void Test_FindGroundHeightBelow_OutsideTheBoard_ReturnsTheGrassTop()
        {
            float groundHeight = BoardGeometry.FindGroundHeightBelow(_layout, _pointOutsideTheBoard);

            Assert.Equal(BoardGeometry.GrassTop, groundHeight);
        }

        [Fact]
        public void Test_FindFlowerTopPoint_OfALevelTwoFlower_ReturnsTheTopOfItsSecondLevel()
        {
            Vector3 flowerTopPoint = BoardGeometry.FindFlowerTopPoint(_layout, SampleBoards.FlowerOfFirstPlanter);

            Assert.Equal(LevelTwoFlowerTop, flowerTopPoint.Y, Precision);
        }

        [Fact]
        public void Test_FindFlowerTopPoint_OfAPlanterCellAtLevelZero_ReturnsThePlanterTop()
        {
            Vector3 flowerTopPoint = BoardGeometry.FindFlowerTopPoint(_layout, SampleBoards.LevelZeroCellOfFirstPlanter);

            Assert.Equal(BoardGeometry.PlanterTop, flowerTopPoint.Y, Precision);
        }

        [Fact]
        public void Test_GetFlowerTop_OfLevelOne_ReturnsTheTopOfTheFirstLevel()
        {
            float flowerTop = BoardGeometry.GetFlowerTop(FlowerLevelOne);

            Assert.Equal(LevelOneFlowerTop, flowerTop, Precision);
        }

        [Fact]
        public void Test_CreateHeadingRotation_WithoutHeading_ReturnsTheIdentity()
        {
            Matrix headingRotation = BoardGeometry.CreateHeadingRotation(NoHeadingDegrees);

            Assert.Equal(Matrix.Identity, headingRotation);
        }

        [Fact]
        public void Test_GetHeadingDegrees_TowardsTheEast_ReturnsZero()
        {
            float headingDegrees = BoardGeometry.GetHeadingDegrees(Vector3.UnitX);

            Assert.Equal(NoHeadingDegrees, headingDegrees, Precision);
        }

        [Fact]
        public void Test_GetHeadingDegrees_TowardsTheNorth_ReturnsNinety()
        {
            float headingDegrees = BoardGeometry.GetHeadingDegrees(Vector3.Forward);

            Assert.Equal(ForwardHeadingDegrees, headingDegrees, Precision);
        }

        [Fact]
        public void Test_GetHeadingDirection_OfNinetyDegrees_PointsTowardsTheNorth()
        {
            Vector3 headingDirection = BoardGeometry.GetHeadingDirection(ForwardHeadingDegrees);

            Assert.Equal(ForwardDepth, headingDirection.Z, Precision);
        }
    }
}
