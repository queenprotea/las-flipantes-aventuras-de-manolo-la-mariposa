using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;

namespace Torres.Client.Ui
{
    internal static class Theme
    {
        internal const int WindowWidth = 1024;
        internal const int WindowHeight = 660;
        internal const int CardWidth = 560;
        internal const int Margin = 20;
        internal const int Spacing = 10;

        internal static Color Background { get; } = new Color(18, 26, 22);
        internal static Color Text { get; } = new Color(233, 240, 232);
        internal static Color MutedText { get; } = new Color(150, 168, 154);
        internal static Color Accent { get; } = new Color(138, 196, 108);
        internal static Color Danger { get; } = new Color(224, 122, 114);

        internal static IBrush CardBrush { get; } = new SolidBrush(new Color(30, 42, 36));
        internal static IBrush TopBarBrush { get; } = new SolidBrush(new Color(24, 34, 29));
        internal static IBrush LineBrush { get; } = new SolidBrush(new Color(58, 76, 64));

        internal static Thickness CardPadding { get; } = new Thickness(Margin);
    }
}
