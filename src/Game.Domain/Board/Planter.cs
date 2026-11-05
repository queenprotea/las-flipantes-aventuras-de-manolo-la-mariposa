using System.Collections.Generic;

namespace Game.Domain
{
    public sealed record Planter(IReadOnlyList<Flower> Flowers)
    {
        public int Area => Flowers.Count;
    }
}
