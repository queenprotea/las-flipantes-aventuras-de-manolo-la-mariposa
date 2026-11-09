using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Torres.Client.Ui;

namespace Torres.Client.Avatars
{
    internal static class DefaultAvatarArt
    {
        private const int ArtSide = 128;
        private const float HeadCenterX = 64f;
        private const float HeadCenterY = 50f;
        private const float HeadRadius = 24f;
        private const float ShouldersCenterX = 64f;
        private const float ShouldersCenterY = 128f;
        private const float ShouldersRadiusX = 46f;
        private const float ShouldersRadiusY = 44f;
        private const float PixelCenterOffset = 0.5f;
        private const float UnitRadiusSquared = 1f;

        internal static Texture2D Create(GraphicsDevice graphicsDevice)
        {
            var artPixels = new Color[ArtSide * ArtSide];
            for (var pixelIndex = 0; pixelIndex < artPixels.Length; pixelIndex++)
            {
                var pixelCenter = new Vector2((pixelIndex % ArtSide) + PixelCenterOffset, (pixelIndex / ArtSide) + PixelCenterOffset);
                artPixels[pixelIndex] = IsInsideFigure(pixelCenter) ? Theme.Mint : Theme.MintAlt;
            }

            var artTexture = new Texture2D(graphicsDevice, ArtSide, ArtSide);
            artTexture.SetData(artPixels);

            return artTexture;
        }

        private static bool IsInsideFigure(Vector2 pixelCenter)
        {
            float headDistanceSquared = Vector2.DistanceSquared(pixelCenter, new Vector2(HeadCenterX, HeadCenterY));
            float normalizedShouldersX = (pixelCenter.X - ShouldersCenterX) / ShouldersRadiusX;
            float normalizedShouldersY = (pixelCenter.Y - ShouldersCenterY) / ShouldersRadiusY;
            bool isInsideHead = headDistanceSquared <= HeadRadius * HeadRadius;
            bool isInsideShoulders = (normalizedShouldersX * normalizedShouldersX) + (normalizedShouldersY * normalizedShouldersY) <= UnitRadiusSquared;
            bool isInsideFigure = isInsideHead || isInsideShoulders;

            return isInsideFigure;
        }
    }
}
