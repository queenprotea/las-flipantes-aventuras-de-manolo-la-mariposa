using System;

using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

namespace Torres.Client.UI
{
    internal sealed class FlowerClock : Widget
    {
        private const float FallingStepProgress = 0.25f;
        private const float WitherSpeedFactor = 1.4f;
        private const float FallenOpacityLoss = 0.75f;
        private const int FallSidewaysOffset = 3;
        private const int FallDownwardOffset = 9;
        private const int LastPetalsLeft = 1;

        private readonly VolumeTextures _volumeTextures;

        private int _petalsLeft;
        private float _stepProgress;
        private bool _isLastPetal;

        internal FlowerClock(VolumeTextures volumeTextures)
        {
            _volumeTextures = volumeTextures;
            Width = MatchLayout.FlowerClockSize;
            Height = MatchLayout.FlowerClockSize;
        }

        internal float RemainingProgress { get; set; }

        public override void InternalRender(RenderContext context)
        {
            float remainingProgress = MathHelper.Clamp(RemainingProgress, 0f, 1f);
            float remainingPetals = remainingProgress * FlowerClockShapes.PetalCount;
            _petalsLeft = (int)MathF.Ceiling(remainingPetals);
            _stepProgress = _petalsLeft - remainingPetals;
            _isLastPetal = _petalsLeft == LastPetalsLeft;
            for (var petalIndex = 0; petalIndex < _petalsLeft; petalIndex++)
            {
                DrawStandingPetal(context, petalIndex);
            }

            bool isPetalFalling = (remainingProgress > 0f) && (_petalsLeft < FlowerClockShapes.PetalCount) && (_stepProgress < FallingStepProgress);
            if (isPetalFalling)
            {
                DrawFallingPetal(context, _petalsLeft, _stepProgress / FallingStepProgress);
            }

            _volumeTextures.FlowerCenterImage.Draw(context, ActualBounds, Color.White);
        }

        private Color FindPetalColor(int petalIndex)
        {
            Color petalColor = _isLastPetal ? Theme.ClockPetalLast : Theme.ClockPetal;
            bool isNextToFall = (petalIndex == _petalsLeft - 1) && !_isLastPetal;
            if (isNextToFall)
            {
                petalColor = Color.Lerp(Theme.ClockPetal, Theme.ClockPetalWithered, Math.Min(1f, _stepProgress * WitherSpeedFactor));
            }

            return petalColor;
        }

        private void DrawStandingPetal(RenderContext context, int petalIndex)
        {
            Color lineColor = _isLastPetal ? Theme.ClockPetalLastLine : Theme.ClockPetalLine;
            _volumeTextures.PetalLineImages[petalIndex].Draw(context, ActualBounds, lineColor);
            _volumeTextures.PetalFillImages[petalIndex].Draw(context, ActualBounds, FindPetalColor(petalIndex));
        }

        private void DrawFallingPetal(RenderContext context, int petalIndex, float fallProgress)
        {
            Rectangle fallBounds = ActualBounds;
            fallBounds.Offset((int)(FallSidewaysOffset * fallProgress), (int)(FallDownwardOffset * fallProgress));
            float fallOpacity = 1f - (fallProgress * FallenOpacityLoss);
            _volumeTextures.PetalLineImages[petalIndex].Draw(context, fallBounds, Theme.ClockPetalLine * fallOpacity);
            _volumeTextures.PetalFillImages[petalIndex].Draw(context, fallBounds, Theme.ClockPetalWithered * fallOpacity);
        }
    }
}
