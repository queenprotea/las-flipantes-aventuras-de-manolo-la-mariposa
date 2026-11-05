using System;

using Microsoft.Xna.Framework;

using Torres.Client.Ui;

namespace Torres.Client.Scene
{
    internal static class CaterpillarShapes
    {
        private const float LiftOnFlower = 0.03f;
        private const float SegmentSpacing = 1.45f;
        private const float FirstSegmentOffset = -0.33f;
        private const float SegmentBobHeight = 0.025f;
        private const float SegmentBobStepDegrees = 97.4f;
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

        internal static void AddCaterpillars(MeshBuilder meshBuilder, BoardLayout layout)
        {
            foreach (BoardCaterpillar caterpillar in layout.Caterpillars)
            {
                AddCaterpillar(meshBuilder, layout, caterpillar);
            }
        }

        private static void AddCaterpillar(MeshBuilder meshBuilder, BoardLayout layout, BoardCaterpillar caterpillar)
        {
            int flowerLevel = layout.GetFlowerLevel(caterpillar.Cell);
            float baseHeight = flowerLevel > 0
                ? BoardGeometry.GetFlowerTop(flowerLevel) + LiftOnFlower
                : BoardGeometry.GrassTop;
            Vector3 bodyOrigin = BoardGeometry.GetCellCenter(caterpillar.Cell) + new Vector3(0f, baseHeight, 0f);
            Vector3 headingDirection = BoardGeometry.GetHeadingDirection(caterpillar.HeadingDegrees);
            Color bodyColor = SeatColors.ColorFor(caterpillar.SeatColor);

            float segmentOffset = FirstSegmentOffset;
            for (var segmentIndex = 0; segmentIndex < _segmentRadii.Length; segmentIndex++)
            {
                float segmentRadius = _segmentRadii[segmentIndex];
                float bobPhaseRadians = MathHelper.ToRadians(SegmentBobStepDegrees * segmentIndex);
                float bobLift = SegmentBobHeight * MathF.Sin(bobPhaseRadians);
                Vector3 segmentCenter = bodyOrigin + (headingDirection * segmentOffset) + new Vector3(0f, segmentRadius + bobLift, 0f);
                meshBuilder.AddSphere(Matrix.CreateScale(segmentRadius) * Matrix.CreateTranslation(segmentCenter), bodyColor);
                segmentOffset += segmentRadius * SegmentSpacing;
            }

            Vector3 headCenter = bodyOrigin + (headingDirection * (segmentOffset + HeadForwardGap)) + new Vector3(0f, HeadRadius + HeadLift, 0f);
            meshBuilder.AddSphere(Matrix.CreateScale(HeadRadius) * Matrix.CreateTranslation(headCenter), bodyColor);
            AddFace(meshBuilder, caterpillar, headCenter);
        }

        private static void AddFace(MeshBuilder meshBuilder, BoardCaterpillar caterpillar, Vector3 headCenter)
        {
            Vector3 headingDirection = BoardGeometry.GetHeadingDirection(caterpillar.HeadingDegrees);
            Vector3 sidewaysDirection = Vector3.Cross(Vector3.Up, headingDirection);
            Color bodyColor = SeatColors.ColorFor(caterpillar.SeatColor);
            foreach (float sideSign in BoardGeometry.MirroredSideSigns)
            {
                Vector3 eyeCenter = headCenter + (headingDirection * EyeForward) + (sidewaysDirection * EyeSpread * sideSign) + new Vector3(0f, EyeLift, 0f);
                meshBuilder.AddSphere(Matrix.CreateScale(EyeRadius) * Matrix.CreateTranslation(eyeCenter), GardenPalette.CaterpillarEye);

                Vector3 antennaBase = headCenter + (sidewaysDirection * AntennaSpread * sideSign) + new Vector3(0f, AntennaBaseLift, 0f);
                float antennaTiltRadians = MathHelper.ToRadians(-AntennaTiltDegrees * sideSign);
                Matrix antennaTransform = Matrix.CreateScale(_antennaScale)
                    * Matrix.CreateFromAxisAngle(headingDirection, antennaTiltRadians)
                    * Matrix.CreateTranslation(antennaBase);
                meshBuilder.AddCylinder(antennaTransform, bodyColor);
            }
        }
    }
}
