using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class GardenPalette
    {
        private const float ShadowOpacity = 0.16f;

        internal static Color Background { get; } = new Color(0xEE, 0xF5, 0xEA);
        internal static Color TablePlank { get; } = new Color(0xEA, 0xDF, 0xCB);
        internal static Color TablePlankDark { get; } = new Color(0xE3, 0xD6, 0xBF);
        internal static Color Tray { get; } = new Color(0xD8, 0xC3, 0xA0);
        internal static Color TrayRim { get; } = new Color(0xCD, 0xB4, 0x8A);
        internal static Color GrassLight { get; } = new Color(0xE3, 0xEF, 0xD8);
        internal static Color GrassDark { get; } = new Color(0xD5, 0xE5, 0xC8);
        internal static Color PlanterSoil { get; } = new Color(0xEF, 0xE4, 0xCC);
        internal static Color PlanterRim { get; } = new Color(0xC2, 0xAC, 0x7E);
        internal static Color Stem { get; } = new Color(0x6F, 0xAF, 0x8B);
        internal static Color Leaf { get; } = new Color(0xA8, 0xD5, 0xBA);
        internal static Color SilverLeaf { get; } = new Color(0x8F, 0xAE, 0x94);
        internal static Color ProteaBudBract { get; } = new Color(0xD7, 0xA3, 0xA0);
        internal static Color ProteaBudTip { get; } = new Color(0xC9, 0x99, 0x96);
        internal static Color ProteaOpeningBract { get; } = new Color(0xE8, 0xA1, 0xA8);
        internal static Color ProteaOpeningHeart { get; } = new Color(0xF1, 0xE3, 0xC6);
        internal static Color ProteaOuterBract { get; } = new Color(0xD9, 0x84, 0x8F);
        internal static Color ProteaInnerBract { get; } = new Color(0xEB, 0xA3, 0xAA);
        internal static Color ProteaDome { get; } = new Color(0xF3, 0xDD, 0xD3);
        internal static Color ProteaFloret { get; } = new Color(0xFB, 0xF3, 0xEE);
        internal static Color LavenderBudGreen { get; } = new Color(0xA9, 0xBF, 0xA0);
        internal static Color LavenderBudGray { get; } = new Color(0xB7, 0xB3, 0xC9);
        internal static Color LavenderBloom { get; } = new Color(0xB4, 0x9B, 0xDB);
        internal static Color LavenderDeepBloom { get; } = new Color(0x9A, 0x7F, 0xCB);
        internal static Color AlliumBud { get; } = new Color(0xCF, 0xC8, 0xDC);
        internal static Color AlliumBudTip { get; } = new Color(0xB9, 0xC9, 0xA8);
        internal static Color AlliumFloret { get; } = new Color(0x8E, 0x6B, 0xB8);
        internal static Color AlliumLightFloret { get; } = new Color(0xA4, 0x87, 0xCC);
        internal static Color ConeflowerBud { get; } = new Color(0x9D, 0xBF, 0x8A);
        internal static Color ConeflowerSepal { get; } = new Color(0x8D, 0xB2, 0x7C);
        internal static Color ConeflowerPetal { get; } = new Color(0xF0, 0xA3, 0x83);
        internal static Color ConeflowerCone { get; } = new Color(0xB8, 0x6A, 0x3C);
        internal static Color NarcissusBud { get; } = new Color(0xD9, 0xDC, 0xC0);
        internal static Color NarcissusPetal { get; } = new Color(0xF6, 0xF2, 0xE4);
        internal static Color NarcissusCup { get; } = new Color(0xF2, 0xC4, 0x8E);
        internal static Color NarcissusCupRim { get; } = new Color(0xE9, 0xA9, 0x6A);
        internal static Color CaterpillarEye { get; } = new Color(0x2B, 0x2B, 0x2B);
        internal static Color CaterpillarCheek { get; } = new Color(0xF4, 0xA6, 0xA0);
        internal static Color Shadow { get; } = new Color(0x37, 0x47, 0x3C) * ShadowOpacity;
        internal static Color HoveredCellFrame { get; } = new Color(0xFF, 0xFF, 0xFF);
        internal static Color DewLeaf { get; } = new Color(0x79, 0xB8, 0x92);
        internal static Color DewLeafEdge { get; } = new Color(0x5E, 0x9C, 0x78);
        internal static Color DewLeafCream { get; } = new Color(0xF1, 0xF3, 0xD2);
        internal static Color DewLeafRib { get; } = new Color(0xD6, 0xEB, 0xCF);
        internal static Color DewDrop { get; } = new Color(0x8E, 0xC5, 0xE8);
        internal static Color ExtraDewDrop { get; } = new Color(0xF2, 0xD2, 0x7A);
        internal static Color CardEdge { get; } = new Color(0xCF, 0xDC, 0xC4);
        internal static Color DeckCardEdge { get; } = new Color(0x8C, 0xC9, 0xA6);
        internal static Color ChosenCardOutline { get; } = new Color(0x5F, 0xB3, 0x7F);
        internal static Color TargetDisc { get; } = new Color(0x9E, 0xD9, 0xBC);
        internal static Color TargetDiscRim { get; } = new Color(0x5F, 0xB3, 0x7F);
        internal static Color SelectionRing { get; } = new Color(0x3E, 0x9A, 0x68);
        internal static Color ButterflyBody { get; } = new Color(0x5F, 0x71, 0x65);
        internal static Color ButterflyLeftWing { get; } = new Color(0xF5, 0xC9, 0xC6);
        internal static Color ButterflyRightWing { get; } = new Color(0xBF, 0xE0, 0xEC);
        internal static Color ButterflyLowerWing { get; } = new Color(0xF7, 0xE9, 0xB0);
        internal static Color ButterflyLeftWingRim { get; } = new Color(0xD8, 0x9A, 0x96);
        internal static Color ButterflyRightWingRim { get; } = new Color(0x8F, 0xBF, 0xD1);
        internal static Color ButterflyLowerWingRim { get; } = new Color(0xD9, 0xC2, 0x71);
    }
}
