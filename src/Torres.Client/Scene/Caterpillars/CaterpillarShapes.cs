using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class CaterpillarShapes
    {
        private const float LiftOnFlower = 0.03f;
        private const float SegmentSpacing = 1.45f;
        private const float FirstSegmentOffset = -0.33f;
        private const float SegmentWaveHeight = 0.025f;
        private const float SegmentWaveStepDegrees = 97.4f;
        private const float HeadRadius = 0.11f;
        private const float HeadForwardGap = 0.03f;
        private const float HeadLift = 0.04f;
        private const float EyeRadius = 0.022f;
        private const float EyeForward = 0.09f;
        private const float EyeSpread = 0.045f;
        private const float EyeLift = 0.03f;
        private const float AntennaSpread = 0.05f;
        private const float AntennaBaseLift = 0.08f;
        private const float AntennaTiltDegrees = 20f;

        private static readonly float[] _segmentRadii = new float[] { 0.07f, 0.08f, 0.085f, 0.09f };
        private static readonly Vector3 _antennaScale = new Vector3(0.01f, 0.12f, 0.01f);

        internal static float FindBaseHeight(BoardLayout layout, BoardCell caterpillarCell)
        {
            int flowerLevel = layout.GetFlowerLevel(caterpillarCell);
            float baseHeight = BoardGeometry.GrassTop;
            if (flowerLevel > 0)
            {
                baseHeight = BoardGeometry.GetFlowerTop(flowerLevel) + LiftOnFlower;
            }

            return baseHeight;
        }

        internal static void AddParts(List<CaterpillarPart> parts, CaterpillarPose pose)
        {
            Vector3 headingDirection = BoardGeometry.GetHeadingDirection(pose.HeadingDegrees);
            float segmentOffset = FirstSegmentOffset;
            for (var segmentIndex = 0; segmentIndex < _segmentRadii.Length; segmentIndex++)
            {
                float segmentRadius = _segmentRadii[segmentIndex];
                float wavePhaseRadians = MathHelper.ToRadians(SegmentWaveStepDegrees * segmentIndex) + pose.WavePhaseRadians;
                float waveLift = SegmentWaveHeight * MathF.Sin(wavePhaseRadians);
                Vector3 segmentCenter = pose.BodyOrigin + (headingDirection * segmentOffset) + new Vector3(0f, segmentRadius + waveLift, 0f);
                parts.Add(new CaterpillarPart(Matrix.CreateScale(segmentRadius) * Matrix.CreateTranslation(segmentCenter), pose.BodyColor, CaterpillarPartShape.Sphere));
                segmentOffset += segmentRadius * SegmentSpacing;
            }

            Vector3 headCenter = pose.BodyOrigin + (headingDirection * (segmentOffset + HeadForwardGap)) + new Vector3(0f, HeadRadius + HeadLift, 0f);
            parts.Add(new CaterpillarPart(Matrix.CreateScale(HeadRadius) * Matrix.CreateTranslation(headCenter), pose.BodyColor, CaterpillarPartShape.Sphere));
            AddFace(parts, pose, headCenter);
        }

        private static void AddFace(List<CaterpillarPart> parts, CaterpillarPose pose, Vector3 headCenter)
        {
            Vector3 headingDirection = BoardGeometry.GetHeadingDirection(pose.HeadingDegrees);
            Vector3 sidewaysDirection = Vector3.Cross(Vector3.Up, headingDirection);
            foreach (float sideSign in BoardGeometry.MirroredSideSigns)
            {
                Vector3 eyeCenter = headCenter + (headingDirection * EyeForward) + (sidewaysDirection * EyeSpread * sideSign) + new Vector3(0f, EyeLift, 0f);
                parts.Add(new CaterpillarPart(Matrix.CreateScale(EyeRadius) * Matrix.CreateTranslation(eyeCenter), GardenPalette.CaterpillarEye, CaterpillarPartShape.Sphere));

                Vector3 antennaBase = headCenter + (sidewaysDirection * AntennaSpread * sideSign) + new Vector3(0f, AntennaBaseLift, 0f);
                float antennaTiltRadians = MathHelper.ToRadians(-AntennaTiltDegrees * sideSign);
                Matrix antennaTransform = Matrix.CreateScale(_antennaScale)
                    * Matrix.CreateFromAxisAngle(headingDirection, antennaTiltRadians)
                    * Matrix.CreateTranslation(antennaBase);
                parts.Add(new CaterpillarPart(antennaTransform, pose.BodyColor, CaterpillarPartShape.Cylinder));
            }
        }
    }
}
