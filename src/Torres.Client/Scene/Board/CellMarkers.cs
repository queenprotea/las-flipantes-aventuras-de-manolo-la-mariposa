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
        private const float FullScale = 1f;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly StaticMesh _hoverFrameMesh;
        private readonly StaticMesh _selectionRingMesh;
        private readonly StaticMesh _targetDiscMesh;

        private IReadOnlyList<BoardCell> _targetCells = Array.Empty<BoardCell>();
        private IReadOnlyList<BoardCell> _arrivalCells = Array.Empty<BoardCell>();
        private TimeSpan _arrivalTime;

        internal CellMarkers(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _hoverFrameMesh = SceneMeshes.Build(graphicsDevice, frameBuilder => CellFrameShapes.AddCellFrame(frameBuilder, GardenPalette.HoveredCellFrame));
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

        internal void Draw(BasicEffect sceneEffect, BoardLayout layout, TimeSpan sceneTime, CellFocus cellFocus)
        {
            float targetScale = FullScale + (TargetPulseGrowth * SceneTiming.FindCycleWave(sceneTime, TargetPulsePeriodSeconds));
            foreach (BoardCell targetCell in _targetCells)
            {
                DrawMarker(sceneEffect, layout, _targetDiscMesh, targetCell, targetScale);
            }

            float arrivalProgress = SceneTiming.FindProgress(sceneTime, _arrivalTime, ArrivalMarkSeconds);
            if (arrivalProgress < FullScale)
            {
                foreach (BoardCell arrivalCell in _arrivalCells)
                {
                    DrawMarker(sceneEffect, layout, _selectionRingMesh, arrivalCell, FullScale + (ArrivalMarkGrowth * arrivalProgress));
                }
            }

            if ((cellFocus.HoveredCell is BoardCell hoveredCell) && (hoveredCell != cellFocus.SelectedCell))
            {
                DrawMarker(sceneEffect, layout, _hoverFrameMesh, hoveredCell, FullScale);
            }

            if (cellFocus.SelectedCell is BoardCell selectedCell)
            {
                DrawMarker(sceneEffect, layout, _selectionRingMesh, selectedCell, FullScale);
            }

            sceneEffect.World = Matrix.Identity;
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

        private void DrawMarker(BasicEffect sceneEffect, BoardLayout layout, StaticMesh markerMesh, BoardCell markedCell, float markerScale)
        {
            float markerHeight = BoardGeometry.GetSurfaceHeight(layout, markedCell) + FrameLift;
            Vector3 markerCenter = BoardGeometry.GetCellCenter(markedCell) + new Vector3(0f, markerHeight, 0f);
            sceneEffect.World = Matrix.CreateScale(markerScale, FullScale, markerScale) * Matrix.CreateTranslation(markerCenter);
            markerMesh.Draw(_graphicsDevice, sceneEffect);
        }
    }
}
