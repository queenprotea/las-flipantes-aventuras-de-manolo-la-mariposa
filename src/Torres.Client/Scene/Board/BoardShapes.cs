using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class BoardShapes
    {
        private const int CheckerPeriod = 2;
        private const float BoardBaseThickness = 0.16f;
        private const float BoardBaseMargin = 0.3f;
        private const float BoardBaseSpan = (BoardLayout.Size * BoardGeometry.CellSize) + BoardBaseMargin;
        private const float RimThickness = 0.12f;

        // The rim sticks out a little so it does not share a plane with the soil and flicker.
        private const float RimOverhang = 0.005f;
        private const float RimLength = BoardGeometry.CellSize + (RimOverhang * 2f);
        private const float RimEdgeDistance = BoardGeometry.HalfCell + RimOverhang - (RimThickness / 2f);

        private static readonly Vector3 _rowRimSize = new Vector3(RimLength, BoardGeometry.PlanterTop, RimThickness);
        private static readonly Vector3 _columnRimSize = new Vector3(RimThickness, BoardGeometry.PlanterTop, RimLength);
        private static readonly BoardCell[] _neighborOffsets = new BoardCell[]
        {
            new BoardCell(-1, 0), new BoardCell(1, 0), new BoardCell(0, -1), new BoardCell(0, 1),
        };

        internal static void AddBoard(MeshBuilder meshBuilder, BoardLayout layout)
        {
            AddBoardBase(meshBuilder);
            foreach (BoardCell cell in BoardLayout.EnumerateCells())
            {
                AddCellGround(meshBuilder, layout, cell);
            }
        }

        private static void AddBoardBase(MeshBuilder meshBuilder)
        {
            Matrix baseTransform = Matrix.CreateScale(BoardBaseSpan, BoardBaseThickness, BoardBaseSpan)
                * Matrix.CreateTranslation(0f, -BoardBaseThickness, 0f);
            meshBuilder.AddBox(baseTransform, GardenPalette.BoardBase);
        }

        private static void AddCellGround(MeshBuilder meshBuilder, BoardLayout layout, BoardCell cell)
        {
            Vector3 cellCenter = BoardGeometry.GetCellCenter(cell);
            if (layout.IsPlanter(cell))
            {
                meshBuilder.AddBox(CreateSlab(cellCenter, BoardGeometry.SoilTop), GardenPalette.Soil);
                AddPlanterRims(meshBuilder, layout, cell);
            }
            else
            {
                bool isDarkSquare = ((cell.Row + cell.Column) % CheckerPeriod) == 0;
                Color grassColor = isDarkSquare ? GardenPalette.GrassDark : GardenPalette.GrassLight;
                meshBuilder.AddBox(CreateSlab(cellCenter, BoardGeometry.GrassTop), grassColor);
            }
        }

        private static Matrix CreateSlab(Vector3 cellCenter, float slabHeight)
        {
            Matrix slabTransform = Matrix.CreateScale(BoardGeometry.CellSize, slabHeight, BoardGeometry.CellSize)
                * Matrix.CreateTranslation(cellCenter);

            return slabTransform;
        }

        // The rim rises only where the neighbor is not part of the same planter, so an L-shaped planter reads as one piece.
        private static void AddPlanterRims(MeshBuilder meshBuilder, BoardLayout layout, BoardCell cell)
        {
            Vector3 cellCenter = BoardGeometry.GetCellCenter(cell);
            foreach (BoardCell offset in _neighborOffsets)
            {
                var neighbor = new BoardCell(cell.Row + offset.Row, cell.Column + offset.Column);
                if (!layout.IsSamePlanter(cell, neighbor))
                {
                    AddRim(meshBuilder, cellCenter, offset);
                }
            }
        }

        private static void AddRim(MeshBuilder meshBuilder, Vector3 cellCenter, BoardCell offset)
        {
            var rimOffset = new Vector3(offset.Column * RimEdgeDistance, 0f, offset.Row * RimEdgeDistance);
            Vector3 rimSize = offset.Row != 0 ? _rowRimSize : _columnRimSize;
            Matrix rimTransform = Matrix.CreateScale(rimSize) * Matrix.CreateTranslation(cellCenter + rimOffset);
            meshBuilder.AddBox(rimTransform, GardenPalette.Brick);
        }
    }
}
