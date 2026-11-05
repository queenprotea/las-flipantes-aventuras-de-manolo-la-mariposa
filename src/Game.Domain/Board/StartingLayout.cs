using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public sealed class StartingLayout
    {
        public const int StartingFlowerLevel = 1;

        public const int MinimumFlowerCount = MatchTable.MaximumSeatCount;

        private const int SingleCellCount = 1;
        private const string TallStartingFlowerMessage = "Every starting flower must be one level high.";
        private const string WidePlanterMessage = "Every starting planter must take a single cell.";

        public StartingLayout(IReadOnlyList<Flower> flowers)
        {
            ArgumentNullException.ThrowIfNull(flowers);
            ArgumentOutOfRangeException.ThrowIfLessThan(flowers.Count, MinimumFlowerCount, nameof(flowers));
            if (flowers.Any(flower => flower.Level != StartingFlowerLevel))
            {
                throw new ArgumentException(TallStartingFlowerMessage, nameof(flowers));
            }

            var startingBoard = new Board(flowers);
            if (startingBoard.FindPlanters().Any(planter => planter.CellCount > SingleCellCount))
            {
                throw new ArgumentException(WidePlanterMessage, nameof(flowers));
            }

            Flowers = startingBoard.FindFlowers();
        }

        public IReadOnlyList<Flower> Flowers { get; }
    }
}
