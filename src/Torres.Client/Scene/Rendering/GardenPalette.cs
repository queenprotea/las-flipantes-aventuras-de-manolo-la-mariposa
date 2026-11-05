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
        internal static Color Soil { get; } = new Color(0xEF, 0xE4, 0xCC);
        internal static Color Brick { get; } = new Color(0xC2, 0xAC, 0x7E);
        internal static Color Stem { get; } = new Color(0x6F, 0xAF, 0x8B);
        internal static Color Leaf { get; } = new Color(0xA8, 0xD5, 0xBA);
        internal static Color Petal { get; } = new Color(0xF5, 0xC9, 0xC6);
        internal static Color Heart { get; } = new Color(0xF7, 0xE9, 0xB0);
        internal static Color Eye { get; } = new Color(0x2B, 0x2B, 0x2B);
        internal static Color Shadow { get; } = new Color(0x37, 0x47, 0x3C) * ShadowOpacity;
        internal static Color Selection { get; } = new Color(0x5E, 0x9A, 0x78);
        internal static Color Hover { get; } = new Color(0xFF, 0xFF, 0xFF);
        internal static Color Target { get; } = new Color(0xF2, 0xB8, 0x4B);
        internal static Color Butterfly { get; } = new Color(0xB4, 0x9C, 0xE0);
    }
}
