using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal readonly record struct PickTarget(BoundingBox Bounds, BoardCell Cell);
}
