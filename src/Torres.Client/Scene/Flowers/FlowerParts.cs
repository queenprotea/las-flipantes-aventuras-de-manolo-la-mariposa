using System;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class FlowerParts
    {
        private const float FullTurnDegrees = 360f;

        internal static void AddColumn(MeshBuilder meshBuilder, PartPose columnPose, Color color)
        {
            meshBuilder.AddCylinder(CreatePartTransform(columnPose), color);
        }

        internal static void AddPart(MeshBuilder meshBuilder, PartPose partPose, Color color)
        {
            meshBuilder.AddSphere(CreatePartTransform(partPose), color);
        }

        internal static void AddCoarsePart(MeshBuilder meshBuilder, PartPose partPose, Color color)
        {
            meshBuilder.AddCoarseSphere(CreatePartTransform(partPose), color);
        }

        internal static void AddRing(MeshBuilder meshBuilder, Vector3 axisPoint, PetalRing petalRing)
        {
            float tiltRadians = MathHelper.ToRadians(petalRing.TiltDegrees);
            float petalLength = petalRing.PetalScale.Y;
            float petalReach = petalRing.Radius + (MathF.Sin(tiltRadians) * petalLength);
            float petalLift = MathF.Cos(tiltRadians) * petalLength;
            for (var petalIndex = 0; petalIndex < petalRing.PetalCount; petalIndex++)
            {
                float headingDegrees = GetCircleHeadingDegrees(petalIndex, petalRing.PetalCount, petalRing.PhaseDegrees);
                Vector3 petalCenter = axisPoint + (BoardGeometry.GetHeadingDirection(headingDegrees) * petalReach) + new Vector3(0f, petalLift, 0f);
                AddPart(meshBuilder, new PartPose(petalCenter, petalRing.PetalScale, petalRing.TiltDegrees, headingDegrees), petalRing.Color);
            }
        }

        internal static void AddLeafRing(MeshBuilder meshBuilder, Vector3 cellCenter, LeafRing leafRing)
        {
            for (var leafIndex = 0; leafIndex < leafRing.LeafCount; leafIndex++)
            {
                float headingDegrees = GetCircleHeadingDegrees(leafIndex, leafRing.LeafCount, leafRing.PhaseDegrees);
                Vector3 leafCenter = cellCenter + (BoardGeometry.GetHeadingDirection(headingDegrees) * leafRing.Distance) + new Vector3(0f, BoardGeometry.PlanterTop + leafRing.Lift, 0f);
                var leafPose = new PartPose(leafCenter, leafRing.Scale, leafRing.TiltDegrees, headingDegrees);
                if (leafRing.IsCoarse)
                {
                    AddCoarsePart(meshBuilder, leafPose, leafRing.Color);
                }
                else
                {
                    AddPart(meshBuilder, leafPose, leafRing.Color);
                }
            }
        }

        internal static float GetCircleHeadingDegrees(int partIndex, int partCount, float phaseDegrees)
        {
            float headingDegrees = (FullTurnDegrees * partIndex / partCount) + phaseDegrees;

            return headingDegrees;
        }

        private static Matrix CreatePartTransform(PartPose partPose)
        {
            Matrix partTransform = Matrix.CreateScale(partPose.Scale)
                * Matrix.CreateRotationZ(MathHelper.ToRadians(-partPose.TiltDegrees))
                * BoardGeometry.CreateHeadingRotation(partPose.HeadingDegrees)
                * Matrix.CreateTranslation(partPose.Center);

            return partTransform;
        }
    }
}
