using System;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class NarcissusShapes
    {
        private const float HeadDrop = 0.08f;
        private const float StemRadius = 0.03f;
        private const int BaseLeafCount = 2;
        private const float LeafPhaseDegrees = 63f;
        private const float LeafDistance = 0.07f;
        private const float LeafLift = 0.13f;
        private const float LeafTiltDegrees = 18f;
        private const float BudLift = -0.02f;
        private const float OpeningDrop = 0.08f;
        private const float BudTiltDegrees = 20f;
        private const float OpeningCupHeight = 0.06f;
        private const float OpeningCupRadius = 0.035f;
        private const int FullBloomPetalCount = 6;
        private const float FullBloomPetalDistance = 0.12f;
        private const float FullBloomPetalLift = 0.03f;
        private const float FullBloomPetalTiltDegrees = -10f;
        private const float FullBloomCupHeight = 0.09f;
        private const float FullBloomCupRadius = 0.05f;

        private static readonly Vector3 _leafScale = new Vector3(0.03f, 0.22f, 0.012f);
        private static readonly Vector3 _budScale = new Vector3(0.065f, 0.12f, 0.065f);
        private static readonly Vector3 _fullBloomPetalScale = new Vector3(0.12f, 0.014f, 0.065f);
        private static readonly Vector3 _cupRimScale = new Vector3(0.058f, 0.012f, 0.058f);
        private static readonly PetalRing _openingPetals = new PetalRing(6, 0.03f, new Vector3(0.06f, 0.1f, 0.035f), 38f, 0f, GardenPalette.NarcissusPetal);

        internal static void AddFlower(MeshBuilder meshBuilder, Vector3 cellCenter, int flowerLevel)
        {
            var flowerPhase = (FlowerPhase)flowerLevel;
            float headHeight = BoardGeometry.GetFlowerTop(flowerLevel) - HeadDrop;
            FlowerParts.AddColumn(meshBuilder, PartPose.CreateUpright(cellCenter + new Vector3(0f, BoardGeometry.SoilTop, 0f), new Vector3(StemRadius, headHeight - BoardGeometry.SoilTop, StemRadius)), GardenPalette.Stem);
            FlowerParts.AddLeafRing(meshBuilder, cellCenter, new LeafRing(BaseLeafCount + flowerLevel, LeafPhaseDegrees, LeafDistance, LeafLift, _leafScale, LeafTiltDegrees, GardenPalette.Leaf, false));
            Vector3 headPoint = cellCenter + new Vector3(0f, headHeight, 0f);
            switch (flowerPhase)
            {
                case FlowerPhase.Bud:
                    FlowerParts.AddPart(meshBuilder, new PartPose(headPoint + new Vector3(0f, BudLift, 0f), _budScale, BudTiltDegrees, 0f), GardenPalette.NarcissusBud);
                    break;
                case FlowerPhase.Opening:
                    Vector3 openingPoint = headPoint - new Vector3(0f, OpeningDrop, 0f);
                    FlowerParts.AddRing(meshBuilder, openingPoint, _openingPetals);
                    FlowerParts.AddColumn(meshBuilder, PartPose.CreateUpright(openingPoint, new Vector3(OpeningCupRadius, OpeningCupHeight, OpeningCupRadius)), GardenPalette.NarcissusCup);
                    break;
                case FlowerPhase.FullBloom:
                    AddFullBloom(meshBuilder, headPoint);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flowerLevel));
            }
        }

        private static void AddFullBloom(MeshBuilder meshBuilder, Vector3 headPoint)
        {
            for (var petalIndex = 0; petalIndex < FullBloomPetalCount; petalIndex++)
            {
                float headingDegrees = FlowerParts.GetCircleHeadingDegrees(petalIndex, FullBloomPetalCount, 0f);
                Vector3 petalCenter = headPoint + (BoardGeometry.GetHeadingDirection(headingDegrees) * FullBloomPetalDistance) + new Vector3(0f, FullBloomPetalLift, 0f);
                FlowerParts.AddPart(meshBuilder, new PartPose(petalCenter, _fullBloomPetalScale, FullBloomPetalTiltDegrees, headingDegrees), GardenPalette.NarcissusPetal);
            }

            FlowerParts.AddColumn(meshBuilder, PartPose.CreateUpright(headPoint, new Vector3(FullBloomCupRadius, FullBloomCupHeight, FullBloomCupRadius)), GardenPalette.NarcissusCup);
            FlowerParts.AddPart(meshBuilder, new PartPose(headPoint + new Vector3(0f, FullBloomCupHeight, 0f), _cupRimScale, 0f, 0f), GardenPalette.NarcissusCupRim);
        }
    }
}
