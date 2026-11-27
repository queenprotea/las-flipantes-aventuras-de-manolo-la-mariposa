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
        private const int DragThresholdPixels = 4;
        private const float NearPlaneDepth = 0f;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly OrbitCamera _orbitCamera = new OrbitCamera();
        private readonly GardenScene _gardenScene;
        private readonly GrowingFlowers _growingFlowers;
        private readonly CellMarkers _cellMarkers;
        private readonly HandLayer _handLayer;

        private CellPicker _cellPicker;
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
            _cellPicker = new CellPicker(layout);
            _gardenScene = new GardenScene(graphicsDevice, layout);
            _growingFlowers = new GrowingFlowers(graphicsDevice);
            _cellMarkers = new CellMarkers(graphicsDevice, _gardenScene.SceneEffect);
            _handLayer = new HandLayer(graphicsDevice);

            ClipToBounds = true;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }

        protected override bool AcceptsMouseWheel => true;

        internal MatchCard? HoveredCard => _handLayer.HoveredCard;

        internal void ShowLayout(BoardLayout layout)
        {
            var layoutChange = new LayoutChange(_gardenScene.ShownLayout, layout);
            _gardenScene.CaterpillarFigure.PlanMotions(layoutChange, _sceneTime);
            _growingFlowers.Replace(layoutChange, _sceneTime);
            _cellMarkers.MarkArrivals(layoutChange, _sceneTime);
            ReplaceLayout(layout);
        }

        internal void ShowLayoutWithoutMotion(BoardLayout layout)
        {
            _gardenScene.CaterpillarFigure.ClearMotions();
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
                _gardenScene.ButterflyFigure.LandAt(BoardGeometry.FindFlowerTopPoint(_gardenScene.ShownLayout, landingCell));
            }
            else if (butterflyCell is BoardCell shownCell)
            {
                _gardenScene.ButterflyFigure.FlyTo(BoardGeometry.FindFlowerTopPoint(_gardenScene.ShownLayout, shownCell), _sceneTime);
            }
            else
            {
                _gardenScene.ButterflyFigure.Hide();
            }

            _isSkippingNextFlight = false;
        }

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
            bool canHoverCell = IsMouseInside && (_handLayer.HoveredCard is null);
            _hoveredCell = canHoverCell ? PickCellAt(desktop.MousePosition) : null;
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

            _gardenScene.BeginFrame(_sceneRenderTarget, _orbitCamera.CreateViewMatrix(), CreateProjectionMatrix());
            _gardenScene.DrawSolids(_sceneTime);
            _growingFlowers.Draw(_gardenScene.SceneEffect, _sceneTime);
            _gardenScene.DrawShadows();
            if (_gardenScene.ButterflyFigure.IsShown)
            {
                _gardenScene.ButterflyFigure.DrawShadow(_gardenScene.SceneEffect, _sceneTime, BoardGeometry.FindGroundHeightBelow(_gardenScene.ShownLayout, _gardenScene.ButterflyFigure.FindFlightPoint(_sceneTime)));
            }

            _cellMarkers.Draw(_gardenScene.ShownLayout, _sceneTime, new CellFocus(_hoveredCell, _selectedCell));
            _handLayer.Draw(_gardenScene.SceneEffect);

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
            _gardenScene.Dispose();
            _growingFlowers.Dispose();
            _cellMarkers.Dispose();
            _handLayer.Dispose();
        }

        private void ReplaceLayout(BoardLayout layout)
        {
            _gardenScene.ReplaceGarden(layout, _growingFlowers.Cells);
            _cellPicker = new CellPicker(layout);
            _selectedCell = null;
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
                _sceneRenderTarget = GardenScene.CreateDepthSurface(_graphicsDevice, viewBounds.Width, viewBounds.Height);
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
            Vector3 farPoint = viewport.Unproject(new Vector3(surfacePosition, SceneValues.FarPlaneDepth), projectionMatrix, viewMatrix, Matrix.Identity);
            var pickRay = new Ray(nearPoint, Vector3.Normalize(farPoint - nearPoint));

            return pickRay;
        }
    }
}
