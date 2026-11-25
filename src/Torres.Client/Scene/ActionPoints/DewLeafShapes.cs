using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class DewLeafShapes
    {
        internal const float LeafLength = 2.1f;

        private const float LeafHalfWidth = 0.46f;
        private const float ArchHeight = 0.16f;
        private const float FoldSlope = 0.55f;
        private const float SurfaceLift = 0.015f;
        private const float WidthShapePower = 0.85f;
        private const float WidthRoundPower = 0.75f;
        private const float ArchPower = 1.2f;
        private const int LengthSteps = 90;
        private const int WidthSteps = 28;
        private const int VariegationSeed = 4;
        private const int SmoothingPasses = 4;
        private const int SmoothingWindow = 3;
        private const float EdgeShadeStart = 0.7f;
        private const float EdgeShadeSpan = 0.3f;
        private const float VariegationStart = 0.1f;
        private const float VariegationEnd = 0.86f;
        private const float VariegationBaseReach = 0.25f;
        private const float VariegationExtraReach = 0.4f;
        private const float VariegationSoftness = 0.08f;
        private const float RibHalfWidth = 0.05f;
        private const float HalfLength = 0.5f;
        private const float StemLength = 0.32f;
        private const float StemRadius = 0.028f;
        private const float StemLift = 0.025f;
        private const float SideSpan = 2f;
        private const float SideStart = -1f;

        internal static Vector3 FindRibPoint(float lengthShare)
        {
            Vector3 ribPoint = FindSurfacePoint(lengthShare, 0f);

            return ribPoint;
        }

        internal static void AddLeaf(MeshBuilder meshBuilder)
        {
            IReadOnlyList<float> variegationReach = CreateVariegationReach();
            var gridPositions = new List<Vector3>();
            var gridColors = new List<Color>();
            for (var lengthIndex = 0; lengthIndex <= LengthSteps; lengthIndex++)
            {
                float lengthShare = (float)lengthIndex / LengthSteps;
                for (var widthIndex = 0; widthIndex <= WidthSteps; widthIndex++)
                {
                    float sideShare = SideStart + (SideSpan * widthIndex / WidthSteps);
                    gridPositions.Add(FindSurfacePoint(lengthShare, sideShare));
                    gridColors.Add(FindLeafColor(lengthShare, Math.Abs(sideShare), variegationReach[lengthIndex]));
                }
            }

            meshBuilder.AddSurfaceGrid(gridPositions, gridColors, WidthSteps + 1);
            Matrix stemTransform = Matrix.CreateScale(StemRadius, StemLength, StemRadius)
                * Matrix.CreateRotationZ(MathHelper.PiOver2)
                * Matrix.CreateTranslation(-LeafLength * HalfLength, StemLift, 0f);
            meshBuilder.AddCylinder(stemTransform, GardenPalette.Stem);
        }

        private static Vector3 FindSurfacePoint(float lengthShare, float sideShare)
        {
            float halfWidth = LeafHalfWidth * MathF.Pow(Math.Max(MathF.Sin(MathF.PI * MathF.Pow(lengthShare, WidthShapePower)), 0f), WidthRoundPower);
            float archLift = ArchHeight * MathF.Pow(MathF.Sin(MathF.PI * lengthShare), ArchPower);
            float foldLift = FoldSlope * Math.Abs(sideShare) * halfWidth;
            var surfacePoint = new Vector3((lengthShare - HalfLength) * LeafLength, archLift + foldLift + SurfaceLift, sideShare * halfWidth);

            return surfacePoint;
        }

        private static Color FindLeafColor(float lengthShare, float sideDistance, float reachFactor)
        {
            Color leafColor = Color.Lerp(GardenPalette.DewLeaf, GardenPalette.DewLeafEdge, Math.Max(0f, (sideDistance - EdgeShadeStart) / EdgeShadeSpan));
            bool isVariegated = (lengthShare > VariegationStart) && (lengthShare < VariegationEnd);
            if (isVariegated)
            {
                float creamReach = (VariegationBaseReach + (VariegationExtraReach * reachFactor)) * MathF.Sin(MathF.PI * (lengthShare - VariegationStart) / (VariegationEnd - VariegationStart));
                if (sideDistance < creamReach)
                {
                    leafColor = Color.Lerp(GardenPalette.DewLeafCream, leafColor, Math.Max(0f, (sideDistance - creamReach + VariegationSoftness) / VariegationSoftness));
                }
            }

            if (sideDistance < RibHalfWidth)
            {
                leafColor = GardenPalette.DewLeafRib;
            }

            return leafColor;
        }

        private static IReadOnlyList<float> CreateVariegationReach()
        {
            var variegationSource = new Random(VariegationSeed);
            var reachFactors = new float[LengthSteps + 1];
            for (var lengthIndex = 0; lengthIndex <= LengthSteps; lengthIndex++)
            {
                reachFactors[lengthIndex] = (float)variegationSource.NextDouble();
            }

            for (var pass = 0; pass < SmoothingPasses; pass++)
            {
                var smoothedFactors = (float[])reachFactors.Clone();
                for (var lengthIndex = 1; lengthIndex < LengthSteps; lengthIndex++)
                {
                    smoothedFactors[lengthIndex] = (reachFactors[lengthIndex - 1] + reachFactors[lengthIndex] + reachFactors[lengthIndex + 1]) / SmoothingWindow;
                }

                reachFactors = smoothedFactors;
            }

            return reachFactors;
        }
    }
}
