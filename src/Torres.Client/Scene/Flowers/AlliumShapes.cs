using System;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class AlliumShapes
    {
        private const float StemRadius = 0.03f;
        private const int BaseLeafCount = 2;
        private const float LeafPhaseDegrees = 23f;
        private const float LeafDistance = 0.1f;
        private const float LeafLift = 0.1f;
        private const float LeafTiltDegrees = 40f;
        private const float BudRadius = 0.07f;
        private const float OpeningRadius = 0.11f;
        private const float FullBloomRadius = 0.16f;
        private const float BudTipLift = 0.09f;
        private const int OpeningFloretCount = 40;
        private const int FullBloomFloretCount = 90;
        private const float OpeningLowestFraction = -0.1f;
        private const float GoldenAngleDegrees = 137.50776f;
        private const float SphereTopFraction = 1f;
        private const float SphereSpanFraction = 2f;
        private const float HalfStep = 0.5f;
        private const int FloretColorCycle = 3;
        private const float OpeningCupDrop = 0.03f;

        private static readonly Vector3 _leafScale = new Vector3(0.035f, 0.2f, 0.01f);
        private static readonly Vector3 _budScale = new Vector3(0.06f, 0.09f, 0.06f);
        private static readonly Vector3 _budTipScale = new Vector3(0.015f, 0.04f, 0.015f);
        private static readonly Vector3 _floretScale = new Vector3(0.026f, 0.026f, 0.026f);
        private static readonly Vector3 _openingCupScale = new Vector3(0.08f, 0.06f, 0.08f);

        internal static void AddFlower(MeshBuilder meshBuilder, Vector3 cellCenter, int flowerLevel)
        {
            var flowerPhase = (FlowerPhase)flowerLevel;
            float headRadius = flowerPhase switch
            {
                FlowerPhase.Bud => BudRadius,
                FlowerPhase.Opening => OpeningRadius,
                FlowerPhase.FullBloom => FullBloomRadius,
                _ => throw new ArgumentOutOfRangeException(nameof(flowerLevel)),
            };
            float headCenterHeight = BoardGeometry.GetFlowerTop(flowerLevel) - headRadius;
            FlowerParts.AddColumn(meshBuilder, PartPose.CreateUpright(cellCenter + new Vector3(0f, BoardGeometry.SoilTop, 0f), new Vector3(StemRadius, headCenterHeight - BoardGeometry.SoilTop, StemRadius)), GardenPalette.Stem);
            FlowerParts.AddLeafRing(meshBuilder, cellCenter, new LeafRing(BaseLeafCount + flowerLevel, LeafPhaseDegrees, LeafDistance, LeafLift, _leafScale, LeafTiltDegrees, GardenPalette.Leaf, false));
            Vector3 headCenter = cellCenter + new Vector3(0f, headCenterHeight, 0f);
            switch (flowerPhase)
            {
                case FlowerPhase.Bud:
                    FlowerParts.AddPart(meshBuilder, new PartPose(headCenter, _budScale, 0f, 0f), GardenPalette.AlliumBud);
                    FlowerParts.AddPart(meshBuilder, new PartPose(headCenter + new Vector3(0f, BudTipLift, 0f), _budTipScale, 0f, 0f), GardenPalette.AlliumBudTip);
                    break;
                case FlowerPhase.Opening:
                    AddFlorets(meshBuilder, new BoundingSphere(headCenter, headRadius), flowerPhase);
                    FlowerParts.AddPart(meshBuilder, new PartPose(headCenter - new Vector3(0f, OpeningCupDrop, 0f), _openingCupScale, 0f, 0f), GardenPalette.AlliumBud);
                    break;
                case FlowerPhase.FullBloom:
                    AddFlorets(meshBuilder, new BoundingSphere(headCenter, headRadius), flowerPhase);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flowerLevel));
            }
        }

        private static void AddFlorets(MeshBuilder meshBuilder, BoundingSphere head, FlowerPhase flowerPhase)
        {
            bool isOpening = flowerPhase == FlowerPhase.Opening;
            int floretCount = isOpening ? OpeningFloretCount : FullBloomFloretCount;
            float lowestFraction = isOpening ? OpeningLowestFraction : -SphereTopFraction;
            for (var floretIndex = 0; floretIndex < floretCount; floretIndex++)
            {
                float heightFraction = SphereTopFraction - (SphereSpanFraction * (floretIndex + HalfStep) / floretCount);
                if (heightFraction < lowestFraction)
                {
                    continue;
                }

                float ringFraction = MathF.Sqrt((SphereTopFraction * SphereTopFraction) - (heightFraction * heightFraction));
                Vector3 ringDirection = BoardGeometry.GetHeadingDirection(GoldenAngleDegrees * floretIndex);
                Vector3 floretCenter = head.Center + (ringDirection * ringFraction * head.Radius) + new Vector3(0f, heightFraction * head.Radius, 0f);
                Color floretColor = (floretIndex % FloretColorCycle) != 0 ? GardenPalette.AlliumFloret : GardenPalette.AlliumLightFloret;
                FlowerParts.AddCoarsePart(meshBuilder, PartPose.CreateUpright(floretCenter, _floretScale), floretColor);
            }
        }
    }
}
