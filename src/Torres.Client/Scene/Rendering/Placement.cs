using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal readonly record struct Placement(Matrix Transform, Color Color)
    {
        internal Matrix NormalTransform { get; } = Matrix.Transpose(Matrix.Invert(Transform));
    }
}
