using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal sealed class TableFigures : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly StaticMesh _tableMesh;
        private readonly DewLeafFigure _dewLeafFigure;
        private readonly ReserveFigure _reserveFigure;

        internal TableFigures(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _tableMesh = SceneMeshes.Build(graphicsDevice, TableShapes.AddTable);
            _dewLeafFigure = new DewLeafFigure(graphicsDevice);
            _reserveFigure = new ReserveFigure(graphicsDevice);
        }

        internal void ShowActionPoints(int actionPoints, int previewCost, TimeSpan sceneTime)
        {
            _dewLeafFigure.ShowActionPoints(actionPoints, previewCost, sceneTime);
        }

        internal void ForgetActionPoints()
        {
            _dewLeafFigure.ForgetActionPoints();
        }

        internal void ShowReserve(int caterpillarsLeft, Color caterpillarColor, int buildsLeft)
        {
            _reserveFigure.ShowReserve(caterpillarsLeft, caterpillarColor, buildsLeft);
        }

        internal void DrawTable(BasicEffect sceneEffect)
        {
            sceneEffect.World = Matrix.Identity;
            _tableMesh.Draw(_graphicsDevice, sceneEffect);
        }

        internal void DrawObjects(BasicEffect sceneEffect, TimeSpan sceneTime)
        {
            _reserveFigure.Draw(sceneEffect);
            _dewLeafFigure.Draw(sceneEffect, sceneTime);
        }

        public void Dispose()
        {
            _tableMesh.Dispose();
            _dewLeafFigure.Dispose();
            _reserveFigure.Dispose();
        }
    }
}
