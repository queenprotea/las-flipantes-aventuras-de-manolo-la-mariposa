using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal readonly record struct Placement(Matrix Transform, Color Color)
    {
        // Computed once per shape and not per vertex: inverting a matrix for every vertex multiplies the load time.
        internal Matrix NormalTransform { get; } = Matrix.Transpose(Matrix.Invert(Transform));
    }
}
