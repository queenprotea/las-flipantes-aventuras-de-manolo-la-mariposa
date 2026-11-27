using System;
using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class LavenderShapes
    {
        private const int BaseLeafCount = 4;
        private const int LeavesPerLevel = 2;
        private const float LeafDistance = 0.08f;
        private const float LeafLift = 0.08f;
        private const float LeafTiltDegrees = 35f;
        private const int BaseWhorlCount = 3;
        private const int WhorlsPerLevel = 2;
        private const float SpikeLengthPerWhorl = 0.06f;
        private const float StemRadius = 0.018f;
        private const int SingleStemCount = 1;
        private const int BeadsPerWhorl = 4;
        private const float WhorlTwistDegrees = 34f;
        private const float HalfStep = 0.5f;
        private const float BudBeadDistance = 0.026f;
        private const float BloomBeadDistance = 0.03f;
        private const float OpeningBloomFraction = 0.55f;
        private const int AlternatingWhorlCount = 2;

        private static readonly Vector3 _leafScale = new Vector3(0.016f, 0.13f, 0.016f);
        private static readonly Vector3 _budBeadScale = new Vector3(0.03f, 0.034f, 0.03f);
        private static readonly Vector3 _bloomBeadScale = new Vector3(0.024f, 0.028f, 0.024f);
        private static readonly Vector3[] _bushStemOffsets = new Vector3[] { Vector3.Zero, new Vector3(0.07f, -0.12f, 0.03f), new Vector3(-0.05f, -0.08f, -0.06f) };

        internal static void AddFlower(MeshBuilder meshBuilder, Vector3 cellCenter, int flowerLevel)
        {
            var flowerPhase = (FlowerPhase)flowerLevel;
            FlowerParts.AddLeafRing(meshBuilder, cellCenter, new LeafRing(BaseLeafCount + (LeavesPerLevel * flowerLevel), 0f, LeafDistance, LeafLift, _leafScale, LeafTiltDegrees, GardenPalette.SilverLeaf, true));
            int stemCount = flowerPhase == FlowerPhase.Bud ? SingleStemCount : _bushStemOffsets.Length;
            float flowerTop = BoardGeometry.GetFlowerTop(flowerLevel);
            for (var stemIndex = 0; stemIndex < stemCount; stemIndex++)
            {
                Vector3 stemOffset = _bushStemOffsets[stemIndex];
                AddSpike(meshBuilder, cellCenter + new Vector3(stemOffset.X, flowerTop + stemOffset.Y, stemOffset.Z), flowerLevel);
            }
        }

        private static void AddSpike(MeshBuilder meshBuilder, Vector3 spikeTip, int flowerLevel)
        {
            var flowerPhase = (FlowerPhase)flowerLevel;
            int whorlCount = BaseWhorlCount + (WhorlsPerLevel * flowerLevel);
            var stemAxis = new Vector3(spikeTip.X, 0f, spikeTip.Z);
            float spikeTop = spikeTip.Y;
            float spikeLength = SpikeLengthPerWhorl * whorlCount;
            float spikeBottom = spikeTop - spikeLength;
            FlowerParts.AddColumn(meshBuilder, PartPose.CreateUpright(stemAxis + new Vector3(0f, BoardGeometry.SoilTop, 0f), new Vector3(StemRadius, spikeBottom - BoardGeometry.SoilTop, StemRadius)), GardenPalette.Stem);
            bool isBud = flowerPhase == FlowerPhase.Bud;
            float beadDistance = isBud ? BudBeadDistance : BloomBeadDistance;
            Vector3 beadScale = isBud ? _budBeadScale : _bloomBeadScale;
            for (var whorlIndex = 0; whorlIndex < whorlCount; whorlIndex++)
            {
                float whorlHeight = spikeBottom + ((whorlIndex + HalfStep) * spikeLength / whorlCount);
                Color whorlColor = FindWhorlColor(flowerPhase, whorlIndex, (float)whorlIndex / (whorlCount - 1));
                for (var beadIndex = 0; beadIndex < BeadsPerWhorl; beadIndex++)
                {
                    float headingDegrees = FlowerParts.GetCircleHeadingDegrees(beadIndex, BeadsPerWhorl, WhorlTwistDegrees * whorlIndex);
                    Vector3 beadCenter = stemAxis + (BoardGeometry.GetHeadingDirection(headingDegrees) * beadDistance) + new Vector3(0f, whorlHeight, 0f);
                    FlowerParts.AddCoarsePart(meshBuilder, PartPose.CreateUpright(beadCenter, beadScale), whorlColor);
                }
            }
        }

        private static Color FindWhorlColor(FlowerPhase flowerPhase, int whorlIndex, float spikeFraction)
        {
            bool isAlternateWhorl = (whorlIndex % AlternatingWhorlCount) != 0;
            Color whorlColor;
            switch (flowerPhase)
            {
                case FlowerPhase.Bud:
                    whorlColor = isAlternateWhorl ? GardenPalette.LavenderBudGreen : GardenPalette.LavenderBudGray;
                    break;
                case FlowerPhase.Opening:
                    whorlColor = spikeFraction < OpeningBloomFraction ? GardenPalette.LavenderBloom : GardenPalette.LavenderBudGreen;
                    break;
                case FlowerPhase.FullBloom:
                    whorlColor = isAlternateWhorl ? GardenPalette.LavenderBloom : GardenPalette.LavenderDeepBloom;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flowerPhase));
            }

            return whorlColor;
        }
    }
}
