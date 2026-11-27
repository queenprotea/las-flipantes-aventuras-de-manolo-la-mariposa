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
            FlowerParts.AddColumn(meshBuilder, PartPose.CreateUpright(Vector3.Zero, new Vector3(DiscRimRadius, DiscRimHeight, DiscRimRadius)), GardenPalette.TargetDiscRim);
            FlowerParts.AddColumn(meshBuilder, PartPose.CreateUpright(Vector3.Zero, new Vector3(DiscRadius, DiscHeight, DiscRadius)), GardenPalette.TargetDisc);
        }

        internal static void AddSelectionRing(MeshBuilder meshBuilder)
        {
            for (var beadIndex = 0; beadIndex < RingBeadCount; beadIndex++)
            {
                float headingDegrees = FlowerParts.GetCircleHeadingDegrees(beadIndex, RingBeadCount, 0f);
                FlowerParts.AddCoarsePart(meshBuilder, PartPose.CreateUpright(BoardGeometry.GetHeadingDirection(headingDegrees) * RingRadius, _ringBeadScale), GardenPalette.SelectionRing);
            }
        }
    }
}
