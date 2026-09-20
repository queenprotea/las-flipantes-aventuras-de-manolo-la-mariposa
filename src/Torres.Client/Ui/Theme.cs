using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;

namespace Torres.Client.Ui
{
    /// <summary>
    /// Paleta, medidas y pinceles del cliente. Los valores salen de las variables CSS y de las
    /// reglas de docs/prototipo/Prototipo-Pantallas-Torres.html: el prototipo manda sobre esta clase.
    /// </summary>
    internal static class Theme
    {
        internal const int WindowWidth = 1024;
        internal const int WindowHeight = 660;

        // .screen { padding: 20px 22px }
        internal const int ScreenPaddingX = 22;
        internal const int ScreenPaddingY = 20;

        // .card { padding: 18px }
        internal const int CardPaddingSize = 18;

        // .card > h3 { margin-bottom: 2px }
        internal const int CardHeaderSpacing = 2;

        // .field { margin-bottom: 13px } y label.f { margin: 0 0 5px }
        internal const int FieldSpacing = 13;
        internal const int FieldLabelSpacing = 5;

        // .btns { gap: 9px }
        internal const int ButtonSpacing = 9;

        // .row { gap: 16px }
        internal const int ColumnSpacing = 16;

        // .mainrow { gap: 26px }
        internal const int MenuRowSpacing = 26;

        // .appbar { padding-bottom: 14px; margin-bottom: 20px }
        internal const int AppBarSpacing = 14;
        internal const int AppBarBottomSpacing = 20;

        // .list { gap: 8px } y .langlist { gap: 6px }
        internal const int ListSpacing = 6;

        // .mlist hr { margin: 8px 4px }
        internal const int MenuSeparatorSpacing = 8;
        internal const int MenuSeparatorInset = 4;

        internal const int BorderSize = 1;

        // Anchos de tarjeta declarados en cada pantalla del prototipo.
        internal const int LoginCardWidth = 348;
        internal const int RegisterCardWidth = 360;
        internal const int GuestCardWidth = 340;
        internal const int LanguageCardWidth = 340;
        internal const int RecoveryCardWidth = 290;
        internal const int ProfileAvatarCardWidth = 230;
        internal const int ProfileAccountCardWidth = 360;
        internal const int AccountSettingsCardWidth = 400;

        // .mlist { flex: 0 0 268px } y .av.lg { width: 92px }
        internal const int MenuListWidth = 268;
        internal const int LargeAvatarSize = 92;

        // --surface, --surface-2, --surface-3
        internal static Color Surface { get; } = new Color(0xFF, 0xFF, 0xFF);
        internal static Color SurfaceAlt { get; } = new Color(0xED, 0xF3, 0xE6);
        internal static Color SurfaceSunken { get; } = new Color(0xF9, 0xFB, 0xF5);

        // --line, --line-2
        internal static Color Line { get; } = new Color(0xE2, 0xEA, 0xDA);
        internal static Color StrongLine { get; } = new Color(0xCF, 0xDC, 0xC4);

        // --ink, --ink-2, --ink-3
        internal static Color Ink { get; } = new Color(0x37, 0x47, 0x3C);
        internal static Color SoftInk { get; } = new Color(0x5F, 0x71, 0x65);
        internal static Color MutedInk { get; } = new Color(0x8D, 0xA0, 0x93);

        // --mint, --mint-2, --mint-3, --mint-ink, --mint-line
        internal static Color Mint { get; } = new Color(0xA8, 0xD5, 0xBA);
        internal static Color MintAlt { get; } = new Color(0xDC, 0xEF, 0xE2);
        internal static Color MintTint { get; } = new Color(0xF0, 0xF8, 0xF2);
        internal static Color MintInk { get; } = new Color(0x22, 0x40, 0x2F);
        internal static Color MintLine { get; } = new Color(0x84, 0xC1, 0xA0);

        // --lav-ink
        internal static Color LavenderInk { get; } = new Color(0x3B, 0x35, 0x68);

        // --blush, --blush-2, --blush-ink
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
        internal static IBrush MintLineBrush { get; } = new SolidBrush(MintLine);
        internal static IBrush BlushBrush { get; } = new SolidBrush(Blush);
        internal static IBrush BlushTintBrush { get; } = new SolidBrush(BlushTint);

        internal static Thickness CardPadding { get; } = new Thickness(CardPaddingSize);

        // .inp { padding: 9px 12px }
        internal static Thickness InputPadding { get; } = new Thickness(12, 9);

        // .btn { padding: 9px 16px }
        internal static Thickness ButtonPadding { get; } = new Thickness(16, 9);

        // .who button { padding: 5px 13px }
        internal static Thickness PillButtonPadding { get; } = new Thickness(13, 5);

        // .btn.sm con padding-left a cero: el enlace del pie de PT-03
        internal static Thickness LinkButtonPadding { get; } = new Thickness(0, 5, 11, 5);

        // .btn.sm { padding: 5px 11px }
        internal static Thickness SmallButtonPadding { get; } = new Thickness(11, 5);

        // .mlist button { padding: 11px 12px }
        internal static Thickness MenuItemPadding { get; } = new Thickness(12, 11);

        // .langbtn y .langlist button { padding: 11px 14px }
        internal static Thickness ListItemPadding { get; } = new Thickness(14, 11);

        internal static Thickness Border { get; } = new Thickness(BorderSize);
    }
}
