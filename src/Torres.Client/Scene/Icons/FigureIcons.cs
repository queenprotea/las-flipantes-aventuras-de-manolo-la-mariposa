using System;
using System.Collections.Generic;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Myra.Graphics2D.TextureAtlases;

using Torres.Client.UI;

namespace Torres.Client.Scene
{
    internal sealed class FigureIcons : IDisposable
    {
        private const int CaterpillarIconWidth = 128;
        private const int IconHeight = 64;
        private const int SproutIconWidth = 64;
        private const float CaterpillarHeadingDegrees = 0f;
        private const float CaterpillarScale = 1f;
        private const float CaterpillarViewWidth = 1.05f;
        private const float CaterpillarViewHeight = 0.525f;
        private const float SproutViewSize = 0.5f;
        private const float NearPlaneDistance = 0.05f;
        private const float FarPlaneDistance = 10f;
        private const float SproutPotRadius = 0.17f;
        private const float SproutPotHeight = 0.18f;
        private const float SproutLift = 0.24f;

        private static readonly Vector3 _caterpillarFocus = new Vector3(0.06f, 0.14f, 0f);
        private static readonly Vector3 _caterpillarCameraOffset = new Vector3(0.25f, 0.7f, 1.8f);
        private static readonly Vector3 _sproutFocus = new Vector3(0f, 0.16f, 0f);
        private static readonly Vector3 _sproutCameraOffset = new Vector3(0f, 0.55f, 1.2f);
        private static readonly Vector3 _sproutScale = new Vector3(0.06f, 0.11f, 0.06f);

        private readonly GraphicsDevice _graphicsDevice;
        private readonly RenderTarget2D _iconAtlas;
        private readonly BasicEffect _iconEffect;
        private readonly CaterpillarFigure _caterpillarFigure;
        private readonly StaticMesh _sproutMesh;
        private readonly SeatColor[] _seatColors = Enum.GetValues<SeatColor>();
        private readonly Dictionary<SeatColor, TextureRegion> _caterpillarIcons = new Dictionary<SeatColor, TextureRegion>();
        private bool _isRendered;

        internal FigureIcons(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            int atlasWidth = (CaterpillarIconWidth * _seatColors.Length) + SproutIconWidth;
            _iconAtlas = new RenderTarget2D(graphicsDevice, atlasWidth, IconHeight, false, SurfaceFormat.Color, DepthFormat.Depth24, SceneValues.MultiSampleCount, RenderTargetUsage.PreserveContents);
            _iconEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                PreferPerPixelLighting = true,
                SpecularColor = Vector3.Zero,
                LightingEnabled = true,
            };
            SceneLighting.ApplyTo(_iconEffect);
            _caterpillarFigure = new CaterpillarFigure(graphicsDevice);
            _sproutMesh = BuildSproutMesh(graphicsDevice);
            for (var colorIndex = 0; colorIndex < _seatColors.Length; colorIndex++)
            {
                _caterpillarIcons[_seatColors[colorIndex]] = new TextureRegion(_iconAtlas, FindCaterpillarBounds(colorIndex));
            }

            SproutIcon = new TextureRegion(_iconAtlas, FindSproutBounds());
        }

        internal TextureRegion SproutIcon { get; }

        internal TextureRegion FindCaterpillarIcon(SeatColor seatColor)
        {
            TextureRegion caterpillarIcon = _caterpillarIcons[seatColor];

            return caterpillarIcon;
        }

        internal void RenderOnce()
        {
            if (_isRendered)
            {
                return;
            }

            Viewport screenViewport = _graphicsDevice.Viewport;
            _graphicsDevice.SetRenderTarget(_iconAtlas);
            _graphicsDevice.Clear(Color.Transparent);
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            for (var colorIndex = 0; colorIndex < _seatColors.Length; colorIndex++)
            {
                RenderCaterpillar(FindCaterpillarBounds(colorIndex), SeatColors.ColorFor(_seatColors[colorIndex]));
            }

            RenderSprout(FindSproutBounds());
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.Viewport = screenViewport;
            _isRendered = true;
        }

        public void Dispose()
        {
            _iconAtlas.Dispose();
            _iconEffect.Dispose();
            _caterpillarFigure.Dispose();
            _sproutMesh.Dispose();
        }

        private static Rectangle FindCaterpillarBounds(int colorIndex)
        {
            var caterpillarBounds = new Rectangle(CaterpillarIconWidth * colorIndex, 0, CaterpillarIconWidth, IconHeight);

            return caterpillarBounds;
        }

        private static StaticMesh BuildSproutMesh(GraphicsDevice graphicsDevice)
        {
            var sproutBuilder = new MeshBuilder();
            sproutBuilder.AddCylinder(Matrix.CreateScale(SproutPotRadius, SproutPotHeight, SproutPotRadius), GardenPalette.PlanterRim);
            sproutBuilder.AddSphere(Matrix.CreateScale(_sproutScale) * Matrix.CreateTranslation(0f, SproutLift, 0f), GardenPalette.Leaf);
            StaticMesh sproutMesh = sproutBuilder.BuildMesh(graphicsDevice);

            return sproutMesh;
        }

        private Rectangle FindSproutBounds()
        {
            var sproutBounds = new Rectangle(CaterpillarIconWidth * _seatColors.Length, 0, SproutIconWidth, IconHeight);

            return sproutBounds;
        }

        private void RenderCaterpillar(Rectangle iconBounds, Color bodyColor)
        {
            _graphicsDevice.Viewport = new Viewport(iconBounds);
            _iconEffect.View = Matrix.CreateLookAt(_caterpillarFocus + _caterpillarCameraOffset, _caterpillarFocus, Vector3.Up);
            _iconEffect.Projection = Matrix.CreateOrthographic(CaterpillarViewWidth, CaterpillarViewHeight, NearPlaneDistance, FarPlaneDistance);
            _caterpillarFigure.DrawStill(_iconEffect, new CaterpillarPose(Vector3.Zero, CaterpillarHeadingDegrees, bodyColor, 0f, CaterpillarScale));
        }

        private void RenderSprout(Rectangle iconBounds)
        {
            _graphicsDevice.Viewport = new Viewport(iconBounds);
            _iconEffect.View = Matrix.CreateLookAt(_sproutFocus + _sproutCameraOffset, _sproutFocus, Vector3.Up);
            _iconEffect.Projection = Matrix.CreateOrthographic(SproutViewSize, SproutViewSize, NearPlaneDistance, FarPlaneDistance);
            _iconEffect.World = Matrix.Identity;
            _sproutMesh.Draw(_graphicsDevice, _iconEffect);
        }
    }
}
