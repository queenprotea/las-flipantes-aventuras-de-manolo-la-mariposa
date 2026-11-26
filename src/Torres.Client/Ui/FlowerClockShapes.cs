using System;

using Microsoft.Xna.Framework;

namespace Torres.Client.Ui
{
    internal static class FlowerClockShapes
    {
        internal const int PetalCount = 12;
        internal const float PetalLineShare = 0.024f;

        private const int SampleGrid = 4;
        private const float HalfShare = 0.5f;
        private const float StartAngleRadians = -MathHelper.PiOver2;
        private const float PetalInnerShare = 0.12f;
        private const float PetalLengthShare = 0.46f;
        private const float PetalWidthShare = 0.118f;
        private const float PetalTaperPower = 0.8f;
        private const float PetalRoundPower = 0.7f;
        private const float CenterRadiusShare = 0.14f;
        private const float CenterLineShare = 0.03f;
        private const int SeedCount = 7;
        private const float SeedPhaseRadians = 0.3f;
        private const float SeedReachShare = 0.45f;
        private const float SeedRadiusShare = 0.018f;

        internal static Color[] CreatePetalMask(int petalIndex, float insetShare)
        {
            float petalAngleRadians = StartAngleRadians + (MathHelper.TwoPi * petalIndex / PetalCount);
            var petalDirection = new Vector2(MathF.Cos(petalAngleRadians), MathF.Sin(petalAngleRadians));
            Color[] maskPixels = CreatePixels(shapePoint => IsInsidePetal(shapePoint, petalDirection, insetShare) ? Color.White : Color.Transparent);

            return maskPixels;
        }

        internal static Color[] CreateCenter()
        {
            Color[] centerPixels = CreatePixels(FindCenterColor);

            return centerPixels;
        }

        private static Color[] CreatePixels(Func<Vector2, Color> findShapeColor)
        {
            int textureSize = MatchLayout.FlowerClockSize;
            var pixels = new Color[textureSize * textureSize];
            for (var row = 0; row < textureSize; row++)
            {
                for (var column = 0; column < textureSize; column++)
                {
                    pixels[(row * textureSize) + column] = SamplePixel(column, row, findShapeColor);
                }
            }

            return pixels;
        }

        private static Color SamplePixel(int column, int row, Func<Vector2, Color> findShapeColor)
        {
            float textureSize = MatchLayout.FlowerClockSize;
            Vector4 colorSum = Vector4.Zero;
            for (var sampleIndex = 0; sampleIndex < SampleGrid * SampleGrid; sampleIndex++)
            {
                float sampleX = column + (((sampleIndex % SampleGrid) + HalfShare) / SampleGrid);
                float sampleY = row + (((sampleIndex / SampleGrid) + HalfShare) / SampleGrid);
                var shapePoint = new Vector2((sampleX / textureSize) - HalfShare, (sampleY / textureSize) - HalfShare);
                colorSum += findShapeColor(shapePoint).ToVector4();
            }

            var pixelColor = new Color(colorSum / (SampleGrid * SampleGrid));

            return pixelColor;
        }

        private static bool IsInsidePetal(Vector2 shapePoint, Vector2 petalDirection, float insetShare)
        {
            float alongReach = Vector2.Dot(shapePoint, petalDirection);
            float acrossReach = Math.Abs((shapePoint.Y * petalDirection.X) - (shapePoint.X * petalDirection.Y));
            bool isWithinLength = (alongReach >= PetalInnerShare + insetShare) && (alongReach <= PetalLengthShare - insetShare);
            float lengthShare = (alongReach - PetalInnerShare) / (PetalLengthShare - PetalInnerShare);
            float halfWidth = isWithinLength
                ? (PetalWidthShare * MathF.Pow(MathF.Sin(MathHelper.Pi * MathF.Pow(lengthShare, PetalTaperPower)), PetalRoundPower)) - insetShare
                : 0f;
            bool isInside = isWithinLength && (acrossReach <= halfWidth);

            return isInside;
        }

        private static Color FindCenterColor(Vector2 shapePoint)
        {
            float centerDistance = shapePoint.Length();
            Color centerColor = Color.Transparent;
            if (centerDistance <= CenterRadiusShare)
            {
                bool isOnLine = centerDistance >= CenterRadiusShare - CenterLineShare;
                centerColor = (isOnLine || IsOnSeed(shapePoint)) ? Theme.ButterLine : Theme.Butter;
            }

            return centerColor;
        }

        private static bool IsOnSeed(Vector2 shapePoint)
        {
            var isOnSeed = false;
            for (var seedIndex = 0; seedIndex < SeedCount; seedIndex++)
            {
                float seedAngleRadians = SeedPhaseRadians + (MathHelper.TwoPi * seedIndex / SeedCount);
                Vector2 seedCenter = new Vector2(MathF.Cos(seedAngleRadians), MathF.Sin(seedAngleRadians)) * CenterRadiusShare * SeedReachShare;
                isOnSeed = isOnSeed || (Vector2.Distance(shapePoint, seedCenter) <= SeedRadiusShare);
            }

            return isOnSeed;
        }
    }
}
