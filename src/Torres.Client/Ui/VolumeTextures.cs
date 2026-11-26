using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;

namespace Torres.Client.Ui
{
    internal sealed class VolumeTextures : IDisposable
    {
        private const int ShadowMargin = 4;
        private const float ShadowDrop = 2f;
        private const float ShadowBlur = 4f;
        private const float ShadowOpacity = 0.14f;
        private const float LineWidth = 1f;
        private const float PixelCenter = 0.5f;
        private const float HalfShare = 0.5f;
        private const int CardRadius = 14;
        private const int PillRadius = 12;
        private const int CircleRadius = 13;

        private readonly List<Texture2D> _textures = new List<Texture2D>();

        internal VolumeTextures(GraphicsDevice graphicsDevice)
        {
            CardBrush = CreatePanel(graphicsDevice, new PanelLook(Theme.Surface, Theme.Surface, Theme.Line, CardRadius, true, true));
            PillBrush = CreatePanel(graphicsDevice, new PanelLook(Theme.Surface, Theme.SurfaceSunken, Theme.StrongLine, PillRadius, true, true));
            HoveredPillBrush = CreatePanel(graphicsDevice, new PanelLook(Theme.MintTint, Theme.MintAlt, Theme.MintLine, PillRadius, true, true));
            ChosenPillBrush = CreatePanel(graphicsDevice, new PanelLook(Theme.ChosenActionTop, Theme.ChosenAction, Theme.ChosenActionLine, PillRadius, true, true));
            HintPillBrush = CreatePanel(graphicsDevice, new PanelLook(Theme.HintPill, Theme.HintPill, Theme.HintPill, PillRadius, false, true));
            WinnerPillBrush = CreatePanel(graphicsDevice, new PanelLook(Theme.ButterTint, Theme.Butter, Theme.ButterLine, PillRadius, true, true));
            ActivePillBrush = CreatePanel(graphicsDevice, new PanelLook(Theme.MintTint, Theme.MintAlt, Theme.ChosenAction, PillRadius, true, true));
            CircleBrush = CreatePanel(graphicsDevice, new PanelLook(Theme.Surface, Theme.SurfaceSunken, Theme.StrongLine, CircleRadius, true, false));
            PetalLineImages = CreatePetalImages(graphicsDevice, 0f);
            PetalFillImages = CreatePetalImages(graphicsDevice, FlowerClockShapes.PetalLineShare);
            FlowerCenterImage = new TextureRegion(CreateTexture(graphicsDevice, MatchLayout.FlowerClockSize, FlowerClockShapes.CreateCenter()));
        }

        internal IBrush CardBrush { get; }

        internal IBrush PillBrush { get; }

        internal IBrush HoveredPillBrush { get; }

        internal IBrush ChosenPillBrush { get; }

        internal IBrush HintPillBrush { get; }

        internal IBrush ActivePillBrush { get; }

        internal IBrush WinnerPillBrush { get; }

        internal IBrush CircleBrush { get; }

        internal IReadOnlyList<TextureRegion> PetalLineImages { get; }

        internal IReadOnlyList<TextureRegion> PetalFillImages { get; }

        internal TextureRegion FlowerCenterImage { get; }

        public void Dispose()
        {
            foreach (Texture2D texture in _textures)
            {
                texture.Dispose();
            }
        }

        private static float FindRoundedDistance(Vector2 pixelPoint, Vector2 shapeCenter, Vector2 halfSize, float cornerRadius)
        {
            Vector2 centerOffset = pixelPoint - shapeCenter;
            Vector2 innerOffset = new Vector2(Math.Abs(centerOffset.X), Math.Abs(centerOffset.Y)) - (halfSize - new Vector2(cornerRadius));
            Vector2 cornerOffset = Vector2.Max(Vector2.Zero, innerOffset);
            float roundedDistance = cornerOffset.Length() + Math.Min(Math.Max(innerOffset.X, innerOffset.Y), 0f) - cornerRadius;

            return roundedDistance;
        }

        private static Color BlendOver(Color upperColor, float upperCoverage, Color lowerColor)
        {
            var blendedColor = new Color((upperColor.ToVector4() * upperCoverage) + (lowerColor.ToVector4() * (1f - upperCoverage)));

            return blendedColor;
        }

        private IBrush CreatePanel(GraphicsDevice graphicsDevice, PanelLook panelLook)
        {
            int shadowMargin = panelLook.HasShadow ? ShadowMargin : 0;
            int inset = panelLook.CornerRadius + shadowMargin;
            int textureSize = (inset * 2) + 1;
            var shapeCenter = new Vector2(textureSize * HalfShare);
            var halfSize = new Vector2((textureSize * HalfShare) - shadowMargin);
            var pixels = new Color[textureSize * textureSize];
            for (var row = 0; row < textureSize; row++)
            {
                float verticalShare = (float)row / textureSize;
                Color fillColor = Color.Lerp(panelLook.TopColor, panelLook.BottomColor, verticalShare);
                for (var column = 0; column < textureSize; column++)
                {
                    var pixelPoint = new Vector2(column + PixelCenter, row + PixelCenter);
                    pixels[(row * textureSize) + column] = ShadePixel(pixelPoint, shapeCenter, halfSize, panelLook, fillColor);
                }
            }

            Texture2D panelTexture = CreateTexture(graphicsDevice, textureSize, pixels);
            var panelBrush = new NinePatchRegion(panelTexture, new Rectangle(0, 0, textureSize, textureSize), new Thickness(inset));

            return panelBrush;
        }

        private static Color ShadePixel(Vector2 pixelPoint, Vector2 shapeCenter, Vector2 halfSize, PanelLook panelLook, Color fillColor)
        {
            float shapeDistance = FindRoundedDistance(pixelPoint, shapeCenter, halfSize, panelLook.CornerRadius);
            Color pixelColor = Color.Transparent;
            if (panelLook.HasShadow)
            {
                float shadowDistance = FindRoundedDistance(pixelPoint, shapeCenter + new Vector2(0f, ShadowDrop), halfSize, panelLook.CornerRadius);
                float shadowAlpha = ShadowOpacity * MathHelper.Clamp(HalfShare - (shadowDistance / (ShadowBlur * 2f)), 0f, 1f);
                pixelColor = Theme.PanelShadow * shadowAlpha;
            }

            float fillCoverage = MathHelper.Clamp(HalfShare - shapeDistance, 0f, 1f);
            Color surfaceColor = fillColor;
            if (panelLook.HasLine)
            {
                float innerCoverage = MathHelper.Clamp(HalfShare - (shapeDistance + LineWidth), 0f, 1f);
                surfaceColor = Color.Lerp(panelLook.LineColor, fillColor, innerCoverage);
            }

            Color shadedColor = BlendOver(surfaceColor, fillCoverage, pixelColor);

            return shadedColor;
        }

        private IReadOnlyList<TextureRegion> CreatePetalImages(GraphicsDevice graphicsDevice, float insetShare)
        {
            var petalImages = new List<TextureRegion>();
            for (var petalIndex = 0; petalIndex < FlowerClockShapes.PetalCount; petalIndex++)
            {
                petalImages.Add(new TextureRegion(CreateTexture(graphicsDevice, MatchLayout.FlowerClockSize, FlowerClockShapes.CreatePetalMask(petalIndex, insetShare))));
            }

            return petalImages;
        }

        private Texture2D CreateTexture(GraphicsDevice graphicsDevice, int textureSize, Color[] pixels)
        {
            var texture = new Texture2D(graphicsDevice, textureSize, textureSize);
            texture.SetData(pixels);
            _textures.Add(texture);

            return texture;
        }
    }
}
