using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal sealed class ReserveFigure : IDisposable
    {
        private const float TrayWidth = 1.5f;
        private const float TrayHeight = 0.12f;
        private const float TrayDepth = 2.6f;
        private const float CaterpillarLift = 0.04f;
        private const float CaterpillarStep = 0.5f;
        private const float CaterpillarHeadingDegrees = 0f;
        private const float PotStep = 0.5f;
        private const float PotRadius = 0.17f;
        private const float PotHeight = 0.18f;
        private const float SproutLift = 0.24f;
        private const float HalfShare = 0.5f;
        private const float LabelLift = 0.3f;
        private const float LabelGap = 0.3f;

        private static readonly Vector3 _trayCenter = new Vector3(-5.6f, TableShapes.TableTop, 3.9f);
        private static readonly Vector3 _firstCaterpillar = new Vector3(-5.95f, TableShapes.TableTop + TrayHeight + CaterpillarLift, 2.85f);
        private static readonly Vector3 _firstPot = new Vector3(-5.05f, TableShapes.TableTop + TrayHeight, 4.15f);
        private static readonly Vector3 _sproutScale = new Vector3(0.06f, 0.11f, 0.06f);

        private readonly GraphicsDevice _graphicsDevice;
        private readonly StaticMesh _trayMesh;
        private readonly StaticMesh _potMesh;
        private readonly CaterpillarFigure _caterpillarFigure;

        private int _caterpillarsLeft;
        private int _buildsLeft;
        private Color _caterpillarColor = Color.White;

        internal ReserveFigure(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            var trayBuilder = new MeshBuilder();
            trayBuilder.AddBox(Matrix.CreateScale(TrayWidth, TrayHeight, TrayDepth) * Matrix.CreateTranslation(_trayCenter), GardenPalette.Tray);
            _trayMesh = trayBuilder.BuildMesh(graphicsDevice);
            var potBuilder = new MeshBuilder();
            potBuilder.AddCylinder(Matrix.CreateScale(PotRadius, PotHeight, PotRadius), GardenPalette.PlanterRim);
            potBuilder.AddSphere(Matrix.CreateScale(_sproutScale) * Matrix.CreateTranslation(0f, SproutLift, 0f), GardenPalette.Leaf);
            _potMesh = potBuilder.BuildMesh(graphicsDevice);
            _caterpillarFigure = new CaterpillarFigure(graphicsDevice);
        }

        internal static Vector3 LabelAnchor { get; } = _trayCenter + new Vector3(0f, LabelLift, -(TrayDepth * HalfShare) - LabelGap);

        internal void ShowReserve(int caterpillarsLeft, Color caterpillarColor, int buildsLeft)
        {
            _caterpillarsLeft = caterpillarsLeft;
            _caterpillarColor = caterpillarColor;
            _buildsLeft = buildsLeft;
        }

        internal void Draw(BasicEffect sceneEffect)
        {
            sceneEffect.World = Matrix.Identity;
            _trayMesh.Draw(_graphicsDevice, sceneEffect);
            for (var caterpillarIndex = 0; caterpillarIndex < _caterpillarsLeft; caterpillarIndex++)
            {
                Vector3 bodyOrigin = _firstCaterpillar + new Vector3(0f, 0f, CaterpillarStep * caterpillarIndex);
                _caterpillarFigure.DrawStill(sceneEffect, bodyOrigin, CaterpillarHeadingDegrees, _caterpillarColor);
            }

            for (var potIndex = 0; potIndex < _buildsLeft; potIndex++)
            {
                sceneEffect.World = Matrix.CreateTranslation(_firstPot + new Vector3(0f, 0f, PotStep * potIndex));
                _potMesh.Draw(_graphicsDevice, sceneEffect);
            }

            sceneEffect.World = Matrix.Identity;
        }

        public void Dispose()
        {
            _trayMesh.Dispose();
            _potMesh.Dispose();
            _caterpillarFigure.Dispose();
        }
    }
}
