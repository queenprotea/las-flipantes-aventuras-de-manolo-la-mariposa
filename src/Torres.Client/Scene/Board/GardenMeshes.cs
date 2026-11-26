using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal static class GardenMeshes
    {
        internal static StaticMesh BuildGardenMesh(GraphicsDevice graphicsDevice, BoardLayout layout, IReadOnlyCollection<BoardCell> growingCells)
        {
            var gardenBuilder = new MeshBuilder();
            BoardShapes.AddBoard(gardenBuilder, layout);
            FlowerShapes.AddFlowers(gardenBuilder, layout, growingCells);
            StaticMesh gardenMesh = gardenBuilder.BuildMesh(graphicsDevice);

            return gardenMesh;
        }

        internal static StaticMesh BuildShadowMesh(GraphicsDevice graphicsDevice, BoardLayout layout)
        {
            var shadowBuilder = new MeshBuilder();
            ShadowShapes.AddShadows(shadowBuilder, layout);
            StaticMesh shadowMesh = shadowBuilder.BuildMesh(graphicsDevice);

            return shadowMesh;
        }
    }
}
