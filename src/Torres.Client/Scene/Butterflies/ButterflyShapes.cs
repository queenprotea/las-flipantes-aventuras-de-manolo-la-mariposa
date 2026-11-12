using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class ButterflyShapes
    {
        private const float WingThickness = 0.018f;
        private const float UpperWingSpan = 0.22f;
        private const float UpperWingDepth = 0.2f;
        private const float UpperWingForward = 0.05f;
        private const float UpperWingSweepDegrees = 18f;
        private const float LowerWingSpan = 0.15f;
        private const float LowerWingDepth = 0.13f;
        private const float LowerWingBackward = 0.11f;
        private const float LowerWingSweepDegrees = -28f;
        private const float WingRootGap = 0.03f;
        private const float WingRimSpread = 1.09f;
        private const float WingRimThinning = 0.55f;
        private const float BodyLength = 0.21f;
        private const float BodyRadius = 0.048f;
        private const float HeadRadius = 0.058f;
        private const float HeadForward = 0.21f;
        private const float AntennaLength = 0.2f;
        private const float AntennaRadius = 0.01f;
        private const float AntennaForwardTiltDegrees = 28f;
        private const float AntennaSpreadDegrees = 26f;
        private const float AntennaTipRadius = 0.021f;
        private const float ShadowLength = 0.32f;
        private const float ShadowWidth = 0.24f;

        internal static void AddBody(MeshBuilder meshBuilder)
        {
            meshBuilder.AddSphere(Matrix.CreateScale(BodyLength, BodyRadius, BodyRadius), GardenPalette.ButterflyBody);
            var headCenter = new Vector3(HeadForward, 0f, 0f);
            meshBuilder.AddSphere(Matrix.CreateScale(HeadRadius) * Matrix.CreateTranslation(headCenter), GardenPalette.ButterflyBody);
            foreach (float sideSign in BoardGeometry.MirroredSideSigns)
            {
                AddAntenna(meshBuilder, headCenter, sideSign);
            }
        }

        internal static void AddWings(MeshBuilder meshBuilder, float sideSign)
        {
            bool isLeftSide = sideSign < 0f;
            var upperWingCenter = new Vector3(UpperWingForward, 0f, sideSign * (WingRootGap + UpperWingSpan));
            Matrix upperWingTransform = Matrix.CreateRotationY(MathHelper.ToRadians(-sideSign * UpperWingSweepDegrees)) * Matrix.CreateTranslation(upperWingCenter);
            var upperWingSize = new Vector3(UpperWingDepth, WingThickness, UpperWingSpan);
            Color upperWingColor = isLeftSide ? GardenPalette.ButterflyLeftWing : GardenPalette.ButterflyRightWing;
            Color upperWingRimColor = isLeftSide ? GardenPalette.ButterflyLeftWingRim : GardenPalette.ButterflyRightWingRim;
            AddWing(meshBuilder, new WingPiece(upperWingSize, upperWingTransform), upperWingColor);
            AddWing(meshBuilder, new WingPiece(upperWingSize * new Vector3(WingRimSpread, WingRimThinning, WingRimSpread), upperWingTransform), upperWingRimColor);

            var lowerWingCenter = new Vector3(-LowerWingBackward, 0f, sideSign * (WingRootGap + LowerWingSpan));
            Matrix lowerWingTransform = Matrix.CreateRotationY(MathHelper.ToRadians(-sideSign * LowerWingSweepDegrees)) * Matrix.CreateTranslation(lowerWingCenter);
            var lowerWingSize = new Vector3(LowerWingDepth, WingThickness, LowerWingSpan);
            AddWing(meshBuilder, new WingPiece(lowerWingSize, lowerWingTransform), GardenPalette.ButterflyLowerWing);
            AddWing(meshBuilder, new WingPiece(lowerWingSize * new Vector3(WingRimSpread, WingRimThinning, WingRimSpread), lowerWingTransform), GardenPalette.ButterflyLowerWingRim);
        }

        internal static void AddShadow(MeshBuilder meshBuilder)
        {
            meshBuilder.AddDisc(Matrix.CreateScale(ShadowLength, 1f, ShadowWidth), GardenPalette.Shadow);
        }

        private static void AddWing(MeshBuilder meshBuilder, WingPiece wingPiece, Color wingColor)
        {
            meshBuilder.AddSphere(Matrix.CreateScale(wingPiece.Size) * wingPiece.Transform, wingColor);
        }

        private static void AddAntenna(MeshBuilder meshBuilder, Vector3 headCenter, float sideSign)
        {
            Matrix antennaRotation = Matrix.CreateRotationZ(MathHelper.ToRadians(-AntennaForwardTiltDegrees))
                * Matrix.CreateRotationX(MathHelper.ToRadians(sideSign * AntennaSpreadDegrees));
            Matrix antennaTransform = Matrix.CreateScale(AntennaRadius, AntennaLength, AntennaRadius)
                * antennaRotation
                * Matrix.CreateTranslation(headCenter);
            meshBuilder.AddCylinder(antennaTransform, GardenPalette.ButterflyBody);

            Vector3 antennaTip = headCenter + Vector3.TransformNormal(Vector3.UnitY * AntennaLength, antennaRotation);
            meshBuilder.AddSphere(Matrix.CreateScale(AntennaTipRadius) * Matrix.CreateTranslation(antennaTip), GardenPalette.ButterflyBody);
        }
    }
}
