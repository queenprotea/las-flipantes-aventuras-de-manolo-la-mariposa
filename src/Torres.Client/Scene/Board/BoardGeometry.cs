using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class BoardGeometry
    {
        internal const float CellSideLength = 1f;
        internal const float HalfCellSideLength = CellSideLength / 2f;
        internal const float GrassTop = 0.06f;
        internal const float PlanterTop = 0.30f;
        internal const float SoilTop = 0.26f;
        internal const float FlowerLevelHeight = 0.34f;

        private const float HalfSpan = (BoardLayout.CellsPerSide - 1) * HalfCellSideLength;
        private const float FlowerTopOffset = 0.15f;

        internal static IReadOnlyList<float> MirroredSideSigns { get; } = new float[] { -1f, 1f };

        internal static Vector3 GetCellCenter(BoardCell examinedCell)
        {
            var cellCenter = new Vector3(examinedCell.Column - HalfSpan, 0f, examinedCell.Row - HalfSpan);

            return cellCenter;
        }

        internal static BoardCell? FindCellAt(Vector3 worldPoint)
        {
            var nearestCell = new BoardCell((int)MathF.Round(worldPoint.Z + HalfSpan), (int)MathF.Round(worldPoint.X + HalfSpan));
            bool isRowInside = (nearestCell.Row >= 0) && (nearestCell.Row < BoardLayout.CellsPerSide);
            bool isColumnInside = (nearestCell.Column >= 0) && (nearestCell.Column < BoardLayout.CellsPerSide);
            BoardCell? foundCell = null;
            if (isRowInside && isColumnInside)
            {
                foundCell = nearestCell;
            }

            return foundCell;
        }

        internal static float GetSurfaceHeight(BoardLayout layout, BoardCell examinedCell)
        {
            float surfaceHeight = layout.IsPlanter(examinedCell) ? PlanterTop : GrassTop;

            return surfaceHeight;
        }

        internal static float GetFlowerTop(int flowerLevel)
        {
            float flowerTop = PlanterTop + FlowerTopOffset + (FlowerLevelHeight * flowerLevel);

            return flowerTop;
        }

        internal static Matrix CreateHeadingRotation(float headingDegrees)
        {
            float headingRadians = MathHelper.ToRadians(headingDegrees);
            Matrix headingRotation = Matrix.CreateRotationY(headingRadians);

            return headingRotation;
        }

        internal static float GetHeadingDegrees(Vector3 travelDirection)
        {
            float headingDegrees = MathHelper.ToDegrees(MathF.Atan2(-travelDirection.Z, travelDirection.X));

            return headingDegrees;
        }

        internal static Vector3 GetHeadingDirection(float headingDegrees)
        {
            Matrix headingRotation = CreateHeadingRotation(headingDegrees);
            Vector3 headingDirection = Vector3.TransformNormal(Vector3.UnitX, headingRotation);

            return headingDirection;
        }
    }
}
