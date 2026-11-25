using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal static class SceneLighting
    {
        private static readonly Vector3 _unnormalizedLightDirection = new Vector3(-0.45f, -1f, -0.35f);
        private static readonly Vector3 _unnormalizedFillLightDirection = new Vector3(0.55f, -0.45f, 0.6f);

        internal static Vector3 LightDirection { get; } = Vector3.Normalize(_unnormalizedLightDirection);
        internal static Vector3 FillLightDirection { get; } = Vector3.Normalize(_unnormalizedFillLightDirection);
        internal static Vector3 AmbientColor { get; } = new Vector3(0.54f);
        internal static Vector3 DiffuseColor { get; } = new Vector3(0.48f);
        internal static Vector3 FillDiffuseColor { get; } = new Vector3(0.2f);

        internal static void ApplyTo(BasicEffect sceneEffect)
        {
            sceneEffect.AmbientLightColor = AmbientColor;
            sceneEffect.DirectionalLight0.Enabled = true;
            sceneEffect.DirectionalLight0.Direction = LightDirection;
            sceneEffect.DirectionalLight0.DiffuseColor = DiffuseColor;
            sceneEffect.DirectionalLight0.SpecularColor = Vector3.Zero;
            sceneEffect.DirectionalLight1.Enabled = true;
            sceneEffect.DirectionalLight1.Direction = FillLightDirection;
            sceneEffect.DirectionalLight1.DiffuseColor = FillDiffuseColor;
            sceneEffect.DirectionalLight1.SpecularColor = Vector3.Zero;
            sceneEffect.DirectionalLight2.Enabled = false;
        }

        internal static Vector3 GetShadowOffset(float heightAboveSurface)
        {
            float shadowShiftPerHeight = heightAboveSurface / -LightDirection.Y;
            var shadowOffset = new Vector3(LightDirection.X * shadowShiftPerHeight, 0f, LightDirection.Z * shadowShiftPerHeight);

            return shadowOffset;
        }
    }
}
