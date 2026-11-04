using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    // Each cell gives one box for its ground and more for what stands on it. The nearest hit wins, so a click on a tall
    // flower picks its own cell and not the one behind; the stem box is narrow so it does not cover the cell behind it.
    internal sealed class CellPicker
    {
        private const float BlossomHalfWidth = 0.2f;
        private const float BlossomDepth = 0.06f;
        private const float StemHalfWidth = 0.06f;
        private const float FlowerHeadroom = 0.08f;
        private const float CaterpillarHeadroom = 0.35f;
        private const float GroundCaterpillarHalfWidth = 0.45f;
        private const float GroundCaterpillarHeight = 0.32f;

        private readonly List<PickTarget> _pickTargets = new List<PickTarget>();

        internal CellPicker(BoardLayout layout)
        {
            foreach (BoardCell cell in BoardLayout.EnumerateCells())
            {
                AddCellTargets(layout, cell);
            }
        }

        internal BoardCell? Pick(Ray pickRay)
        {
            BoardCell? nearestCell = null;
            float nearestDistance = float.MaxValue;
            foreach (PickTarget pickTarget in _pickTargets)
            {
                float? hitDistance = pickRay.Intersects(pickTarget.Bounds);
                if (hitDistance.HasValue && (hitDistance.Value < nearestDistance))
                {
                    nearestDistance = hitDistance.Value;
                    nearestCell = pickTarget.Cell;
                }
            }

            return nearestCell;
        }

        private static BoundingBox CreateColumn(Vector3 columnBase, float halfWidth, float columnHeight)
        {
            var minimumCorner = new Vector3(columnBase.X - halfWidth, columnBase.Y, columnBase.Z - halfWidth);
            var maximumCorner = new Vector3(columnBase.X + halfWidth, columnBase.Y + columnHeight, columnBase.Z + halfWidth);
            var columnBounds = new BoundingBox(minimumCorner, maximumCorner);

            return columnBounds;
        }

        private void AddCellTargets(BoardLayout layout, BoardCell cell)
        {
            Vector3 cellCenter = BoardGeometry.GetCellCenter(cell);
            float surfaceHeight = BoardGeometry.GetSurfaceHeight(layout, cell);
            AddTarget(cell, CreateColumn(cellCenter, BoardGeometry.HalfCell, surfaceHeight));

            Vector3 surfacePoint = cellCenter + new Vector3(0f, surfaceHeight, 0f);
            if (layout.GetFlowerLevel(cell) > 0)
            {
                AddFlowerTargets(layout, cell, surfacePoint);
            }
            else if (layout.HasCaterpillar(cell))
            {
                AddTarget(cell, CreateColumn(surfacePoint, GroundCaterpillarHalfWidth, GroundCaterpillarHeight));
            }
        }

        private void AddFlowerTargets(BoardLayout layout, BoardCell cell, Vector3 surfacePoint)
        {
            float headroom = layout.HasCaterpillar(cell) ? CaterpillarHeadroom : FlowerHeadroom;
            float flowerTop = BoardGeometry.GetFlowerTop(layout.GetFlowerLevel(cell));
            var blossomBase = new Vector3(surfacePoint.X, flowerTop - BlossomDepth, surfacePoint.Z);
            AddTarget(cell, CreateColumn(surfacePoint, StemHalfWidth, flowerTop - surfacePoint.Y));
            AddTarget(cell, CreateColumn(blossomBase, BlossomHalfWidth, BlossomDepth + headroom));
        }

        private void AddTarget(BoardCell cell, BoundingBox bounds)
        {
            _pickTargets.Add(new PickTarget(bounds, cell));
        }
    }
}
