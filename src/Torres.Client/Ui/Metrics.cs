using Myra.Graphics2D;

namespace Torres.Client.Ui
{
    internal static class Metrics
    {
        internal const int ScreenPaddingX = 22;
        internal const int ScreenPaddingY = 20;

        internal const int CardPaddingSize = 18;

        internal const int CardHeaderSpacing = 2;

        internal const int FieldSpacing = 13;
        internal const int FieldLabelSpacing = 5;

        internal const int ButtonSpacing = 9;

        internal const int ColumnSpacing = 16;

        internal const int MenuRowSpacing = 26;

        internal const int AppBarSpacing = 14;
        internal const int AppBarBottomSpacing = 20;

        internal const int ListSpacing = 6;

        internal const int MenuSeparatorSpacing = 8;
        internal const int MenuSeparatorInset = 4;

        internal static Thickness CardPadding { get; } = new Thickness(CardPaddingSize);

        internal static Thickness InputPadding { get; } = new Thickness(12, 9);

        internal static Thickness ButtonPadding { get; } = new Thickness(16, 9);

        internal static Thickness PillButtonPadding { get; } = new Thickness(13, 5);

        internal static Thickness LinkButtonPadding { get; } = new Thickness(0, 5, 11, 5);

        internal static Thickness SmallButtonPadding { get; } = new Thickness(11, 5);

        internal static Thickness MenuItemPadding { get; } = new Thickness(12, 11);

        internal static Thickness ListItemPadding { get; } = new Thickness(14, 11);
    }
}
