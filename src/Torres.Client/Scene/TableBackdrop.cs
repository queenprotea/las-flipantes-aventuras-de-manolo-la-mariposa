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
        private const int MultiSampleCount = 4;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly OrbitCamera _orbitCamera = new OrbitCamera();
        private readonly BasicEffect _sceneEffect;
        private readonly TableFigures _tableFigures;
        private readonly CaterpillarFigure _caterpillarFigure;
        private readonly ButterflyFigure _butterflyFigure;
        private readonly SpriteBatch _spriteBatch;

        private BoardLayout _shownLayout = BoardLayout.Empty;
        private StaticMesh _gardenMesh;
        private StaticMesh _shadowMesh;
        private RenderTarget2D? _sceneRenderTarget;
        private RenderTarget2D? _blurredRenderTarget;
        private TimeSpan _sceneTime;

        internal TableBackdrop(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _sceneEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                PreferPerPixelLighting = true,
                SpecularColor = Vector3.Zero,
            };
            SceneLighting.ApplyTo(_sceneEffect);
            _tableFigures = new TableFigures(graphicsDevice);
            _caterpillarFigure = new CaterpillarFigure(graphicsDevice);
            _butterflyFigure = new ButterflyFigure(graphicsDevice);
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _gardenMesh = GardenMeshes.BuildGardenMesh(graphicsDevice, _shownLayout, Array.Empty<BoardCell>());
            _shadowMesh = GardenMeshes.BuildShadowMesh(graphicsDevice, _shownLayout);
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }

        internal void ShowGarden(BoardLayout layout, BoardCell? butterflyCell)
        {
            _gardenMesh.Dispose();
            _shadowMesh.Dispose();
            _shownLayout = layout;
            _gardenMesh = GardenMeshes.BuildGardenMesh(_graphicsDevice, layout, Array.Empty<BoardCell>());
            _shadowMesh = GardenMeshes.BuildShadowMesh(_graphicsDevice, layout);
            if (butterflyCell is BoardCell landingCell)
            {
                _butterflyFigure.LandAt(BoardGeometry.FindFlowerTopPoint(layout, landingCell));
            }
            else
            {
                _butterflyFigure.Hide();
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

            _graphicsDevice.SetRenderTarget(_sceneRenderTarget);
            _graphicsDevice.Clear(GardenPalette.Background);
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _sceneEffect.View = _orbitCamera.CreateViewMatrix();
            _sceneEffect.Projection = _orbitCamera.CreateProjectionMatrix(_graphicsDevice.Viewport.AspectRatio);
            _sceneEffect.World = Matrix.Identity;
            _sceneEffect.LightingEnabled = true;
            _tableFigures.DrawTable(_sceneEffect);
            _gardenMesh.Draw(_graphicsDevice, _sceneEffect);
            _caterpillarFigure.Draw(_sceneEffect, _shownLayout, _sceneTime);
            if (_butterflyFigure.IsShown)
            {
                _butterflyFigure.Draw(_sceneEffect, _sceneTime);
            }

            _sceneEffect.World = Matrix.Identity;
            _sceneEffect.LightingEnabled = false;
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            _shadowMesh.Draw(_graphicsDevice, _sceneEffect);
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
            _gardenMesh.Dispose();
            _shadowMesh.Dispose();
            _tableFigures.Dispose();
            _caterpillarFigure.Dispose();
            _butterflyFigure.Dispose();
            _sceneEffect.Dispose();
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
                _sceneRenderTarget = new RenderTarget2D(
                    _graphicsDevice,
                    surfaceWidth,
                    surfaceHeight,
                    false,
                    SurfaceFormat.Color,
                    DepthFormat.Depth24,
                    MultiSampleCount,
                    RenderTargetUsage.DiscardContents);
            }
        }
    }
}
