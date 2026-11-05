using System.Collections.Generic;

namespace Game.Domain
{
    public sealed record Planter(IReadOnlyList<Flower> Flowers)
    {
        public int CellCount => Flowers.Count;
    }
}
