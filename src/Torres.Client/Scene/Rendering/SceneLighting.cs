using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class SceneLighting
    {
        private static readonly Vector3 _lightSource = new Vector3(-0.45f, -1f, -0.35f);

        internal static Vector3 Direction { get; } = Vector3.Normalize(_lightSource);
        internal static Vector3 AmbientColor { get; } = new Vector3(0.62f);
        internal static Vector3 DiffuseColor { get; } = new Vector3(0.48f);

        internal static Vector3 GetShadowOffset(float heightAboveSurface)
        {
            float travel = heightAboveSurface / -Direction.Y;
            var shadowOffset = new Vector3(Direction.X * travel, 0f, Direction.Z * travel);

            return shadowOffset;
        }
    }
}
