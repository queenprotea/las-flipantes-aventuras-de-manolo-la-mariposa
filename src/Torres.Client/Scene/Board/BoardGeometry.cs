using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class BoardGeometry
    {
        internal const float CellSize = 1f;
        internal const float HalfCell = CellSize / 2f;
        internal const float GrassTop = 0.06f;
        internal const float PlanterTop = 0.30f;
        internal const float SoilTop = 0.26f;
        internal const float LevelStep = 0.34f;

        private const float HalfSpan = (BoardLayout.Size - 1) * HalfCell;
        private const float FlowerTopOffset = 0.15f;

        internal static IReadOnlyList<float> MirroredSides { get; } = new float[] { -1f, 1f };

        internal static Vector3 GetCellCenter(BoardCell cell)
        {
            var cellCenter = new Vector3(cell.Column - HalfSpan, 0f, cell.Row - HalfSpan);

            return cellCenter;
        }

        internal static float GetSurfaceHeight(BoardLayout layout, BoardCell cell)
        {
            float surfaceHeight = layout.IsPlanter(cell) ? PlanterTop : GrassTop;

            return surfaceHeight;
        }

        internal static float GetFlowerTop(int flowerLevel)
        {
            float flowerTop = PlanterTop + FlowerTopOffset + (LevelStep * flowerLevel);

            return flowerTop;
        }

        internal static Matrix CreateHeadingRotation(float headingDegrees)
        {
            float headingRadians = MathHelper.ToRadians(headingDegrees);
            Matrix headingRotation = Matrix.CreateRotationY(headingRadians);

            return headingRotation;
        }

        internal static Vector3 GetHeadingDirection(float headingDegrees)
        {
            Matrix headingRotation = CreateHeadingRotation(headingDegrees);
            Vector3 headingDirection = Vector3.TransformNormal(Vector3.UnitX, headingRotation);

            return headingDirection;
        }
    }
}
