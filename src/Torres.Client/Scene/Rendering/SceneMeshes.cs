using System;

using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal static class SceneMeshes
    {
        internal static StaticMesh Build(GraphicsDevice graphicsDevice, Action<MeshBuilder> addShapes)
        {
            var meshBuilder = new MeshBuilder();
            addShapes(meshBuilder);
            StaticMesh builtMesh = meshBuilder.BuildMesh(graphicsDevice);

            return builtMesh;
        }
    }
}
