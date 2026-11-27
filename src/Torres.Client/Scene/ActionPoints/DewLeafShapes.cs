using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class DewLeafShapes
    {
        internal const float LeafLength = 2.1f;
        internal const float PetioleReach = (LeafLength * HalfLength) + StemLength;

        private const float LeafHalfWidth = 0.46f;
        private const float ArchHeight = 0.16f;
        private const float FoldSlope = 0.55f;
        private const float SurfaceLift = 0.015f;
        private const float WidthShapePower = 0.85f;
        private const float WidthRoundPower = 0.75f;
        private const float ArchPower = 1.2f;
        private const int LengthSteps = 90;
        private const int WidthSteps = 28;
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
        private const string SeededVariegationJustification = "The fixed seed gives every leaf the same variegation on every run; the values are not used for security.";

        internal static Vector3 FindRibPoint(float lengthParameter)
        {
            Vector3 ribPoint = FindSurfacePoint(lengthParameter, 0f);

            return ribPoint;
        }

        internal static Vector3 FindSurfacePoint(float lengthParameter, float sideParameter)
        {
            float halfWidth = LeafHalfWidth * MathF.Pow(Math.Max(MathF.Sin(MathF.PI * MathF.Pow(lengthParameter, WidthShapePower)), 0f), WidthRoundPower);
            float archLift = ArchHeight * MathF.Pow(MathF.Sin(MathF.PI * lengthParameter), ArchPower);
            float foldLift = FoldSlope * Math.Abs(sideParameter) * halfWidth;
            var surfacePoint = new Vector3((lengthParameter - HalfLength) * LeafLength, archLift + foldLift + SurfaceLift, sideParameter * halfWidth);

            return surfacePoint;
        }

        internal static void AddLeaf(MeshBuilder meshBuilder, int variegationSeed)
        {
            float[] variegationReach = CreateVariegationReach(variegationSeed);
            var gridPositions = new List<Vector3>();
            var gridColors = new List<Color>();
            for (var lengthIndex = 0; lengthIndex <= LengthSteps; lengthIndex++)
            {
                float lengthParameter = (float)lengthIndex / LengthSteps;
                for (var widthIndex = 0; widthIndex <= WidthSteps; widthIndex++)
                {
                    float sideParameter = SideStart + (SideSpan * widthIndex / WidthSteps);
                    gridPositions.Add(FindSurfacePoint(lengthParameter, sideParameter));
                    gridColors.Add(FindLeafColor(lengthParameter, Math.Abs(sideParameter), variegationReach[lengthIndex]));
                }
            }

            meshBuilder.AddSurfaceGrid(gridPositions, gridColors, WidthSteps + 1);
            Matrix stemTransform = Matrix.CreateScale(StemRadius, StemLength, StemRadius)
                * Matrix.CreateRotationZ(MathHelper.PiOver2)
                * Matrix.CreateTranslation(-LeafLength * HalfLength, StemLift, 0f);
            meshBuilder.AddCylinder(stemTransform, GardenPalette.Stem);
        }

        private static Color FindLeafColor(float lengthParameter, float sideDistance, float reachFactor)
        {
            Color leafColor = Color.Lerp(GardenPalette.DewLeaf, GardenPalette.DewLeafEdge, Math.Max(0f, (sideDistance - EdgeShadeStart) / EdgeShadeSpan));
            bool isVariegated = (lengthParameter > VariegationStart) && (lengthParameter < VariegationEnd);
            if (isVariegated)
            {
                float creamReach = (VariegationBaseReach + (VariegationExtraReach * reachFactor)) * MathF.Sin(MathF.PI * (lengthParameter - VariegationStart) / (VariegationEnd - VariegationStart));
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

        [SuppressMessage("Security", "S2245", Justification = SeededVariegationJustification)]
        private static float[] CreateVariegationReach(int variegationSeed)
        {
            var variegationSource = new Random(variegationSeed);
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
