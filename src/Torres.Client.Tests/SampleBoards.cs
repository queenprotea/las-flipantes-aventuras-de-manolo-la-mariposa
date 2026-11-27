using System.Collections.Generic;

using Game.Contracts;

using Torres.Client.Scene;

namespace Torres.Client.Tests
{
    internal static class SampleBoards
    {
        internal const int FlowerLevelTwo = 2;
        internal const int SecondPlanterNumber = 2;

        private const float NoHeadingDegrees = 0f;

        internal static BoardCell FlowerOfFirstPlanter { get; } = new BoardCell(0, 0);

        internal static BoardCell LevelZeroCellOfFirstPlanter { get; } = new BoardCell(1, 0);

        internal static BoardCell CellOfSecondPlanter { get; } = new BoardCell(3, 3);

        internal static BoardCell GroundCell { get; } = new BoardCell(2, 2);

        internal static BoardCell GroundCaterpillarCell { get; } = new BoardCell(5, 5);

        internal static BoardCell OutsideCell { get; } = new BoardCell(-1, 0);

        internal static BoardLayout CreateLayout()
        {
            var planterRows = new List<string> { "AA......", "AA......", "........", "...BB...", "........", "........", "........", "........" };
            var levelRows = new List<string> { "21......", "00......", "........", "...30...", "........", "........", "........", "........" };
            var caterpillars = new List<BoardCaterpillar>
            {
                new BoardCaterpillar(SeatColor.Green, FlowerOfFirstPlanter, NoHeadingDegrees),
                new BoardCaterpillar(SeatColor.Blue, GroundCaterpillarCell, NoHeadingDegrees),
            };
            var layout = new BoardLayout(planterRows, levelRows, caterpillars);

            return layout;
        }
    }
}
