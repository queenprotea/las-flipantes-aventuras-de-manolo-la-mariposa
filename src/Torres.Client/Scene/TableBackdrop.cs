using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

namespace Torres.Client.Scene
{
    internal sealed class TableBackdrop : Widget, IDisposable
    {
        private const int BlurDivisor = 4;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly OrbitCamera _orbitCamera = new OrbitCamera();
        private readonly GardenScene _gardenScene;
        private readonly SpriteBatch _spriteBatch;

        private RenderTarget2D? _sceneRenderTarget;
        private RenderTarget2D? _blurredRenderTarget;
        private TimeSpan _sceneTime;

        internal TableBackdrop(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _gardenScene = new GardenScene(graphicsDevice, BoardLayout.Empty);
            _spriteBatch = new SpriteBatch(graphicsDevice);
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }

        internal void ShowGarden(BoardLayout layout, BoardCell? butterflyCell)
        {
            _gardenScene.ReplaceGarden(layout, Array.Empty<BoardCell>());
            if (butterflyCell is BoardCell landingCell)
            {
                _gardenScene.ButterflyFigure.LandAt(BoardGeometry.FindFlowerTopPoint(layout, landingCell));
            }
            else
            {
                _gardenScene.ButterflyFigure.Hide();
            }
        }

        internal void Advance(TimeSpan elapsedTime)
        {
            _sceneTime += elapsedTime;
        }

        internal void RenderScene()
        {
            EnsureRenderTargets();
            if ((_sceneRenderTarget is null) || (_blurredRenderTarget is null))
            {
                return;
            }

            float aspectRatio = (float)_sceneRenderTarget.Width / _sceneRenderTarget.Height;
            _gardenScene.BeginFrame(_sceneRenderTarget, _orbitCamera.CreateViewMatrix(), OrbitCamera.CreateProjectionMatrix(aspectRatio));
            _gardenScene.DrawSolids(_sceneTime);
            _gardenScene.DrawShadows();
            _graphicsDevice.SetRenderTarget(_blurredRenderTarget);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp);
            _spriteBatch.Draw(_sceneRenderTarget, _blurredRenderTarget.Bounds, Color.White);
            _spriteBatch.End();
            _graphicsDevice.SetRenderTarget(null);
        }

        public override void InternalRender(RenderContext context)
        {
            if (_blurredRenderTarget is not null)
            {
                context.Draw(_blurredRenderTarget, ActualBounds, Color.White);
            }

            context.FillRectangle(ActualBounds, GardenPalette.BackdropVeil);
        }

        public void Dispose()
        {
            _sceneRenderTarget?.Dispose();
            _blurredRenderTarget?.Dispose();
            _spriteBatch.Dispose();
            _gardenScene.Dispose();
        }

        private void EnsureRenderTargets()
        {
            int surfaceWidth = ActualBounds.Width / BlurDivisor;
            int surfaceHeight = ActualBounds.Height / BlurDivisor;
            if ((surfaceWidth > 0) && (surfaceHeight > 0))
            {
                EnsureBlurredRenderTarget();
                EnsureSceneRenderTarget(surfaceWidth, surfaceHeight);
            }
        }

        private void EnsureBlurredRenderTarget()
        {
            bool hasSizeChanged = (_blurredRenderTarget is null)
                || (_blurredRenderTarget.Width != ActualBounds.Width)
                || (_blurredRenderTarget.Height != ActualBounds.Height);
            if (hasSizeChanged)
            {
                _blurredRenderTarget?.Dispose();
                _blurredRenderTarget = new RenderTarget2D(_graphicsDevice, ActualBounds.Width, ActualBounds.Height);
            }
        }

        private void EnsureSceneRenderTarget(int surfaceWidth, int surfaceHeight)
        {
            bool hasSizeChanged = (_sceneRenderTarget is null)
                || (_sceneRenderTarget.Width != surfaceWidth)
                || (_sceneRenderTarget.Height != surfaceHeight);
            if (hasSizeChanged)
            {
                _sceneRenderTarget?.Dispose();
                _sceneRenderTarget = GardenScene.CreateDepthSurface(_graphicsDevice, surfaceWidth, surfaceHeight);
            }
        }
    }
}
