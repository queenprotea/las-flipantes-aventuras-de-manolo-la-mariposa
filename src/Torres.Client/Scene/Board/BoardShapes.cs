using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class BoardShapes
    {
        private const int CheckerPeriod = 2;
        private const float BoardBaseThickness = 0.16f;
        private const float BoardBaseMargin = 0.3f;
        private const float BoardBaseSpan = (BoardLayout.CellsPerSide * BoardGeometry.CellSideLength) + BoardBaseMargin;
        private const float RimThickness = 0.12f;

        private const float RimOverhang = 0.005f;
        private const float RimLength = BoardGeometry.CellSideLength + (RimOverhang * 2f);
        private const float RimEdgeDistance = BoardGeometry.HalfCellSideLength + RimOverhang - (RimThickness / 2f);

        private static readonly Vector3 _rowRimSize = new Vector3(RimLength, BoardGeometry.PlanterTop, RimThickness);
        private static readonly Vector3 _columnRimSize = new Vector3(RimThickness, BoardGeometry.PlanterTop, RimLength);
        private static readonly BoardCell[] _neighborOffsets = new BoardCell[]
        {
            new BoardCell(-1, 0), new BoardCell(1, 0), new BoardCell(0, -1), new BoardCell(0, 1),
        };

        internal static void AddBoard(MeshBuilder meshBuilder, BoardLayout layout)
        {
            AddBoardBase(meshBuilder);
            foreach (BoardCell drawnCell in BoardLayout.EnumerateCells())
            {
                AddCellGround(meshBuilder, layout, drawnCell);
            }
        }

        private static void AddBoardBase(MeshBuilder meshBuilder)
        {
            Matrix baseTransform = Matrix.CreateScale(BoardBaseSpan, BoardBaseThickness, BoardBaseSpan)
                * Matrix.CreateTranslation(0f, -BoardBaseThickness, 0f);
            meshBuilder.AddBox(baseTransform, GardenPalette.BoardBase);
        }

        private static void AddCellGround(MeshBuilder meshBuilder, BoardLayout layout, BoardCell drawnCell)
        {
            Vector3 cellCenter = BoardGeometry.GetCellCenter(drawnCell);
            if (layout.IsPlanter(drawnCell))
            {
                meshBuilder.AddBox(CreateSlab(cellCenter, BoardGeometry.SoilTop), GardenPalette.PlanterSoil);
                AddPlanterRims(meshBuilder, layout, drawnCell);
            }
            else
            {
                bool isDarkSquare = ((drawnCell.Row + drawnCell.Column) % CheckerPeriod) == 0;
                Color grassColor = isDarkSquare ? GardenPalette.GrassDark : GardenPalette.GrassLight;
                meshBuilder.AddBox(CreateSlab(cellCenter, BoardGeometry.GrassTop), grassColor);
            }
        }

        private static Matrix CreateSlab(Vector3 cellCenter, float slabHeight)
        {
            Matrix slabTransform = Matrix.CreateScale(BoardGeometry.CellSideLength, slabHeight, BoardGeometry.CellSideLength)
                * Matrix.CreateTranslation(cellCenter);

            return slabTransform;
        }

        private static void AddPlanterRims(MeshBuilder meshBuilder, BoardLayout layout, BoardCell planterCell)
        {
            Vector3 cellCenter = BoardGeometry.GetCellCenter(planterCell);
            foreach (BoardCell neighborOffset in _neighborOffsets)
            {
                var neighborCell = new BoardCell(planterCell.Row + neighborOffset.Row, planterCell.Column + neighborOffset.Column);
                if (!layout.IsSamePlanter(planterCell, neighborCell))
                {
                    AddRim(meshBuilder, cellCenter, neighborOffset);
                }
            }
        }

        private static void AddRim(MeshBuilder meshBuilder, Vector3 cellCenter, BoardCell neighborOffset)
        {
            var rimOffset = new Vector3(neighborOffset.Column * RimEdgeDistance, 0f, neighborOffset.Row * RimEdgeDistance);
            Vector3 rimSize = neighborOffset.Row != 0 ? _rowRimSize : _columnRimSize;
            Matrix rimTransform = Matrix.CreateScale(rimSize) * Matrix.CreateTranslation(cellCenter + rimOffset);
            meshBuilder.AddBox(rimTransform, GardenPalette.PlanterRim);
        }
    }
}
