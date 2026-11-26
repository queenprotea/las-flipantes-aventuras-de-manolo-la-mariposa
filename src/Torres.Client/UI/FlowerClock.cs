using System;

using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

namespace Torres.Client.UI
{
    internal sealed class FlowerClock : Widget
    {
        private const float FallingStepShare = 0.25f;
        private const float WitherPace = 1.4f;
        private const float FallenOpacityLoss = 0.75f;
        private const int FallDrift = 3;
        private const int FallDrop = 9;
        private const int LastPetalCount = 1;

        private readonly VolumeTextures _volumeTextures;

        internal FlowerClock(VolumeTextures volumeTextures)
        {
            _volumeTextures = volumeTextures;
            Width = MatchLayout.FlowerClockSize;
            Height = MatchLayout.FlowerClockSize;
        }

        internal float RemainingShare { get; set; }

        public override void InternalRender(RenderContext context)
        {
            float remainingShare = MathHelper.Clamp(RemainingShare, 0f, 1f);
            float remainingPetals = remainingShare * FlowerClockShapes.PetalCount;
            var petalsLeft = (int)MathF.Ceiling(remainingPetals);
            float stepProgress = petalsLeft - remainingPetals;
            bool isLastPetal = petalsLeft == LastPetalCount;
            for (var petalIndex = 0; petalIndex < petalsLeft; petalIndex++)
            {
                DrawPetal(context, petalIndex, ActualBounds, FindPetalColor(petalIndex, petalsLeft, stepProgress, isLastPetal), isLastPetal ? Theme.ClockPetalLastLine : Theme.ClockPetalLine);
            }

            bool isPetalFalling = (remainingShare > 0f) && (petalsLeft < FlowerClockShapes.PetalCount) && (stepProgress < FallingStepShare);
            if (isPetalFalling)
            {
                DrawFallingPetal(context, petalsLeft, stepProgress / FallingStepShare);
            }

            _volumeTextures.FlowerCenterImage.Draw(context, ActualBounds, Color.White);
        }

        private static Color FindPetalColor(int petalIndex, int petalsLeft, float stepProgress, bool isLastPetal)
        {
            Color petalColor = isLastPetal ? Theme.ClockPetalLast : Theme.ClockPetal;
            bool isNextToFall = (petalIndex == petalsLeft - 1) && !isLastPetal;
            if (isNextToFall)
            {
                petalColor = Color.Lerp(Theme.ClockPetal, Theme.ClockPetalWithered, Math.Min(1f, stepProgress * WitherPace));
            }

            return petalColor;
        }

        private void DrawFallingPetal(RenderContext context, int petalIndex, float fallShare)
        {
            Rectangle fallBounds = ActualBounds;
            fallBounds.Offset((int)(FallDrift * fallShare), (int)(FallDrop * fallShare));
            float fallOpacity = 1f - (fallShare * FallenOpacityLoss);
            DrawPetal(context, petalIndex, fallBounds, Theme.ClockPetalWithered * fallOpacity, Theme.ClockPetalLine * fallOpacity);
        }

        private void DrawPetal(RenderContext context, int petalIndex, Rectangle petalBounds, Color fillColor, Color lineColor)
        {
            _volumeTextures.PetalLineImages[petalIndex].Draw(context, petalBounds, lineColor);
            _volumeTextures.PetalFillImages[petalIndex].Draw(context, petalBounds, fillColor);
        }
    }
}
