using System.Collections.Generic;

namespace Game.Domain.Tests
{
    internal static class SampleLayouts
    {
        internal static BoardCell NorthWestFlower { get; } = new BoardCell(1, 1);

        internal static BoardCell NorthEastFlower { get; } = new BoardCell(1, 6);

        internal static BoardCell CenterFlower { get; } = new BoardCell(3, 3);

        internal static BoardCell SouthWestFlower { get; } = new BoardCell(6, 1);

        internal static BoardCell SouthEastFlower { get; } = new BoardCell(6, 6);

        internal static BoardCell GroundCell { get; } = new BoardCell(4, 4);

        internal static BoardCell OutsideCell { get; } = new BoardCell(8, 0);

        internal static IReadOnlyList<Flower> FiveStartingFlowers { get; } = new Flower[] { new Flower(NorthWestFlower, StartingLayout.StartingFlowerHeight), new Flower(NorthEastFlower, StartingLayout.StartingFlowerHeight), new Flower(CenterFlower, StartingLayout.StartingFlowerHeight), new Flower(SouthWestFlower, StartingLayout.StartingFlowerHeight), new Flower(SouthEastFlower, StartingLayout.StartingFlowerHeight) };

        internal static StartingLayout FiveSingleCellPlanters { get; } = new StartingLayout(FiveStartingFlowers);
    }
}
