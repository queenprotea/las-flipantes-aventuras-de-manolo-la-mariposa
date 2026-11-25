using System;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class ConeflowerShapes
    {
        private const float StemRadius = 0.032f;
        private const float BudConeHeight = 0.07f;
        private const float OpeningConeHeight = 0.08f;
        private const float FullBloomConeHeight = 0.1f;
        private const float FirstLeafHeadingDegrees = 20f;
        private const float LeafHeadingStepDegrees = 120f;
        private const float LeafDistance = 0.1f;
        private const float LowestLeafLift = 0.1f;
        private const float LeafLiftStep = 0.25f;
        private const float LeafClearance = 0.05f;
        private const float LeafTiltDegrees = -25f;
        private const float BudLift = 0.04f;
        private const float PetalInnerDistance = 0.06f;
        private const int OpeningPetalCount = 8;
        private const float OpeningPetalLength = 0.06f;
        private const float OpeningPetalDroopDegrees = 80f;
        private const float OpeningPetalLift = 0.02f;
        private const int FullBloomPetalCount = 13;
        private const float FullBloomPetalLength = 0.11f;
        private const float FullBloomPetalDroopDegrees = 112f;
        private const float FullBloomPetalLift = -0.025f;
        private const float RightAngleDegrees = 90f;
        private const float PetalThickness = 0.012f;
        private const float PetalWidth = 0.035f;
        private const float ConeRadius = 0.075f;
        private const float ConeLiftShare = 0.4f;
        private const float ConeHeightShare = 0.7f;

        private static readonly Vector3 _leafScale = new Vector3(0.13f, 0.015f, 0.035f);
        private static readonly Vector3 _budScale = new Vector3(0.06f, 0.05f, 0.06f);
        private static readonly PetalRing _budSepals = new PetalRing(7, 0.04f, new Vector3(0.025f, 0.05f, 0.025f), 40f, 0f, GardenPalette.ConeflowerSepal);

        internal static void AddFlower(MeshBuilder meshBuilder, Vector3 cellCenter, int flowerLevel)
        {
            var flowerPhase = (FlowerPhase)flowerLevel;
            float coneHeight = flowerPhase switch
            {
                FlowerPhase.Bud => BudConeHeight,
                FlowerPhase.Opening => OpeningConeHeight,
                _ => FullBloomConeHeight,
            };
            float headBase = BoardGeometry.GetFlowerTop(flowerLevel) - coneHeight;
            FlowerParts.AddColumn(meshBuilder, cellCenter + new Vector3(0f, BoardGeometry.SoilTop, 0f), headBase - BoardGeometry.SoilTop, StemRadius, GardenPalette.Stem);
            AddLeaves(meshBuilder, cellCenter, flowerLevel, headBase);
            Vector3 headPoint = cellCenter + new Vector3(0f, headBase, 0f);
            switch (flowerPhase)
            {
                case FlowerPhase.Bud:
                    FlowerParts.AddPart(meshBuilder, headPoint + new Vector3(0f, BudLift, 0f), _budScale, 0f, 0f, GardenPalette.ConeflowerBud);
                    FlowerParts.AddRing(meshBuilder, headPoint, _budSepals);
                    break;
                case FlowerPhase.Opening:
                    AddPetals(meshBuilder, headPoint, OpeningPetalCount, OpeningPetalLength, OpeningPetalDroopDegrees, OpeningPetalLift);
                    AddCone(meshBuilder, headPoint, coneHeight);
                    break;
                case FlowerPhase.FullBloom:
                    AddPetals(meshBuilder, headPoint, FullBloomPetalCount, FullBloomPetalLength, FullBloomPetalDroopDegrees, FullBloomPetalLift);
                    AddCone(meshBuilder, headPoint, coneHeight);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flowerLevel));
            }
        }

        private static void AddLeaves(MeshBuilder meshBuilder, Vector3 cellCenter, int flowerLevel, float headBase)
        {
            for (var leafIndex = 0; leafIndex <= flowerLevel; leafIndex++)
            {
                float leafHeight = BoardGeometry.PlanterTop + LowestLeafLift + (LeafLiftStep * leafIndex);
                if (leafHeight >= headBase - LeafClearance)
                {
                    continue;
                }

                float headingDegrees = FirstLeafHeadingDegrees + (LeafHeadingStepDegrees * leafIndex);
                Vector3 leafCenter = cellCenter + (BoardGeometry.GetHeadingDirection(headingDegrees) * LeafDistance) + new Vector3(0f, leafHeight, 0f);
                FlowerParts.AddPart(meshBuilder, leafCenter, _leafScale, LeafTiltDegrees, headingDegrees, GardenPalette.Leaf);
            }
        }

        private static void AddPetals(MeshBuilder meshBuilder, Vector3 headPoint, int petalCount, float petalLength, float droopDegrees, float petalLift)
        {
            var petalScale = new Vector3(petalLength, PetalThickness, PetalWidth);
            float petalDistance = PetalInnerDistance + petalLength;
            for (var petalIndex = 0; petalIndex < petalCount; petalIndex++)
            {
                float headingDegrees = FlowerParts.GetCircleHeading(petalIndex, petalCount, 0f);
                Vector3 petalCenter = headPoint + (BoardGeometry.GetHeadingDirection(headingDegrees) * petalDistance) + new Vector3(0f, petalLift, 0f);
                FlowerParts.AddPart(meshBuilder, petalCenter, petalScale, droopDegrees - RightAngleDegrees, headingDegrees, GardenPalette.ConeflowerPetal);
            }
        }

        private static void AddCone(MeshBuilder meshBuilder, Vector3 headPoint, float coneHeight)
        {
            var coneScale = new Vector3(ConeRadius, coneHeight * ConeHeightShare, ConeRadius);
            FlowerParts.AddPart(meshBuilder, headPoint + new Vector3(0f, coneHeight * ConeLiftShare, 0f), coneScale, 0f, 0f, GardenPalette.ConeflowerCone);
        }
    }
}
