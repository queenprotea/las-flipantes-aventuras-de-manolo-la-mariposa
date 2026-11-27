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
        private const float AntennaTipRadius = 0.02f;
        private const float AntennaTipShade = 0.7f;
        private const float FootSpreadFraction = 0.7f;
        private const float FootDropFraction = 0.85f;
        private const float FootShade = 0.75f;
        private const float CheekForward = 0.07f;
        private const float CheekSpread = 0.075f;
        private const float CheekDrop = 0.02f;

        private static readonly float[] _segmentRadii = new float[] { 0.07f, 0.08f, 0.085f, 0.09f };
        private static readonly Vector3 _antennaScale = new Vector3(0.01f, 0.12f, 0.01f);
        private static readonly Vector3 _footScale = new Vector3(0.018f, 0.014f, 0.018f);
        private static readonly Vector3 _cheekScale = new Vector3(0.02f, 0.016f, 0.02f);

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
            Vector3 sidewaysDirection = Vector3.Cross(Vector3.Up, headingDirection);
            var footColor = new Color(pose.BodyColor.ToVector3() * FootShade);
            float segmentOffset = FirstSegmentOffset;
            for (var segmentIndex = 0; segmentIndex < _segmentRadii.Length; segmentIndex++)
            {
                float segmentRadius = _segmentRadii[segmentIndex];
                float wavePhaseRadians = MathHelper.ToRadians(SegmentWaveStepDegrees * segmentIndex) + pose.WavePhaseRadians;
                float waveLift = SegmentWaveHeight * MathF.Sin(wavePhaseRadians);
                Vector3 segmentCenter = pose.BodyOrigin + (headingDirection * segmentOffset) + new Vector3(0f, segmentRadius + waveLift, 0f);
                parts.Add(new CaterpillarPart(Matrix.CreateScale(segmentRadius) * Matrix.CreateTranslation(segmentCenter), pose.BodyColor, CaterpillarPartShape.Sphere, true));
                var segment = new BoundingSphere(segmentCenter, segmentRadius);
                foreach (float sideSign in BoardGeometry.MirroredSideSigns)
                {
                    parts.Add(new CaterpillarPart(CreateFootTransform(segment, sidewaysDirection, sideSign), footColor, CaterpillarPartShape.Sphere, false));
                }
                segmentOffset += segmentRadius * SegmentSpacing;
            }

            Vector3 headCenter = pose.BodyOrigin + (headingDirection * (segmentOffset + HeadForwardGap)) + new Vector3(0f, HeadRadius + HeadLift, 0f);
            parts.Add(new CaterpillarPart(Matrix.CreateScale(HeadRadius) * Matrix.CreateTranslation(headCenter), pose.BodyColor, CaterpillarPartShape.Sphere, true));
            AddFace(parts, pose, headCenter);
        }

        private static Matrix CreateFootTransform(BoundingSphere segment, Vector3 sidewaysDirection, float sideSign)
        {
            Vector3 footCenter = segment.Center + (sidewaysDirection * segment.Radius * FootSpreadFraction * sideSign) - new Vector3(0f, segment.Radius * FootDropFraction, 0f);
            Matrix footTransform = Matrix.CreateScale(_footScale) * Matrix.CreateTranslation(footCenter);

            return footTransform;
        }

        private static void AddFace(List<CaterpillarPart> parts, CaterpillarPose pose, Vector3 headCenter)
        {
            Vector3 headingDirection = BoardGeometry.GetHeadingDirection(pose.HeadingDegrees);
            Vector3 sidewaysDirection = Vector3.Cross(Vector3.Up, headingDirection);
            var antennaTipColor = new Color(pose.BodyColor.ToVector3() * AntennaTipShade);
            foreach (float sideSign in BoardGeometry.MirroredSideSigns)
            {
                Vector3 eyeCenter = headCenter + (headingDirection * EyeForward) + (sidewaysDirection * EyeSpread * sideSign) + new Vector3(0f, EyeLift, 0f);
                parts.Add(new CaterpillarPart(Matrix.CreateScale(EyeRadius) * Matrix.CreateTranslation(eyeCenter), GardenPalette.CaterpillarEye, CaterpillarPartShape.Sphere, false));

                Vector3 cheekCenter = headCenter + (headingDirection * CheekForward) + (sidewaysDirection * CheekSpread * sideSign) - new Vector3(0f, CheekDrop, 0f);
                parts.Add(new CaterpillarPart(Matrix.CreateScale(_cheekScale) * Matrix.CreateTranslation(cheekCenter), GardenPalette.CaterpillarCheek, CaterpillarPartShape.Sphere, false));

                Vector3 antennaBase = headCenter + (sidewaysDirection * AntennaSpread * sideSign) + new Vector3(0f, AntennaBaseLift, 0f);
                Matrix antennaRotation = Matrix.CreateFromAxisAngle(headingDirection, MathHelper.ToRadians(-AntennaTiltDegrees * sideSign));
                Matrix antennaTransform = Matrix.CreateScale(_antennaScale) * antennaRotation * Matrix.CreateTranslation(antennaBase);
                parts.Add(new CaterpillarPart(antennaTransform, pose.BodyColor, CaterpillarPartShape.Cylinder, false));

                Vector3 antennaTip = antennaBase + Vector3.TransformNormal(new Vector3(0f, _antennaScale.Y, 0f), antennaRotation);
                parts.Add(new CaterpillarPart(Matrix.CreateScale(AntennaTipRadius) * Matrix.CreateTranslation(antennaTip), antennaTipColor, CaterpillarPartShape.Sphere, false));
            }
        }
    }
}
