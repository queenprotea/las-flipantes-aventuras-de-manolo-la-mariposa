using System;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class FlowerParts
    {
        private const float FullTurnDegrees = 360f;

        internal static void AddColumn(MeshBuilder meshBuilder, Vector3 basePoint, float columnHeight, float columnRadius, Color color)
        {
            Matrix columnTransform = Matrix.CreateScale(columnRadius, columnHeight, columnRadius) * Matrix.CreateTranslation(basePoint);
            meshBuilder.AddCylinder(columnTransform, color);
        }

        internal static void AddPart(MeshBuilder meshBuilder, Vector3 partCenter, Vector3 partScale, float tiltDegrees, float headingDegrees, Color color)
        {
            meshBuilder.AddSphere(CreatePartTransform(partCenter, partScale, tiltDegrees, headingDegrees), color);
        }

        internal static void AddCoarsePart(MeshBuilder meshBuilder, Vector3 partCenter, Vector3 partScale, float tiltDegrees, float headingDegrees, Color color)
        {
            meshBuilder.AddCoarseSphere(CreatePartTransform(partCenter, partScale, tiltDegrees, headingDegrees), color);
        }

        internal static void AddBead(MeshBuilder meshBuilder, Vector3 beadCenter, Vector3 beadScale, Color color)
        {
            Matrix beadTransform = Matrix.CreateScale(beadScale) * Matrix.CreateTranslation(beadCenter);
            meshBuilder.AddCoarseSphere(beadTransform, color);
        }

        internal static void AddRing(MeshBuilder meshBuilder, Vector3 axisPoint, PetalRing petalRing)
        {
            float tiltRadians = MathHelper.ToRadians(petalRing.TiltDegrees);
            float petalLength = petalRing.PetalScale.Y;
            float petalReach = petalRing.Radius + (MathF.Sin(tiltRadians) * petalLength);
            float petalLift = MathF.Cos(tiltRadians) * petalLength;
            for (var petalIndex = 0; petalIndex < petalRing.PetalCount; petalIndex++)
            {
                float headingDegrees = GetCircleHeading(petalIndex, petalRing.PetalCount, petalRing.PhaseDegrees);
                Vector3 petalCenter = axisPoint + (BoardGeometry.GetHeadingDirection(headingDegrees) * petalReach) + new Vector3(0f, petalLift, 0f);
                AddPart(meshBuilder, petalCenter, petalRing.PetalScale, petalRing.TiltDegrees, headingDegrees, petalRing.Color);
            }
        }

        internal static float GetCircleHeading(int partIndex, int partCount, float phaseDegrees)
        {
            float headingDegrees = (FullTurnDegrees * partIndex / partCount) + phaseDegrees;

            return headingDegrees;
        }

        private static Matrix CreatePartTransform(Vector3 partCenter, Vector3 partScale, float tiltDegrees, float headingDegrees)
        {
            Matrix partTransform = Matrix.CreateScale(partScale)
                * Matrix.CreateRotationZ(MathHelper.ToRadians(-tiltDegrees))
                * BoardGeometry.CreateHeadingRotation(headingDegrees)
                * Matrix.CreateTranslation(partCenter);

            return partTransform;
        }
    }
}
