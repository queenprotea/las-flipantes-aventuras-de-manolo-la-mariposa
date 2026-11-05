using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class SceneLighting
    {
        private static readonly Vector3 _unnormalizedLightDirection = new Vector3(-0.45f, -1f, -0.35f);

        internal static Vector3 LightDirection { get; } = Vector3.Normalize(_unnormalizedLightDirection);
        internal static Vector3 AmbientColor { get; } = new Vector3(0.62f);
        internal static Vector3 DiffuseColor { get; } = new Vector3(0.48f);

        internal static Vector3 GetShadowOffset(float heightAboveSurface)
        {
            float shadowShiftPerHeight = heightAboveSurface / -LightDirection.Y;
            var shadowOffset = new Vector3(LightDirection.X * shadowShiftPerHeight, 0f, LightDirection.Z * shadowShiftPerHeight);

            return shadowOffset;
        }
    }
}
