using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal sealed class CellPicker
    {
        private const float BlossomHalfWidth = 0.2f;
        private const float BlossomDepth = 0.06f;
        private const float StemHalfWidth = 0.06f;
        private const float FlowerHeadroom = 0.08f;
        private const float CaterpillarHeadroom = 0.35f;
        private const float GroundCaterpillarHalfWidth = 0.45f;
        private const float GroundCaterpillarHeight = 0.32f;

        private readonly List<PickVolume> _pickVolumes = new List<PickVolume>();

        internal CellPicker(BoardLayout layout)
        {
            foreach (BoardCell pickableCell in BoardLayout.EnumerateCells())
            {
                AddCellVolumes(layout, pickableCell);
            }
        }

        internal BoardCell? PickCell(Ray pickRay)
        {
            BoardCell? nearestCell = null;
            float nearestDistance = float.MaxValue;
            foreach (PickVolume pickVolume in _pickVolumes)
            {
                float? hitDistance = pickRay.Intersects(pickVolume.Bounds);
                if (hitDistance.HasValue && (hitDistance.Value < nearestDistance))
                {
                    nearestDistance = hitDistance.Value;
                    nearestCell = pickVolume.Cell;
                }
            }

            return nearestCell;
        }

        private static BoundingBox CreatePickBox(Vector3 boxBase, float halfWidth, float boxHeight)
        {
            var minimumCorner = new Vector3(boxBase.X - halfWidth, boxBase.Y, boxBase.Z - halfWidth);
            var maximumCorner = new Vector3(boxBase.X + halfWidth, boxBase.Y + boxHeight, boxBase.Z + halfWidth);
            var boxBounds = new BoundingBox(minimumCorner, maximumCorner);

            return boxBounds;
        }

        private void AddCellVolumes(BoardLayout layout, BoardCell pickableCell)
        {
            Vector3 cellCenter = BoardGeometry.GetCellCenter(pickableCell);
            float surfaceHeight = BoardGeometry.GetSurfaceHeight(layout, pickableCell);
            AddPickVolume(pickableCell, CreatePickBox(cellCenter, BoardGeometry.HalfCellSideLength, surfaceHeight));

            Vector3 surfacePoint = cellCenter + new Vector3(0f, surfaceHeight, 0f);
            if (layout.GetFlowerLevel(pickableCell) > 0)
            {
                AddFlowerVolumes(layout, pickableCell, surfacePoint);
            }
            else if (layout.HasCaterpillar(pickableCell))
            {
                AddPickVolume(pickableCell, CreatePickBox(surfacePoint, GroundCaterpillarHalfWidth, GroundCaterpillarHeight));
            }
        }

        private void AddFlowerVolumes(BoardLayout layout, BoardCell flowerCell, Vector3 surfacePoint)
        {
            float pickHeadroom = layout.HasCaterpillar(flowerCell) ? CaterpillarHeadroom : FlowerHeadroom;
            float flowerTop = BoardGeometry.GetFlowerTop(layout.GetFlowerLevel(flowerCell));
            var blossomBase = new Vector3(surfacePoint.X, flowerTop - BlossomDepth, surfacePoint.Z);
            AddPickVolume(flowerCell, CreatePickBox(surfacePoint, StemHalfWidth, flowerTop - surfacePoint.Y));
            AddPickVolume(flowerCell, CreatePickBox(blossomBase, BlossomHalfWidth, BlossomDepth + pickHeadroom));
        }

        private void AddPickVolume(BoardCell volumeCell, BoundingBox volumeBounds)
        {
            _pickVolumes.Add(new PickVolume(volumeBounds, volumeCell));
        }
    }
}
