using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class CellMarkerShapes
    {
        private const float DiscRimRadius = 0.36f;
        private const float DiscRimHeight = 0.018f;
        private const float DiscRadius = 0.32f;
        private const float DiscHeight = 0.03f;
        private const int RingBeadCount = 28;
        private const float RingRadius = 0.43f;

        private static readonly Vector3 _ringBeadScale = new Vector3(0.05f, 0.025f, 0.05f);

        internal static void AddTargetDisc(MeshBuilder meshBuilder)
        {
            FlowerParts.AddColumn(meshBuilder, Vector3.Zero, DiscRimHeight, DiscRimRadius, GardenPalette.TargetDiscRim);
            FlowerParts.AddColumn(meshBuilder, Vector3.Zero, DiscHeight, DiscRadius, GardenPalette.TargetDisc);
        }

        internal static void AddSelectionRing(MeshBuilder meshBuilder)
        {
            for (var beadIndex = 0; beadIndex < RingBeadCount; beadIndex++)
            {
                float headingDegrees = FlowerParts.GetCircleHeading(beadIndex, RingBeadCount, 0f);
                FlowerParts.AddBead(meshBuilder, BoardGeometry.GetHeadingDirection(headingDegrees) * RingRadius, _ringBeadScale, GardenPalette.SelectionRing);
            }
        }
    }
}
