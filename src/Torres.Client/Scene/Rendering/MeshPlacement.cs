using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal readonly record struct MeshPlacement(Matrix Transform, Color Color)
    {
        internal Matrix NormalTransform { get; } = Matrix.Transpose(Matrix.Invert(Transform));
    }
}
