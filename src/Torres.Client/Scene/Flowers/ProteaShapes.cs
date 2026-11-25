using System;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class ProteaShapes
    {
        private const float StemRadius = 0.035f;
        private const float StemOverlap = 0.03f;
        private const float HeadHeight = 0.22f;
        private const float FullBloomHeadHeight = 0.24f;
        private const float FirstLeafHeadingDegrees = 40f;
        private const float LeafHeadingStepDegrees = 137f;
        private const float LeafDistance = 0.09f;
        private const float LowestLeafLift = 0.12f;
        private const float LeafSpanMargin = 0.15f;
        private const float LeafTiltDegrees = -35f;
        private const float BudTipLift = 0.14f;
        private const float OpeningHeartLift = 0.12f;
        private const float OuterBractDrop = 0.02f;
        private const float DomeLift = 0.1f;
        private const int FloretCount = 16;
        private const int FloretRowCount = 2;
        private const float InnerFloretDistance = 0.06f;
        private const float FloretRowSpacing = 0.035f;
        private const float FloretLift = 0.19f;
        private const float FloretRowDrop = 0.03f;

        private static readonly Vector3 _leafScale = new Vector3(0.11f, 0.022f, 0.05f);
        private static readonly Vector3 _budTipScale = new Vector3(0.05f, 0.08f, 0.05f);
        private static readonly Vector3 _openingHeartScale = new Vector3(0.075f, 0.07f, 0.075f);
        private static readonly Vector3 _domeScale = new Vector3(0.12f, 0.11f, 0.12f);
        private static readonly Vector3 _floretScale = new Vector3(0.014f, 0.03f, 0.014f);
        private static readonly PetalRing _budBracts = new PetalRing(8, 0.03f, new Vector3(0.04f, 0.1f, 0.04f), 8f, 0f, GardenPalette.ProteaBudBract);
        private static readonly PetalRing _openingBracts = new PetalRing(10, 0.05f, new Vector3(0.045f, 0.12f, 0.045f), 28f, 0f, GardenPalette.ProteaOpeningBract);
        private static readonly PetalRing _outerBracts = new PetalRing(9, 0.07f, new Vector3(0.034f, 0.16f, 0.045f), 55f, 20f, GardenPalette.ProteaOuterBract);
        private static readonly PetalRing _innerBracts = new PetalRing(11, 0.07f, new Vector3(0.032f, 0.16f, 0.04f), 33f, 0f, GardenPalette.ProteaInnerBract);

        internal static void AddFlower(MeshBuilder meshBuilder, Vector3 cellCenter, int flowerLevel)
        {
            var flowerPhase = (FlowerPhase)flowerLevel;
            float headHeight = flowerPhase == FlowerPhase.FullBloom ? FullBloomHeadHeight : HeadHeight;
            float headBase = BoardGeometry.GetFlowerTop(flowerLevel) - headHeight;
            FlowerParts.AddColumn(meshBuilder, cellCenter + new Vector3(0f, BoardGeometry.SoilTop, 0f), headBase + StemOverlap - BoardGeometry.SoilTop, StemRadius, GardenPalette.Stem);
            AddLeaves(meshBuilder, cellCenter, flowerLevel, headBase);
            Vector3 headPoint = cellCenter + new Vector3(0f, headBase, 0f);
            switch (flowerPhase)
            {
                case FlowerPhase.Bud:
                    FlowerParts.AddRing(meshBuilder, headPoint, _budBracts);
                    FlowerParts.AddPart(meshBuilder, headPoint + new Vector3(0f, BudTipLift, 0f), _budTipScale, 0f, 0f, GardenPalette.ProteaBudTip);
                    break;
                case FlowerPhase.Opening:
                    FlowerParts.AddRing(meshBuilder, headPoint, _openingBracts);
                    FlowerParts.AddPart(meshBuilder, headPoint + new Vector3(0f, OpeningHeartLift, 0f), _openingHeartScale, 0f, 0f, GardenPalette.ProteaOpeningHeart);
                    break;
                case FlowerPhase.FullBloom:
                    AddFullBloom(meshBuilder, headPoint);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flowerLevel));
            }
        }

        private static void AddLeaves(MeshBuilder meshBuilder, Vector3 cellCenter, int flowerLevel, float headBase)
        {
            float leafSpacing = (headBase - BoardGeometry.PlanterTop - LeafSpanMargin) / flowerLevel;
            for (var leafIndex = 0; leafIndex <= flowerLevel; leafIndex++)
            {
                float headingDegrees = FirstLeafHeadingDegrees + (LeafHeadingStepDegrees * leafIndex);
                float leafHeight = BoardGeometry.PlanterTop + LowestLeafLift + (leafSpacing * leafIndex);
                Vector3 leafCenter = cellCenter + (BoardGeometry.GetHeadingDirection(headingDegrees) * LeafDistance) + new Vector3(0f, leafHeight, 0f);
                FlowerParts.AddPart(meshBuilder, leafCenter, _leafScale, LeafTiltDegrees, headingDegrees, GardenPalette.SilverLeaf);
            }
        }

        private static void AddFullBloom(MeshBuilder meshBuilder, Vector3 headPoint)
        {
            FlowerParts.AddRing(meshBuilder, headPoint - new Vector3(0f, OuterBractDrop, 0f), _outerBracts);
            FlowerParts.AddRing(meshBuilder, headPoint, _innerBracts);
            FlowerParts.AddPart(meshBuilder, headPoint + new Vector3(0f, DomeLift, 0f), _domeScale, 0f, 0f, GardenPalette.ProteaDome);
            for (var floretIndex = 0; floretIndex < FloretCount; floretIndex++)
            {
                bool isOuterFloret = (floretIndex % FloretRowCount) != 0;
                float floretDistance = isOuterFloret ? InnerFloretDistance + FloretRowSpacing : InnerFloretDistance;
                float floretLift = isOuterFloret ? FloretLift - FloretRowDrop : FloretLift;
                float headingDegrees = FlowerParts.GetCircleHeading(floretIndex, FloretCount, 0f);
                Vector3 floretCenter = headPoint + (BoardGeometry.GetHeadingDirection(headingDegrees) * floretDistance) + new Vector3(0f, floretLift, 0f);
                FlowerParts.AddBead(meshBuilder, floretCenter, _floretScale, GardenPalette.ProteaFloret);
            }
        }
    }
}
