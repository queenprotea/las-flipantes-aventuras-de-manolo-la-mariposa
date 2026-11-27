using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal sealed class GardenScene : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly TableFigures _tableFigures;

        private StaticMesh _gardenMesh;
        private StaticMesh _shadowMesh;

        internal GardenScene(GraphicsDevice graphicsDevice, BoardLayout layout)
        {
            _graphicsDevice = graphicsDevice;
            SceneEffect = CreateSceneEffect(graphicsDevice);
            _tableFigures = new TableFigures(graphicsDevice);
            CaterpillarFigure = new CaterpillarFigure(graphicsDevice);
            ButterflyFigure = new ButterflyFigure(graphicsDevice);
            ShownLayout = layout;
            _gardenMesh = GardenMeshes.BuildGardenMesh(graphicsDevice, layout, Array.Empty<BoardCell>());
            _shadowMesh = GardenMeshes.BuildShadowMesh(graphicsDevice, layout);
        }

        internal BasicEffect SceneEffect { get; }

        internal CaterpillarFigure CaterpillarFigure { get; }

        internal ButterflyFigure ButterflyFigure { get; }

        internal BoardLayout ShownLayout { get; private set; }

        internal static RenderTarget2D CreateDepthSurface(GraphicsDevice graphicsDevice, int surfaceWidth, int surfaceHeight)
        {
            var depthSurface = new RenderTarget2D(
                graphicsDevice,
                surfaceWidth,
                surfaceHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24,
                SceneValues.MultiSampleCount,
                RenderTargetUsage.DiscardContents);

            return depthSurface;
        }

        internal void ReplaceGarden(BoardLayout layout, IReadOnlyCollection<BoardCell> skippedCells)
        {
            _gardenMesh.Dispose();
            _shadowMesh.Dispose();
            ShownLayout = layout;
            _gardenMesh = GardenMeshes.BuildGardenMesh(_graphicsDevice, layout, skippedCells);
            _shadowMesh = GardenMeshes.BuildShadowMesh(_graphicsDevice, layout);
        }

        internal void BeginFrame(RenderTarget2D surface, Matrix viewMatrix, Matrix projectionMatrix)
        {
            _graphicsDevice.SetRenderTarget(surface);
            _graphicsDevice.Clear(GardenPalette.Background);
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            SceneEffect.View = viewMatrix;
            SceneEffect.Projection = projectionMatrix;
            SceneEffect.World = Matrix.Identity;
            SceneEffect.LightingEnabled = true;
        }

        internal void DrawSolids(TimeSpan sceneTime)
        {
            _tableFigures.DrawTable(SceneEffect);
            _gardenMesh.Draw(_graphicsDevice, SceneEffect);
            CaterpillarFigure.Draw(SceneEffect, ShownLayout, sceneTime);
            if (ButterflyFigure.IsShown)
            {
                ButterflyFigure.Draw(SceneEffect, sceneTime);
            }
        }

        internal void DrawShadows()
        {
            SceneEffect.World = Matrix.Identity;
            SceneEffect.LightingEnabled = false;
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            _shadowMesh.Draw(_graphicsDevice, SceneEffect);
        }

        public void Dispose()
        {
            _gardenMesh.Dispose();
            _shadowMesh.Dispose();
            _tableFigures.Dispose();
            CaterpillarFigure.Dispose();
            ButterflyFigure.Dispose();
            SceneEffect.Dispose();
        }

        private static BasicEffect CreateSceneEffect(GraphicsDevice graphicsDevice)
        {
            var sceneEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                PreferPerPixelLighting = true,
                SpecularColor = Vector3.Zero,
            };
            SceneLighting.ApplyTo(sceneEffect);

            return sceneEffect;
        }
    }
}
