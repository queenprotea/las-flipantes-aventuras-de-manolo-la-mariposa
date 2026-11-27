using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class TableShapes
    {
        internal const float TableTop = -0.16f;

        private const int FirstPlankIndex = -15;
        private const int LastPlankIndex = 9;
        private const float PlankPitch = 1.4f;
        private const float PlankDepth = 1.38f;
        private const float PlankHeight = 0.4f;
        private const float PlankWidth = 40f;
        private const int PlankColorPeriod = 2;
        private const float TraySpan = 8.9f;
        private const float TrayHeight = 0.2f;
        private const float TrayBottom = -0.2f;
        private const float RimThickness = 0.3f;
        private const float RimLength = 9f;
        private const float RimHeight = 0.22f;
        private const float RimBottom = -0.06f;
        private const float RimEdgeDistance = 4.35f;

        internal static void AddTable(MeshBuilder meshBuilder)
        {
            for (var plankIndex = FirstPlankIndex; plankIndex <= LastPlankIndex; plankIndex++)
            {
                bool isDarkPlank = (plankIndex % PlankColorPeriod) == 0;
                Matrix plankTransform = Matrix.CreateScale(PlankWidth, PlankHeight, PlankDepth)
                    * Matrix.CreateTranslation(0f, TableTop - PlankHeight, plankIndex * PlankPitch);
                Color plankColor = isDarkPlank ? GardenPalette.TablePlankDark : GardenPalette.TablePlank;
                meshBuilder.AddBox(plankTransform, plankColor);
            }

            meshBuilder.AddBox(Matrix.CreateScale(TraySpan, TrayHeight, TraySpan) * Matrix.CreateTranslation(0f, TrayBottom, 0f), GardenPalette.Tray);
            foreach (float sideSign in BoardGeometry.MirroredSideSigns)
            {
                meshBuilder.AddBox(Matrix.CreateScale(RimLength, RimHeight, RimThickness) * Matrix.CreateTranslation(0f, RimBottom, sideSign * RimEdgeDistance), GardenPalette.TrayRim);
                meshBuilder.AddBox(Matrix.CreateScale(RimThickness, RimHeight, RimLength) * Matrix.CreateTranslation(sideSign * RimEdgeDistance, RimBottom, 0f), GardenPalette.TrayRim);
            }
        }
    }
}
