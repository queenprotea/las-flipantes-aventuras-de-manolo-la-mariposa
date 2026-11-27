using System;

using Microsoft.Xna.Framework;

namespace Torres.Client.UI
{
    internal static class FlowerClockShapes
    {
        internal const int PetalCount = 12;
        internal const float PetalLineFraction = 0.024f;

        private const int SamplesPerSide = 4;
        private const float SampleCenterOffset = 0.5f;
        private const float TextureCenterOffset = 0.5f;
        private const float StartAngleRadians = -MathHelper.PiOver2;
        private const float PetalInnerFraction = 0.12f;
        private const float PetalLengthFraction = 0.46f;
        private const float PetalWidthFraction = 0.118f;
        private const float PetalTaperPower = 0.8f;
        private const float PetalRoundPower = 0.7f;
        private const float CenterRadiusFraction = 0.14f;
        private const float CenterLineFraction = 0.03f;
        private const int SeedCount = 7;
        private const float SeedPhaseRadians = 0.3f;
        private const float SeedReachFraction = 0.45f;
        private const float SeedRadiusFraction = 0.018f;

        internal static Color[] CreatePetalMask(int petalIndex, float insetFraction)
        {
            float petalAngleRadians = StartAngleRadians + (MathHelper.TwoPi * petalIndex / PetalCount);
            var petalDirection = new Vector2(MathF.Cos(petalAngleRadians), MathF.Sin(petalAngleRadians));
            Color[] maskPixels = CreatePixels(new PetalMask(petalDirection, insetFraction));

            return maskPixels;
        }

        internal static Color[] CreateCenter()
        {
            Color[] centerPixels = CreatePixels(null);

            return centerPixels;
        }

        private static Color[] CreatePixels(PetalMask? petalMask)
        {
            int textureSize = MatchLayout.FlowerClockSize;
            var pixels = new Color[textureSize * textureSize];
            for (var row = 0; row < textureSize; row++)
            {
                for (var column = 0; column < textureSize; column++)
                {
                    pixels[(row * textureSize) + column] = SamplePixel(column, row, petalMask);
                }
            }

            return pixels;
        }

        private static Color SamplePixel(int column, int row, PetalMask? petalMask)
        {
            float textureSize = MatchLayout.FlowerClockSize;
            Vector4 colorSum = Vector4.Zero;
            for (var sampleIndex = 0; sampleIndex < SamplesPerSide * SamplesPerSide; sampleIndex++)
            {
                float sampleX = column + (((sampleIndex % SamplesPerSide) + SampleCenterOffset) / SamplesPerSide);
                float sampleY = row + (((sampleIndex / SamplesPerSide) + SampleCenterOffset) / SamplesPerSide);
                var shapePoint = new Vector2((sampleX / textureSize) - TextureCenterOffset, (sampleY / textureSize) - TextureCenterOffset);
                colorSum += FindShapeColor(shapePoint, petalMask).ToVector4();
            }

            var pixelColor = new Color(colorSum / (SamplesPerSide * SamplesPerSide));

            return pixelColor;
        }

        private static Color FindShapeColor(Vector2 shapePoint, PetalMask? petalMask)
        {
            Color shapeColor;
            if (petalMask is PetalMask shownPetal)
            {
                shapeColor = IsInsidePetal(shapePoint, shownPetal) ? Color.White : Color.Transparent;
            }
            else
            {
                shapeColor = FindCenterColor(shapePoint);
            }

            return shapeColor;
        }

        private static bool IsInsidePetal(Vector2 shapePoint, PetalMask petalMask)
        {
            Vector2 petalDirection = petalMask.Direction;
            float insetFraction = petalMask.InsetFraction;
            float alongReach = Vector2.Dot(shapePoint, petalDirection);
            float acrossReach = Math.Abs((shapePoint.Y * petalDirection.X) - (shapePoint.X * petalDirection.Y));
            bool isWithinLength = (alongReach >= PetalInnerFraction + insetFraction) && (alongReach <= PetalLengthFraction - insetFraction);
            float lengthFraction = (alongReach - PetalInnerFraction) / (PetalLengthFraction - PetalInnerFraction);
            float halfWidth = isWithinLength
                ? (PetalWidthFraction * MathF.Pow(MathF.Sin(MathHelper.Pi * MathF.Pow(lengthFraction, PetalTaperPower)), PetalRoundPower)) - insetFraction
                : 0f;
            bool isInside = isWithinLength && (acrossReach <= halfWidth);

            return isInside;
        }

        private static Color FindCenterColor(Vector2 shapePoint)
        {
            float centerDistance = shapePoint.Length();
            Color centerColor = Color.Transparent;
            if (centerDistance <= CenterRadiusFraction)
            {
                bool isOnLine = centerDistance >= CenterRadiusFraction - CenterLineFraction;
                centerColor = (isOnLine || IsOnSeed(shapePoint)) ? Theme.ButterLine : Theme.Butter;
            }

            return centerColor;
        }

        private static bool IsOnSeed(Vector2 shapePoint)
        {
            bool isOnSeed = false;
            for (var seedIndex = 0; seedIndex < SeedCount; seedIndex++)
            {
                float seedAngleRadians = SeedPhaseRadians + (MathHelper.TwoPi * seedIndex / SeedCount);
                Vector2 seedCenter = new Vector2(MathF.Cos(seedAngleRadians), MathF.Sin(seedAngleRadians)) * CenterRadiusFraction * SeedReachFraction;
                isOnSeed = isOnSeed || (Vector2.Distance(shapePoint, seedCenter) <= SeedRadiusFraction);
            }

            return isOnSeed;
        }
    }
}
