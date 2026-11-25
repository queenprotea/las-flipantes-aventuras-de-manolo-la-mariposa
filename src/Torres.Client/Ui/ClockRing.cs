using System;

using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace Torres.Client.Ui
{
    internal sealed class ClockRing : Widget
    {
        private const int DotCount = 36;
        private const int DotSize = 5;
        private const float HalfShare = 0.5f;
        private const float StartAngleRadians = -MathHelper.PiOver2;

        private readonly TextureRegion _dotImage;

        internal ClockRing(TextureRegion dotImage, int ringSize)
        {
            _dotImage = dotImage;
            Width = ringSize;
            Height = ringSize;
        }

        internal float RemainingShare { get; set; }

        public override void InternalRender(RenderContext context)
        {
            Rectangle ringBounds = ActualBounds;
            var ringCenter = new Vector2(ringBounds.X + (ringBounds.Width * HalfShare), ringBounds.Y + (ringBounds.Height * HalfShare));
            float ringRadius = (Math.Min(ringBounds.Width, ringBounds.Height) - DotSize) * HalfShare;
            int remainingDots = (int)MathF.Ceiling(MathHelper.Clamp(RemainingShare, 0f, 1f) * DotCount);
            for (var dotIndex = 0; dotIndex < DotCount; dotIndex++)
            {
                float angleRadians = StartAngleRadians + (MathHelper.TwoPi * dotIndex / DotCount);
                Vector2 dotCenter = ringCenter + (new Vector2(MathF.Cos(angleRadians), MathF.Sin(angleRadians)) * ringRadius);
                var dotBounds = new Rectangle((int)(dotCenter.X - (DotSize * HalfShare)), (int)(dotCenter.Y - (DotSize * HalfShare)), DotSize, DotSize);
                Color dotColor = dotIndex < remainingDots ? Theme.ChosenAction : Theme.ClockTrack;
                _dotImage.Draw(context, dotBounds, dotColor);
            }
        }
    }
}
