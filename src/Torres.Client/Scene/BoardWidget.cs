using System;
using System.Collections.Generic;

using Game.Contracts;

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

        private readonly GraphicsDevice _graphicsDevice;
        private readonly OrbitCamera _orbitCamera = new OrbitCamera();
        private readonly BasicEffect _sceneEffect;
        private readonly ButterflyFigure _butterflyFigure;
        private readonly CaterpillarFigure _caterpillarFigure;
        private readonly GrowingFlowers _growingFlowers;
        private readonly CellMarkers _cellMarkers;
        private readonly TableFigures _tableFigures;
        private readonly HandLayer _handLayer;

        private BoardLayout _shownLayout;
        private CellPicker _cellPicker;
        private StaticMesh _gardenMesh;
        private StaticMesh _shadowMesh;
        private RenderTarget2D? _sceneRenderTarget;
        private TimeSpan _sceneTime;
        private bool _isSkippingNextFlight;
        private BoardCell? _hoveredCell;
        private BoardCell? _selectedCell;
        private BoardCell? _clickedCell;
        private MatchCard? _clickedCard;
        private bool _isPressing;
        private bool _hasDragged;
        private Point _pressPosition;
        private Point _lastDragPosition;

        internal BoardWidget(GraphicsDevice graphicsDevice, BoardLayout layout)
        {
            _graphicsDevice = graphicsDevice;
            _shownLayout = layout;
            _cellPicker = new CellPicker(layout);
            _sceneEffect = CreateSceneEffect(graphicsDevice);
            _gardenMesh = BuildGardenMesh(graphicsDevice, layout, Array.Empty<BoardCell>());
            _shadowMesh = BuildShadowMesh(graphicsDevice, layout);
            _butterflyFigure = new ButterflyFigure(graphicsDevice);
            _caterpillarFigure = new CaterpillarFigure(graphicsDevice);
            _growingFlowers = new GrowingFlowers(graphicsDevice);
            _cellMarkers = new CellMarkers(graphicsDevice);
            _tableFigures = new TableFigures(graphicsDevice);
            _handLayer = new HandLayer(graphicsDevice);

            ClipToBounds = true;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }

        protected override bool AcceptsMouseWheel => true;

        internal void ShowLayout(BoardLayout layout)
        {
            var layoutChange = new LayoutChange(_shownLayout, layout);
            _caterpillarFigure.PlanMotions(layoutChange, _sceneTime);
            _growingFlowers.Replace(layoutChange, _sceneTime);
            _cellMarkers.MarkArrivals(layoutChange, _sceneTime);
            ReplaceLayout(layout);
        }

        internal void ShowLayoutWithoutMotion(BoardLayout layout)
        {
            _caterpillarFigure.ClearMotions();
            _growingFlowers.Clear();
            _handLayer.ForgetHand();
            _cellMarkers.ClearArrivals();
            _handLayer.ForgetActionPoints();
            _isSkippingNextFlight = true;
            ReplaceLayout(layout);
        }

        internal void ShowPlacementTargets(IReadOnlyList<BoardCell> placementTargetCells)
        {
            _cellMarkers.ShowTargets(placementTargetCells);
        }

        internal void ShowButterflyCell(BoardCell? butterflyCell)
        {
            if ((butterflyCell is BoardCell landingCell) && _isSkippingNextFlight)
            {
                _butterflyFigure.LandAt(FindFlowerTopPoint(landingCell));
            }
            else if (butterflyCell is BoardCell shownCell)
            {
                _butterflyFigure.FlyTo(FindFlowerTopPoint(shownCell), _sceneTime);
            }
            else
            {
                _butterflyFigure.Hide();
            }

            _isSkippingNextFlight = false;
        }

        internal MatchCard? HoveredCard => _handLayer.HoveredCard;

        internal void ShowHand(IReadOnlyList<HandCardState> handStates, int deckCount)
        {
            _handLayer.ShowHand(handStates, deckCount);
        }

        internal void RefreshCardFaces()
        {
            _handLayer.RefreshCardFaces();
        }

        internal MatchCard? TakeClickedCard()
        {
            MatchCard? clickedCard = _clickedCard;
            _clickedCard = null;

            return clickedCard;
        }

        internal void ShowActionPoints(int actionPoints, int previewCost)
        {
            _handLayer.ShowActionPoints(actionPoints, previewCost);
        }

        internal void ShowReserve(int caterpillarsLeft, Color caterpillarColor, int buildsLeft)
        {
            _handLayer.ShowReserve(caterpillarsLeft, caterpillarColor, buildsLeft);
        }

        internal Point? ProjectToWidget(Vector3 worldPoint, bool isOnHandLayer)
        {
            Point? widgetPoint = null;
            if (_sceneRenderTarget is not null)
            {
                Matrix viewMatrix = isOnHandLayer ? _handLayer.ViewMatrix : _orbitCamera.CreateViewMatrix();
                Vector3 screenPoint = CreateViewport().Project(worldPoint, CreateProjectionMatrix(), viewMatrix, Matrix.Identity);
                widgetPoint = new Point((int)screenPoint.X, (int)screenPoint.Y);
            }

            return widgetPoint;
        }

        internal void Advance(TimeSpan elapsedTime)
        {
            _sceneTime += elapsedTime;
            _handLayer.Advance(_sceneTime);
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

            _handLayer.HoveredCard = IsMouseInside ? PickCardAt(desktop.MousePosition) : null;
            _hoveredCell = IsMouseInside && (_handLayer.HoveredCard is null) ? PickCellAt(desktop.MousePosition) : null;
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
            _handLayer.RenderFacesOnce();
            EnsureSceneRenderTarget();
            if (_sceneRenderTarget is null)
            {
                return;
            }

            _graphicsDevice.SetRenderTarget(_sceneRenderTarget);
            _graphicsDevice.Clear(GardenPalette.Background);
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _sceneEffect.View = _orbitCamera.CreateViewMatrix();
            _sceneEffect.Projection = CreateProjectionMatrix();
            _sceneEffect.World = Matrix.Identity;
            _sceneEffect.LightingEnabled = true;
            _tableFigures.DrawTable(_sceneEffect);
            _gardenMesh.Draw(_graphicsDevice, _sceneEffect);
            _growingFlowers.Draw(_sceneEffect, _sceneTime);
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
            if (_butterflyFigure.IsShown)
            {
                _butterflyFigure.DrawShadow(_sceneEffect, _sceneTime, FindGroundHeightBelow(_butterflyFigure.FindFlightPoint(_sceneTime)));
            }

            _cellMarkers.Draw(_sceneEffect, _shownLayout, _sceneTime, new CellFocus(_hoveredCell, _selectedCell));
            _handLayer.Draw(_sceneEffect);

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
            if (_sceneRenderTarget is not null)
            {
                context.Draw(_sceneRenderTarget, ActualBounds, Color.White);
            }
        }

        public void Dispose()
        {
            _sceneRenderTarget?.Dispose();
            _gardenMesh.Dispose();
            _shadowMesh.Dispose();
            _butterflyFigure.Dispose();
            _caterpillarFigure.Dispose();
            _growingFlowers.Dispose();
            _cellMarkers.Dispose();
            _tableFigures.Dispose();
            _handLayer.Dispose();
            _sceneEffect.Dispose();
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

        private static StaticMesh BuildGardenMesh(GraphicsDevice graphicsDevice, BoardLayout layout, IReadOnlyCollection<BoardCell> growingCells)
        {
            var gardenBuilder = new MeshBuilder();
            BoardShapes.AddBoard(gardenBuilder, layout);
            FlowerShapes.AddFlowers(gardenBuilder, layout, growingCells);
            StaticMesh gardenMesh = gardenBuilder.BuildMesh(graphicsDevice);

            return gardenMesh;
        }

        private static StaticMesh BuildShadowMesh(GraphicsDevice graphicsDevice, BoardLayout layout)
        {
            var shadowBuilder = new MeshBuilder();
            ShadowShapes.AddShadows(shadowBuilder, layout);
            StaticMesh shadowMesh = shadowBuilder.BuildMesh(graphicsDevice);

            return shadowMesh;
        }

        private void ReplaceLayout(BoardLayout layout)
        {
            _gardenMesh.Dispose();
            _shadowMesh.Dispose();
            _shownLayout = layout;
            _cellPicker = new CellPicker(layout);
            _gardenMesh = BuildGardenMesh(_graphicsDevice, layout, _growingFlowers.Cells);
            _shadowMesh = BuildShadowMesh(_graphicsDevice, layout);
            _selectedCell = null;
        }

        private float FindGroundHeightBelow(Vector3 worldPoint)
        {
            BoardCell? cellBelow = BoardGeometry.FindCellAt(worldPoint);
            bool isOverPlanter = (cellBelow is BoardCell examinedCell) && _shownLayout.IsPlanter(examinedCell);
            float groundHeight = isOverPlanter ? BoardGeometry.SoilTop : BoardGeometry.GrassTop;

            return groundHeight;
        }

        private Vector3 FindFlowerTopPoint(BoardCell flowerCell)
        {
            int flowerLevel = _shownLayout.GetFlowerLevel(flowerCell);
            float topHeight = flowerLevel > 0 ? BoardGeometry.GetFlowerTop(flowerLevel) : BoardGeometry.GetSurfaceHeight(_shownLayout, flowerCell);
            Vector3 flowerTopPoint = BoardGeometry.GetCellCenter(flowerCell) + new Vector3(0f, topHeight, 0f);

            return flowerTopPoint;
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
            MatchCard? pressedCard = _hasDragged ? null : PickCardAt(mousePosition);
            if (pressedCard is not null)
            {
                _clickedCard = pressedCard;
            }
            else if (!_hasDragged)
            {
                _selectedCell = PickCellAt(mousePosition);
                _clickedCell = _selectedCell;
            }

            _isPressing = false;
        }

        private MatchCard? PickCardAt(Point windowPosition)
        {
            MatchCard? pickedCard = null;
            if (_sceneRenderTarget is not null)
            {
                Point localPosition = ToLocal(windowPosition);
                var surfacePosition = new Vector2(localPosition.X - ActualBounds.X, localPosition.Y - ActualBounds.Y);
                pickedCard = _handLayer.PickCard(CreatePickRay(surfacePosition, _handLayer.ViewMatrix));
            }

            return pickedCard;
        }

        private BoardCell? PickCellAt(Point windowPosition)
        {
            BoardCell? pickedCell = null;
            if (_sceneRenderTarget is not null)
            {
                Point localPosition = ToLocal(windowPosition);
                var surfacePosition = new Vector2(localPosition.X - ActualBounds.X, localPosition.Y - ActualBounds.Y);
                pickedCell = _cellPicker.PickCell(CreatePickRay(surfacePosition, _orbitCamera.CreateViewMatrix()));
            }

            return pickedCell;
        }

        private void EnsureSceneRenderTarget()
        {
            Rectangle viewBounds = ActualBounds;
            bool hasArea = (viewBounds.Width > 0) && (viewBounds.Height > 0);
            bool hasSizeChanged = (_sceneRenderTarget is null)
                || (_sceneRenderTarget.Width != viewBounds.Width)
                || (_sceneRenderTarget.Height != viewBounds.Height);
            if (hasArea && hasSizeChanged)
            {
                _sceneRenderTarget?.Dispose();
                _sceneRenderTarget = new RenderTarget2D(
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

        private Ray CreatePickRay(Vector2 surfacePosition, Matrix viewMatrix)
        {
            Viewport viewport = CreateViewport();
            Matrix projectionMatrix = CreateProjectionMatrix();
            Vector3 nearPoint = viewport.Unproject(new Vector3(surfacePosition, NearPlaneDepth), projectionMatrix, viewMatrix, Matrix.Identity);
            Vector3 farPoint = viewport.Unproject(new Vector3(surfacePosition, FarPlaneDepth), projectionMatrix, viewMatrix, Matrix.Identity);
            var pickRay = new Ray(nearPoint, Vector3.Normalize(farPoint - nearPoint));

            return pickRay;
        }
    }
}
