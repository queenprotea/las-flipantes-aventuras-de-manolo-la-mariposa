using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal readonly record struct PickVolume(BoundingBox Bounds, BoardCell Cell);
}
