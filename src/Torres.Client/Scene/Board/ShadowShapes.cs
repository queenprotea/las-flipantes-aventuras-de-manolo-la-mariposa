using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    // BasicEffect cannot cast shadows, so each piece leaves a translucent disc on the surface below it, offset away from the light.
    internal static class ShadowShapes
    {
        // Lifted so the shadow does not share a plane with the soil and flicker.
        private const float ShadowLift = 0.004f;
        private const float FlowerShadowRadius = 0.2f;
        private const float FlowerShadowHeightShare = 0.5f;
        private const float CaterpillarShadowLength = 0.42f;
        private const float CaterpillarShadowWidth = 0.16f;

        internal static void AddShadows(MeshBuilder meshBuilder, BoardLayout layout)
        {
            foreach (BoardCell cell in BoardLayout.EnumerateCells())
            {
                int flowerLevel = layout.GetFlowerLevel(cell);
                if (flowerLevel > 0)
                {
                    AddFlowerShadow(meshBuilder, cell, flowerLevel);
                }
            }

            foreach (BoardCaterpillar caterpillar in layout.Caterpillars)
            {
                if (layout.GetFlowerLevel(caterpillar.Cell) == 0)
                {
                    AddGroundCaterpillarShadow(meshBuilder, caterpillar);
                }
            }
        }

        private static void AddFlowerShadow(MeshBuilder meshBuilder, BoardCell cell, int flowerLevel)
        {
            float shadowHeight = (BoardGeometry.GetFlowerTop(flowerLevel) - BoardGeometry.SoilTop) * FlowerShadowHeightShare;
            Vector3 shadowCenter = BoardGeometry.GetCellCenter(cell)
                + SceneLighting.GetShadowOffset(shadowHeight)
                + new Vector3(0f, BoardGeometry.SoilTop + ShadowLift, 0f);
            Matrix shadowTransform = Matrix.CreateScale(FlowerShadowRadius, 1f, FlowerShadowRadius) * Matrix.CreateTranslation(shadowCenter);
            meshBuilder.AddDisc(shadowTransform, GardenPalette.Shadow);
        }

        private static void AddGroundCaterpillarShadow(MeshBuilder meshBuilder, BoardCaterpillar caterpillar)
        {
            Vector3 shadowCenter = BoardGeometry.GetCellCenter(caterpillar.Cell) + new Vector3(0f, BoardGeometry.GrassTop + ShadowLift, 0f);
            Matrix shadowTransform = Matrix.CreateScale(CaterpillarShadowLength, 1f, CaterpillarShadowWidth)
                * BoardGeometry.CreateHeadingRotation(caterpillar.HeadingDegrees)
                * Matrix.CreateTranslation(shadowCenter);
            meshBuilder.AddDisc(shadowTransform, GardenPalette.Shadow);
        }
    }
}
