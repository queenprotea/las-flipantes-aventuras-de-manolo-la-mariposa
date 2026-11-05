using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

namespace Torres.Client.Scene
{
    internal sealed class BoardWidget : Widget, IDisposable
    {
        private const int MultiSampleCount = 4;
        private const int DragThresholdPixels = 4;
        private const float NearPlaneDepth = 0f;
        private const float FarPlaneDepth = 1f;

        private const float FrameLift = 0.012f;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly OrbitCamera _orbitCamera = new OrbitCamera();
        private readonly BasicEffect _sceneEffect;
        private readonly StaticMesh _hoverFrameMesh;
        private readonly StaticMesh _selectionFrameMesh;
        private readonly StaticMesh _targetFrameMesh;
        private readonly StaticMesh _butterflyFrameMesh;

        private BoardLayout _layout;
        private CellPicker _cellPicker;
        private StaticMesh _gardenMesh;
        private StaticMesh _shadowMesh;
        private IReadOnlyList<BoardCell> _targetCells = Array.Empty<BoardCell>();
        private BoardCell? _butterflyCell;
        private RenderTarget2D? _sceneTarget;
        private BoardCell? _hoveredCell;
        private BoardCell? _selectedCell;
        private BoardCell? _clickedCell;
        private bool _isPressing;
        private bool _hasDragged;
        private Point _pressPosition;
        private Point _lastDragPosition;

        internal BoardWidget(GraphicsDevice graphicsDevice, BoardLayout layout)
        {
            _graphicsDevice = graphicsDevice;
            _layout = layout;
            _cellPicker = new CellPicker(layout);
            _sceneEffect = CreateSceneEffect(graphicsDevice);
            _gardenMesh = BuildGardenMesh(graphicsDevice, layout);
            _shadowMesh = BuildShadowMesh(graphicsDevice, layout);
            _hoverFrameMesh = BuildFrameMesh(graphicsDevice, GardenPalette.Hover);
            _selectionFrameMesh = BuildFrameMesh(graphicsDevice, GardenPalette.Selection);
            _targetFrameMesh = BuildFrameMesh(graphicsDevice, GardenPalette.Target);
            _butterflyFrameMesh = BuildFrameMesh(graphicsDevice, GardenPalette.Butterfly);

            ClipToBounds = true;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }

        protected override bool AcceptsMouseWheel => true;

        internal void ShowLayout(BoardLayout layout)
        {
            _gardenMesh.Dispose();
            _shadowMesh.Dispose();
            _layout = layout;
            _cellPicker = new CellPicker(layout);
            _gardenMesh = BuildGardenMesh(_graphicsDevice, layout);
            _shadowMesh = BuildShadowMesh(_graphicsDevice, layout);
            _selectedCell = null;
        }

        internal void ShowTargets(IReadOnlyList<BoardCell> targetCells)
        {
            _targetCells = targetCells;
        }

        internal void ShowButterfly(BoardCell? butterflyCell)
        {
            _butterflyCell = butterflyCell;
        }

        internal BoardCell? TakeClickedCell()
        {
            BoardCell? clickedCell = _clickedCell;
            _clickedCell = null;

            return clickedCell;
        }

        internal void UpdateInput()
        {
            Desktop? desktop = Desktop;
            if (desktop is null)
            {
                return;
            }

            _hoveredCell = IsMouseInside ? PickAt(desktop.MousePosition) : null;
            if (_isPressing && desktop.IsTouchDown)
            {
                ContinueDrag(desktop.MousePosition);
            }
            else if (_isPressing)
            {
                FinishPress(desktop.MousePosition);
            }
        }

        internal void RenderScene()
        {
            EnsureSceneTarget();
            if (_sceneTarget is null)
            {
                return;
            }

            _graphicsDevice.SetRenderTarget(_sceneTarget);
            _graphicsDevice.Clear(GardenPalette.Background);
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _sceneEffect.View = _orbitCamera.CreateViewMatrix();
            _sceneEffect.Projection = CreateProjectionMatrix();
            _sceneEffect.World = Matrix.Identity;
            _sceneEffect.LightingEnabled = true;
            _gardenMesh.Draw(_graphicsDevice, _sceneEffect);

            _sceneEffect.LightingEnabled = false;
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            _shadowMesh.Draw(_graphicsDevice, _sceneEffect);
            DrawFrames();

            _graphicsDevice.SetRenderTarget(null);
        }

        public override void OnTouchDown()
        {
            base.OnTouchDown();
            Point? touchPosition = Desktop?.TouchPosition;
            if (touchPosition is Point pressPosition)
            {
                _isPressing = true;
                _hasDragged = false;
                _pressPosition = pressPosition;
                _lastDragPosition = pressPosition;
            }
        }

        public override void OnMouseWheel(float delta)
        {
            base.OnMouseWheel(delta);
            _orbitCamera.Zoom(delta);
        }

        public override void InternalRender(RenderContext context)
        {
            if (_sceneTarget is not null)
            {
                context.Draw(_sceneTarget, ActualBounds, Color.White);
            }
        }

        public void Dispose()
        {
            _sceneTarget?.Dispose();
            _gardenMesh.Dispose();
            _shadowMesh.Dispose();
            _hoverFrameMesh.Dispose();
            _selectionFrameMesh.Dispose();
            _targetFrameMesh.Dispose();
            _butterflyFrameMesh.Dispose();
            _sceneEffect.Dispose();
        }

        private static BasicEffect CreateSceneEffect(GraphicsDevice graphicsDevice)
        {
            var sceneEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                PreferPerPixelLighting = true,
                AmbientLightColor = SceneLighting.AmbientColor,
                SpecularColor = Vector3.Zero,
            };
            sceneEffect.DirectionalLight0.Enabled = true;
            sceneEffect.DirectionalLight0.Direction = SceneLighting.Direction;
            sceneEffect.DirectionalLight0.DiffuseColor = SceneLighting.DiffuseColor;
            sceneEffect.DirectionalLight0.SpecularColor = Vector3.Zero;
            sceneEffect.DirectionalLight1.Enabled = false;
            sceneEffect.DirectionalLight2.Enabled = false;

            return sceneEffect;
        }

        private static StaticMesh BuildGardenMesh(GraphicsDevice graphicsDevice, BoardLayout layout)
        {
            var gardenBuilder = new MeshBuilder();
            BoardShapes.AddBoard(gardenBuilder, layout);
            FlowerShapes.AddFlowers(gardenBuilder, layout);
            CaterpillarShapes.AddCaterpillars(gardenBuilder, layout);
            StaticMesh gardenMesh = gardenBuilder.Build(graphicsDevice);

            return gardenMesh;
        }

        private static StaticMesh BuildShadowMesh(GraphicsDevice graphicsDevice, BoardLayout layout)
        {
            var shadowBuilder = new MeshBuilder();
            ShadowShapes.AddShadows(shadowBuilder, layout);
            StaticMesh shadowMesh = shadowBuilder.Build(graphicsDevice);

            return shadowMesh;
        }

        private static StaticMesh BuildFrameMesh(GraphicsDevice graphicsDevice, Color frameColor)
        {
            var frameBuilder = new MeshBuilder();
            CellFrameShapes.AddCellFrame(frameBuilder, frameColor);
            StaticMesh frameMesh = frameBuilder.Build(graphicsDevice);

            return frameMesh;
        }

        private void DrawFrames()
        {
            foreach (BoardCell targetCell in _targetCells)
            {
                DrawFrame(_targetFrameMesh, targetCell);
            }

            if (_butterflyCell is BoardCell butterflyCell)
            {
                DrawFrame(_butterflyFrameMesh, butterflyCell);
            }

            if ((_hoveredCell is BoardCell hoveredCell) && (hoveredCell != _selectedCell))
            {
                DrawFrame(_hoverFrameMesh, hoveredCell);
            }

            if (_selectedCell is BoardCell selectedCell)
            {
                DrawFrame(_selectionFrameMesh, selectedCell);
            }

            _sceneEffect.World = Matrix.Identity;
        }

        private void DrawFrame(StaticMesh frameMesh, BoardCell cell)
        {
            float frameHeight = BoardGeometry.GetSurfaceHeight(_layout, cell) + FrameLift;
            Vector3 frameCenter = BoardGeometry.GetCellCenter(cell) + new Vector3(0f, frameHeight, 0f);
            _sceneEffect.World = Matrix.CreateTranslation(frameCenter);
            frameMesh.Draw(_graphicsDevice, _sceneEffect);
        }

        private void ContinueDrag(Point mousePosition)
        {
            Point dragStep = mousePosition - _lastDragPosition;
            Point dragTotal = mousePosition - _pressPosition;
            int dragDistancePixels = Math.Abs(dragTotal.X) + Math.Abs(dragTotal.Y);
            if (dragDistancePixels > DragThresholdPixels)
            {
                _hasDragged = true;
            }

            if (_hasDragged)
            {
                _orbitCamera.Rotate(new Vector2(dragStep.X, dragStep.Y));
            }

            _lastDragPosition = mousePosition;
        }

        private void FinishPress(Point mousePosition)
        {
            if (!_hasDragged)
            {
                _selectedCell = PickAt(mousePosition);
                _clickedCell = _selectedCell;
            }

            _isPressing = false;
        }

        private BoardCell? PickAt(Point windowPosition)
        {
            BoardCell? pickedCell = null;
            if (_sceneTarget is not null)
            {
                Point localPosition = ToLocal(windowPosition);
                var surfacePosition = new Vector2(localPosition.X - ActualBounds.X, localPosition.Y - ActualBounds.Y);
                pickedCell = _cellPicker.Pick(CreatePickRay(surfacePosition));
            }

            return pickedCell;
        }

        private void EnsureSceneTarget()
        {
            Rectangle viewBounds = ActualBounds;
            bool hasArea = (viewBounds.Width > 0) && (viewBounds.Height > 0);
            bool hasSizeChanged = (_sceneTarget is null)
                || (_sceneTarget.Width != viewBounds.Width)
                || (_sceneTarget.Height != viewBounds.Height);
            if (hasArea && hasSizeChanged)
            {
                _sceneTarget?.Dispose();
                _sceneTarget = new RenderTarget2D(
                    _graphicsDevice,
                    viewBounds.Width,
                    viewBounds.Height,
                    false,
                    SurfaceFormat.Color,
                    DepthFormat.Depth24,
                    MultiSampleCount,
                    RenderTargetUsage.DiscardContents);
            }
        }

        private Viewport CreateViewport()
        {
            var viewport = new Viewport(0, 0, ActualBounds.Width, ActualBounds.Height);

            return viewport;
        }

        private Matrix CreateProjectionMatrix()
        {
            float aspectRatio = CreateViewport().AspectRatio;
            Matrix projectionMatrix = _orbitCamera.CreateProjectionMatrix(aspectRatio);

            return projectionMatrix;
        }

        private Ray CreatePickRay(Vector2 surfacePosition)
        {
            Viewport viewport = CreateViewport();
            Matrix viewMatrix = _orbitCamera.CreateViewMatrix();
            Matrix projectionMatrix = CreateProjectionMatrix();
            Vector3 nearPoint = viewport.Unproject(new Vector3(surfacePosition, NearPlaneDepth), projectionMatrix, viewMatrix, Matrix.Identity);
            Vector3 farPoint = viewport.Unproject(new Vector3(surfacePosition, FarPlaneDepth), projectionMatrix, viewMatrix, Matrix.Identity);
            var pickRay = new Ray(nearPoint, Vector3.Normalize(farPoint - nearPoint));

            return pickRay;
        }
    }
}
