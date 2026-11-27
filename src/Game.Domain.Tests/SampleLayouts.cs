using System.Collections.Generic;

namespace Game.Domain.Tests
{
    internal static class SampleLayouts
    {
        internal const int LevelOne = 1;
        internal const int LevelTwo = 2;
        internal const int LevelThree = 3;
        internal const int FiveFlowers = 5;
        internal const int OneScoredPlanter = 1;

        internal static BoardCell NorthWestFlower { get; } = new BoardCell(1, 1);

        internal static BoardCell NorthEastFlower { get; } = new BoardCell(1, 6);

        internal static BoardCell CenterFlower { get; } = new BoardCell(3, 3);

        internal static BoardCell SouthWestFlower { get; } = new BoardCell(6, 1);

        internal static BoardCell SouthEastFlower { get; } = new BoardCell(6, 6);

        internal static BoardCell GroundCell { get; } = new BoardCell(4, 4);

        internal static BoardCell OutsideCell { get; } = new BoardCell(8, 0);

        internal static IReadOnlyList<Flower> FiveStartingFlowers { get; } = new Flower[] { new Flower(NorthWestFlower, StartingLayout.StartingFlowerLevel), new Flower(NorthEastFlower, StartingLayout.StartingFlowerLevel), new Flower(CenterFlower, StartingLayout.StartingFlowerLevel), new Flower(SouthWestFlower, StartingLayout.StartingFlowerLevel), new Flower(SouthEastFlower, StartingLayout.StartingFlowerLevel) };

        internal static StartingLayout FiveSingleCellPlanters { get; } = new StartingLayout(FiveStartingFlowers);
    }
}
