using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal sealed class TableFigures : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly StaticMesh _tableMesh;

        internal TableFigures(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _tableMesh = SceneMeshes.Build(graphicsDevice, TableShapes.AddTable);
        }

        internal void DrawTable(BasicEffect sceneEffect)
        {
            sceneEffect.World = Matrix.Identity;
            _tableMesh.Draw(_graphicsDevice, sceneEffect);
        }

        public void Dispose()
        {
            _tableMesh.Dispose();
        }
    }
}
