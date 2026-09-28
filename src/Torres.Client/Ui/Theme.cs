using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;

namespace Torres.Client.Ui
{
    internal static class Theme
    {
        internal static Color Surface { get; } = new Color(0xFF, 0xFF, 0xFF);
        internal static Color SurfaceAlt { get; } = new Color(0xED, 0xF3, 0xE6);
        internal static Color SurfaceSunken { get; } = new Color(0xF9, 0xFB, 0xF5);

        internal static Color Line { get; } = new Color(0xE2, 0xEA, 0xDA);
        internal static Color StrongLine { get; } = new Color(0xCF, 0xDC, 0xC4);

        internal static Color Ink { get; } = new Color(0x37, 0x47, 0x3C);
        internal static Color SoftInk { get; } = new Color(0x5F, 0x71, 0x65);
        internal static Color MutedInk { get; } = new Color(0x8D, 0xA0, 0x93);

        internal static Color Mint { get; } = new Color(0xA8, 0xD5, 0xBA);
        internal static Color MintAlt { get; } = new Color(0xDC, 0xEF, 0xE2);
        internal static Color MintTint { get; } = new Color(0xF0, 0xF8, 0xF2);
        internal static Color MintInk { get; } = new Color(0x22, 0x40, 0x2F);
        internal static Color MintLine { get; } = new Color(0x84, 0xC1, 0xA0);

        internal static Color Lavender { get; } = new Color(0xC9, 0xC2, 0xEC);
        internal static Color LavenderTint { get; } = new Color(0xEB, 0xE8, 0xF8);
        internal static Color LavenderInk { get; } = new Color(0x3B, 0x35, 0x68);
        internal static Color LavenderLine { get; } = new Color(0xA7, 0x9D, 0xDD);

        internal static Color Butter { get; } = new Color(0xF7, 0xE9, 0xB0);
        internal static Color ButterTint { get; } = new Color(0xFC, 0xF6, 0xDF);
        internal static Color ButterInk { get; } = new Color(0x6B, 0x5A, 0x20);

        internal static Color Sky { get; } = new Color(0xBF, 0xE0, 0xEC);
        internal static Color SkyTint { get; } = new Color(0xE7, 0xF4, 0xF8);

        internal static Color Blush { get; } = new Color(0xF5, 0xC9, 0xC6);
        internal static Color BlushTint { get; } = new Color(0xFB, 0xEA, 0xE8);
        internal static Color BlushInk { get; } = new Color(0x8E, 0x4B, 0x47);

        internal static IBrush SurfaceBrush { get; } = new SolidBrush(Surface);
        internal static IBrush SurfaceAltBrush { get; } = new SolidBrush(SurfaceAlt);
        internal static IBrush SurfaceSunkenBrush { get; } = new SolidBrush(SurfaceSunken);
        internal static IBrush LineBrush { get; } = new SolidBrush(Line);
        internal static IBrush StrongLineBrush { get; } = new SolidBrush(StrongLine);
        internal static IBrush MintBrush { get; } = new SolidBrush(Mint);
        internal static IBrush MintAltBrush { get; } = new SolidBrush(MintAlt);
        internal static IBrush MintTintBrush { get; } = new SolidBrush(MintTint);
        internal static IBrush MintInkBrush { get; } = new SolidBrush(MintInk);
        internal static IBrush MintLineBrush { get; } = new SolidBrush(MintLine);
        internal static IBrush LavenderBrush { get; } = new SolidBrush(Lavender);
        internal static IBrush LavenderTintBrush { get; } = new SolidBrush(LavenderTint);
        internal static IBrush LavenderLineBrush { get; } = new SolidBrush(LavenderLine);
        internal static IBrush ButterBrush { get; } = new SolidBrush(Butter);
        internal static IBrush ButterTintBrush { get; } = new SolidBrush(ButterTint);
        internal static IBrush SkyBrush { get; } = new SolidBrush(Sky);
        internal static IBrush SkyTintBrush { get; } = new SolidBrush(SkyTint);
        internal static IBrush BlushBrush { get; } = new SolidBrush(Blush);
        internal static IBrush BlushTintBrush { get; } = new SolidBrush(BlushTint);
    }
}
