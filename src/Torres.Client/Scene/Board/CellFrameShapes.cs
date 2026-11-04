using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    // The frame is built around the origin and placed on a cell with the World matrix, so moving it never rebuilds a mesh.
    internal static class CellFrameShapes
    {
        private const float FrameHalfSpan = 0.44f;
        private const float BarThickness = 0.06f;
        private const float BarHeight = 0.02f;
        private const float BarLength = (FrameHalfSpan * 2f) + BarThickness;

        private static readonly Vector3 _acrossBarSize = new Vector3(BarLength, BarHeight, BarThickness);
        private static readonly Vector3 _alongBarSize = new Vector3(BarThickness, BarHeight, BarLength);

        internal static void AddCellFrame(MeshBuilder meshBuilder, Color frameColor)
        {
            meshBuilder.AddBox(CreateBar(_acrossBarSize, new Vector3(0f, 0f, -FrameHalfSpan)), frameColor);
            meshBuilder.AddBox(CreateBar(_acrossBarSize, new Vector3(0f, 0f, FrameHalfSpan)), frameColor);
            meshBuilder.AddBox(CreateBar(_alongBarSize, new Vector3(-FrameHalfSpan, 0f, 0f)), frameColor);
            meshBuilder.AddBox(CreateBar(_alongBarSize, new Vector3(FrameHalfSpan, 0f, 0f)), frameColor);
        }

        private static Matrix CreateBar(Vector3 barSize, Vector3 barCenter)
        {
            Matrix barTransform = Matrix.CreateScale(barSize) * Matrix.CreateTranslation(barCenter);

            return barTransform;
        }
    }
}
