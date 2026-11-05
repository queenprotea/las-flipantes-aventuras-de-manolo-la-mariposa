using System;
using System.Collections.Generic;

using Game.Contracts;

namespace Game.Services
{
    public sealed class StartingBoard
    {
        public StartingBoard(IReadOnlyList<MatchCell> flowerCells)
        {
            ArgumentNullException.ThrowIfNull(flowerCells);

            FlowerCells = flowerCells;
        }

        public IReadOnlyList<MatchCell> FlowerCells { get; }
    }
}
