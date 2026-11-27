using Microsoft.Xna.Framework;

using Torres.Client.Scene;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class CellPickerTests
    {
        private const float RayStartHeight = 10f;

        private readonly CellPicker _cellPicker = new CellPicker(SampleBoards.CreateLayout());

        [Fact]
        public void Test_PickCell_WithARayDownOntoTheGround_ReturnsTheCellBelow()
        {
            Ray pickRay = CreateRayDownOnto(SampleBoards.GroundCell);

            BoardCell? pickedCell = _cellPicker.PickCell(pickRay);

            Assert.Equal(SampleBoards.GroundCell, pickedCell);
        }

        [Fact]
        public void Test_PickCell_WithARayDownOntoAFlower_ReturnsTheFlowerCell()
        {
            Ray pickRay = CreateRayDownOnto(SampleBoards.FlowerOfFirstPlanter);

            BoardCell? pickedCell = _cellPicker.PickCell(pickRay);

            Assert.Equal(SampleBoards.FlowerOfFirstPlanter, pickedCell);
        }

        [Fact]
        public void Test_PickCell_WithARayDownOntoACaterpillarOnTheGround_ReturnsItsCell()
        {
            Ray pickRay = CreateRayDownOnto(SampleBoards.GroundCaterpillarCell);

            BoardCell? pickedCell = _cellPicker.PickCell(pickRay);

            Assert.Equal(SampleBoards.GroundCaterpillarCell, pickedCell);
        }

        [Fact]
        public void Test_PickCell_WithARayPointingUp_ReturnsNull()
        {
            var pickRay = new Ray(BoardGeometry.GetCellCenter(SampleBoards.GroundCell) + (Vector3.Up * RayStartHeight), Vector3.Up);

            BoardCell? pickedCell = _cellPicker.PickCell(pickRay);

            Assert.Null(pickedCell);
        }

        private static Ray CreateRayDownOnto(BoardCell targetCell)
        {
            Vector3 rayStart = BoardGeometry.GetCellCenter(targetCell) + (Vector3.Up * RayStartHeight);
            var downwardRay = new Ray(rayStart, Vector3.Down);

            return downwardRay;
        }
    }
}
