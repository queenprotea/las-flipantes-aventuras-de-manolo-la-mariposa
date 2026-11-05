using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class FlowerShapes
    {
        private const float StemRadius = 0.035f;
        private const float LeafHeightOffset = 0.12f;
        private const float LeafDistance = 0.13f;
        private const float LeafTiltDegrees = 20f;
        private const float FirstLeafHeadingDegrees = 35f;
        private const float LeafHeadingStepDegrees = 90f;
        private const int PetalCount = 6;
        private const float FullTurnDegrees = 360f;
        private const float PetalDistance = 0.11f;
        private const float HeartLift = 0.02f;

        private static readonly Vector3 _leafScale = new Vector3(0.14f, 0.022f, 0.055f);
        private static readonly Vector3 _petalScale = new Vector3(0.094f, 0.03f, 0.068f);
        private static readonly Vector3 _heartScale = new Vector3(0.075f, 0.045f, 0.075f);

        internal static void AddFlowers(MeshBuilder meshBuilder, BoardLayout layout)
        {
            foreach (BoardCell drawnCell in BoardLayout.EnumerateCells())
            {
                int flowerLevel = layout.GetFlowerLevel(drawnCell);
                if (flowerLevel > 0)
                {
                    AddFlower(meshBuilder, BoardGeometry.GetCellCenter(drawnCell), flowerLevel);
                }
            }
        }

        private static void AddFlower(MeshBuilder meshBuilder, Vector3 cellCenter, int flowerLevel)
        {
            float flowerTop = BoardGeometry.GetFlowerTop(flowerLevel);
            float stemLength = flowerTop - BoardGeometry.SoilTop;
            Matrix stemTransform = Matrix.CreateScale(StemRadius, stemLength, StemRadius)
                * Matrix.CreateTranslation(cellCenter + new Vector3(0f, BoardGeometry.SoilTop, 0f));
            meshBuilder.AddCylinder(stemTransform, GardenPalette.Stem);
            for (var levelIndex = 0; levelIndex < flowerLevel; levelIndex++)
            {
                AddLeafPair(meshBuilder, cellCenter, levelIndex);
            }

            AddBlossom(meshBuilder, cellCenter + new Vector3(0f, flowerTop, 0f));
        }

        private static void AddLeafPair(MeshBuilder meshBuilder, Vector3 cellCenter, int levelIndex)
        {
            float leafHeight = BoardGeometry.PlanterTop + LeafHeightOffset + (BoardGeometry.FlowerLevelHeight * levelIndex);
            float leafHeadingDegrees = FirstLeafHeadingDegrees + (LeafHeadingStepDegrees * levelIndex);
            Matrix leafHeadingRotation = BoardGeometry.CreateHeadingRotation(leafHeadingDegrees);
            Vector3 leafDirection = BoardGeometry.GetHeadingDirection(leafHeadingDegrees);
            foreach (float sideSign in BoardGeometry.MirroredSideSigns)
            {
                Vector3 leafCenter = cellCenter + (leafDirection * LeafDistance * sideSign) + new Vector3(0f, leafHeight, 0f);
                Matrix leafTransform = Matrix.CreateScale(_leafScale)
                    * Matrix.CreateRotationZ(MathHelper.ToRadians(LeafTiltDegrees * sideSign))
                    * leafHeadingRotation
                    * Matrix.CreateTranslation(leafCenter);
                meshBuilder.AddSphere(leafTransform, GardenPalette.Leaf);
            }
        }

        private static void AddBlossom(MeshBuilder meshBuilder, Vector3 blossomCenter)
        {
            for (var petalIndex = 0; petalIndex < PetalCount; petalIndex++)
            {
                float petalHeadingDegrees = FullTurnDegrees * petalIndex / PetalCount;
                Vector3 petalCenter = blossomCenter + (BoardGeometry.GetHeadingDirection(petalHeadingDegrees) * PetalDistance);
                Matrix petalTransform = Matrix.CreateScale(_petalScale)
                    * BoardGeometry.CreateHeadingRotation(petalHeadingDegrees)
                    * Matrix.CreateTranslation(petalCenter);
                meshBuilder.AddSphere(petalTransform, GardenPalette.Petal);
            }

            Matrix heartTransform = Matrix.CreateScale(_heartScale) * Matrix.CreateTranslation(blossomCenter + new Vector3(0f, HeartLift, 0f));
            meshBuilder.AddSphere(heartTransform, GardenPalette.BlossomHeart);
        }
    }
}
