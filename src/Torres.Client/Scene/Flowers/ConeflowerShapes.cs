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
        private const float ConeLiftFraction = 0.4f;
        private const float ConeHeightFraction = 0.7f;

        private static readonly Vector3 _leafScale = new Vector3(0.13f, 0.015f, 0.035f);
        private static readonly Vector3 _budScale = new Vector3(0.06f, 0.05f, 0.06f);
        private static readonly PetalRing _openingPetals = new PetalRing(OpeningPetalCount, PetalInnerDistance + OpeningPetalLength, new Vector3(OpeningPetalLength, PetalThickness, PetalWidth), OpeningPetalDroopDegrees - RightAngleDegrees, 0f, GardenPalette.ConeflowerPetal);
        private static readonly PetalRing _fullBloomPetals = new PetalRing(FullBloomPetalCount, PetalInnerDistance + FullBloomPetalLength, new Vector3(FullBloomPetalLength, PetalThickness, PetalWidth), FullBloomPetalDroopDegrees - RightAngleDegrees, 0f, GardenPalette.ConeflowerPetal);
        private static readonly PetalRing _budSepals = new PetalRing(7, 0.04f, new Vector3(0.025f, 0.05f, 0.025f), 40f, 0f, GardenPalette.ConeflowerSepal);

        internal static void AddFlower(MeshBuilder meshBuilder, Vector3 cellCenter, int flowerLevel)
        {
            var flowerPhase = (FlowerPhase)flowerLevel;
            float coneHeight = flowerPhase switch
            {
                FlowerPhase.Bud => BudConeHeight,
                FlowerPhase.Opening => OpeningConeHeight,
                FlowerPhase.FullBloom => FullBloomConeHeight,
                _ => throw new ArgumentOutOfRangeException(nameof(flowerLevel)),
            };
            float headBase = BoardGeometry.GetFlowerTop(flowerLevel) - coneHeight;
            FlowerParts.AddColumn(meshBuilder, PartPose.CreateUpright(cellCenter + new Vector3(0f, BoardGeometry.SoilTop, 0f), new Vector3(StemRadius, headBase - BoardGeometry.SoilTop, StemRadius)), GardenPalette.Stem);
            Vector3 headPoint = cellCenter + new Vector3(0f, headBase, 0f);
            AddLeaves(meshBuilder, headPoint, flowerLevel);
            switch (flowerPhase)
            {
                case FlowerPhase.Bud:
                    FlowerParts.AddPart(meshBuilder, new PartPose(headPoint + new Vector3(0f, BudLift, 0f), _budScale, 0f, 0f), GardenPalette.ConeflowerBud);
                    FlowerParts.AddRing(meshBuilder, headPoint, _budSepals);
                    break;
                case FlowerPhase.Opening:
                    AddPetals(meshBuilder, headPoint + new Vector3(0f, OpeningPetalLift, 0f), _openingPetals);
                    AddCone(meshBuilder, headPoint, coneHeight);
                    break;
                case FlowerPhase.FullBloom:
                    AddPetals(meshBuilder, headPoint + new Vector3(0f, FullBloomPetalLift, 0f), _fullBloomPetals);
                    AddCone(meshBuilder, headPoint, coneHeight);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flowerLevel));
            }
        }

        private static void AddLeaves(MeshBuilder meshBuilder, Vector3 headPoint, int flowerLevel)
        {
            var cellCenter = new Vector3(headPoint.X, 0f, headPoint.Z);
            float headBase = headPoint.Y;
            for (var leafIndex = 0; leafIndex <= flowerLevel; leafIndex++)
            {
                float leafHeight = BoardGeometry.PlanterTop + LowestLeafLift + (LeafLiftStep * leafIndex);
                if (leafHeight >= headBase - LeafClearance)
                {
                    continue;
                }

                float headingDegrees = FirstLeafHeadingDegrees + (LeafHeadingStepDegrees * leafIndex);
                Vector3 leafCenter = cellCenter + (BoardGeometry.GetHeadingDirection(headingDegrees) * LeafDistance) + new Vector3(0f, leafHeight, 0f);
                FlowerParts.AddPart(meshBuilder, new PartPose(leafCenter, _leafScale, LeafTiltDegrees, headingDegrees), GardenPalette.Leaf);
            }
        }

        private static void AddPetals(MeshBuilder meshBuilder, Vector3 petalAxisPoint, PetalRing petalRing)
        {
            for (var petalIndex = 0; petalIndex < petalRing.PetalCount; petalIndex++)
            {
                float headingDegrees = FlowerParts.GetCircleHeadingDegrees(petalIndex, petalRing.PetalCount, petalRing.PhaseDegrees);
                Vector3 petalCenter = petalAxisPoint + (BoardGeometry.GetHeadingDirection(headingDegrees) * petalRing.Radius);
                FlowerParts.AddPart(meshBuilder, new PartPose(petalCenter, petalRing.PetalScale, petalRing.TiltDegrees, headingDegrees), petalRing.Color);
            }
        }

        private static void AddCone(MeshBuilder meshBuilder, Vector3 headPoint, float coneHeight)
        {
            var coneScale = new Vector3(ConeRadius, coneHeight * ConeHeightFraction, ConeRadius);
            FlowerParts.AddPart(meshBuilder, new PartPose(headPoint + new Vector3(0f, coneHeight * ConeLiftFraction, 0f), coneScale, 0f, 0f), GardenPalette.ConeflowerCone);
        }
    }
}
