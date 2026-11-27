using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal sealed class CellMarkers : IDisposable
    {
        private const float FrameLift = 0.012f;
        private const float TargetPulseGrowth = 0.07f;
        private const double TargetPulsePeriodSeconds = 1.2;
        private const double ArrivalMarkSeconds = 0.7;
        private const float ArrivalMarkGrowth = 0.35f;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly BasicEffect _sceneEffect;
        private readonly StaticMesh _hoverFrameMesh;
        private readonly StaticMesh _selectionRingMesh;
        private readonly StaticMesh _targetDiscMesh;

        private IReadOnlyList<BoardCell> _targetCells = Array.Empty<BoardCell>();
        private IReadOnlyList<BoardCell> _arrivalCells = Array.Empty<BoardCell>();
        private TimeSpan _arrivalTime;

        internal CellMarkers(GraphicsDevice graphicsDevice, BasicEffect sceneEffect)
        {
            _graphicsDevice = graphicsDevice;
            _sceneEffect = sceneEffect;
            _hoverFrameMesh = SceneMeshes.Build(graphicsDevice, CellFrameShapes.AddHoveredCellFrame);
            _selectionRingMesh = SceneMeshes.Build(graphicsDevice, CellMarkerShapes.AddSelectionRing);
            _targetDiscMesh = SceneMeshes.Build(graphicsDevice, CellMarkerShapes.AddTargetDisc);
        }

        internal void ShowTargets(IReadOnlyList<BoardCell> targetCells)
        {
            _targetCells = targetCells;
        }

        internal void MarkArrivals(LayoutChange layoutChange, TimeSpan sceneTime)
        {
            _arrivalCells = FindArrivalCells(layoutChange);
            _arrivalTime = sceneTime;
        }

        internal void ClearArrivals()
        {
            _arrivalCells = Array.Empty<BoardCell>();
        }

        internal void Draw(BoardLayout layout, TimeSpan sceneTime, CellFocus cellFocus)
        {
            float targetScale = SceneValues.FullScale + (TargetPulseGrowth * SceneTiming.FindCycleWave(sceneTime, TargetPulsePeriodSeconds));
            foreach (BoardCell targetCell in _targetCells)
            {
                DrawMarker(_targetDiscMesh, FindMarkerCenter(layout, targetCell), targetScale);
            }

            float arrivalProgress = SceneTiming.FindProgress(sceneTime, _arrivalTime, ArrivalMarkSeconds);
            if (arrivalProgress < SceneValues.FullScale)
            {
                foreach (BoardCell arrivalCell in _arrivalCells)
                {
                    DrawMarker(_selectionRingMesh, FindMarkerCenter(layout, arrivalCell), SceneValues.FullScale + (ArrivalMarkGrowth * arrivalProgress));
                }
            }

            if ((cellFocus.HoveredCell is BoardCell hoveredCell) && (hoveredCell != cellFocus.SelectedCell))
            {
                DrawMarker(_hoverFrameMesh, FindMarkerCenter(layout, hoveredCell), SceneValues.FullScale);
            }

            if (cellFocus.SelectedCell is BoardCell selectedCell)
            {
                DrawMarker(_selectionRingMesh, FindMarkerCenter(layout, selectedCell), SceneValues.FullScale);
            }

            _sceneEffect.World = Matrix.Identity;
        }

        public void Dispose()
        {
            _hoverFrameMesh.Dispose();
            _selectionRingMesh.Dispose();
            _targetDiscMesh.Dispose();
        }

        private static List<BoardCell> FindArrivalCells(LayoutChange layoutChange)
        {
            var arrivalCells = new List<BoardCell>();
            foreach (BoardCaterpillar caterpillar in layoutChange.NextLayout.Caterpillars)
            {
                if (!layoutChange.PreviousLayout.Caterpillars.Contains(caterpillar))
                {
                    arrivalCells.Add(caterpillar.Cell);
                }
            }

            foreach (BoardCell examinedCell in BoardLayout.EnumerateCells())
            {
                bool hasLevelChanged = layoutChange.PreviousLayout.GetFlowerLevel(examinedCell) != layoutChange.NextLayout.GetFlowerLevel(examinedCell);
                if (hasLevelChanged && !arrivalCells.Contains(examinedCell))
                {
                    arrivalCells.Add(examinedCell);
                }
            }

            return arrivalCells;
        }

        private static Vector3 FindMarkerCenter(BoardLayout layout, BoardCell markedCell)
        {
            float markerHeight = BoardGeometry.GetSurfaceHeight(layout, markedCell) + FrameLift;
            Vector3 markerCenter = BoardGeometry.GetCellCenter(markedCell) + new Vector3(0f, markerHeight, 0f);

            return markerCenter;
        }

        private void DrawMarker(StaticMesh markerMesh, Vector3 markerCenter, float markerScale)
        {
            _sceneEffect.World = Matrix.CreateScale(markerScale, SceneValues.FullScale, markerScale) * Matrix.CreateTranslation(markerCenter);
            markerMesh.Draw(_graphicsDevice, _sceneEffect);
        }
    }
}
