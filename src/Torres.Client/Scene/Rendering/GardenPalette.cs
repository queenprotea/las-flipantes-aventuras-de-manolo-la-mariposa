using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class GardenPalette
    {
        private const float ShadowOpacity = 0.16f;

        internal static Color Background { get; } = new Color(0xF7, 0xFA, 0xF3);
        internal static Color BoardBase { get; } = new Color(0xB9, 0xD3, 0xA6);
        internal static Color GrassLight { get; } = new Color(0xE3, 0xEF, 0xD8);
        internal static Color GrassDark { get; } = new Color(0xD5, 0xE5, 0xC8);
        internal static Color PlanterSoil { get; } = new Color(0xEF, 0xE4, 0xCC);
        internal static Color PlanterRim { get; } = new Color(0xC2, 0xAC, 0x7E);
        internal static Color Stem { get; } = new Color(0x6F, 0xAF, 0x8B);
        internal static Color Leaf { get; } = new Color(0xA8, 0xD5, 0xBA);
        internal static Color Petal { get; } = new Color(0xF5, 0xC9, 0xC6);
        internal static Color BlossomHeart { get; } = new Color(0xF7, 0xE9, 0xB0);
        internal static Color CaterpillarEye { get; } = new Color(0x2B, 0x2B, 0x2B);
        internal static Color Shadow { get; } = new Color(0x37, 0x47, 0x3C) * ShadowOpacity;
        internal static Color SelectedCellFrame { get; } = new Color(0x5E, 0x9A, 0x78);
        internal static Color HoveredCellFrame { get; } = new Color(0xFF, 0xFF, 0xFF);
        internal static Color PlacementTargetFrame { get; } = new Color(0xF2, 0xB8, 0x4B);
        internal static Color ButterflyBody { get; } = new Color(0x5F, 0x71, 0x65);
        internal static Color ButterflyLeftWing { get; } = new Color(0xF5, 0xC9, 0xC6);
        internal static Color ButterflyRightWing { get; } = new Color(0xBF, 0xE0, 0xEC);
        internal static Color ButterflyLowerWing { get; } = new Color(0xF7, 0xE9, 0xB0);
        internal static Color ButterflyLeftWingRim { get; } = new Color(0xD8, 0x9A, 0x96);
        internal static Color ButterflyRightWingRim { get; } = new Color(0x8F, 0xBF, 0xD1);
        internal static Color ButterflyLowerWingRim { get; } = new Color(0xD9, 0xC2, 0x71);
    }
}
