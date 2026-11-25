using Myra.Graphics2D;

namespace Torres.Client.Ui
{
    internal static class MatchLayout
    {
        internal const float DisabledPillOpacity = 0.5f;
        internal const int SidebarWidth = 176;
        internal const int PlayerPillWidth = 150;
        internal const int TopBarSpacing = 8;
        internal const int ActionColumnWidth = 150;
        internal const int ActionIconCircleSize = 30;
        internal const int TableLabelSpacing = 2;
        internal const int ChatTabWidth = 34;
        internal const int ChatOverlayWidth = 222;
        internal const int RightColumnSpacing = 16;
        internal const int ChatTabHeight = 60;
        internal const int ScoreCircleSize = 26;
        internal const int ClockRingSize = 38;
        internal const int HeaderCardSpacing = 8;
        internal const int ActionIconWidth = 30;
        internal const int ActionIconHeight = 15;
        internal const int ActionPillSpacing = 7;
        internal const int NoticeWidth = 420;
        internal const int SideColumnWidth = 194;
        internal const int CenterSpacing = 12;
        internal const int SectionLabelSpacing = 8;
        internal const int PlayerRowSpacing = 8;
        internal const int PlayerIconSize = 20;
        internal const int PlayerIconWidth = 40;
        internal const int PlayerIconHeight = 20;
        internal const int TurnOrderIconSize = 18;
        internal const int HandSpacing = 6;

        internal static Thickness PlayerRowPadding { get; } = new Thickness(6, 4);

        internal static Thickness PlayerPillPadding { get; } = new Thickness(10, 6);

        internal static Thickness TableLabelPadding { get; } = new Thickness(12, 6);

        internal static Thickness EndTurnPillPadding { get; } = new Thickness(18, 13);

        internal static Thickness ChatOverlayMargin { get; } = new Thickness(0, 70, 176, 70);

        internal static Thickness TableEdgeMargin { get; } = new Thickness(12, 10);

        internal static Thickness HintMargin { get; } = new Thickness(0, 70, 0, 0);

        internal static Thickness NoticeMargin { get; } = new Thickness(0, 108, 0, 0);

        internal static Thickness RightColumnMargin { get; } = new Thickness(0, 70, 12, 12);

        internal static Thickness HeaderCardPadding { get; } = new Thickness(12, 5);

        internal static Thickness ActionPillPadding { get; } = new Thickness(14, 8);

        internal static Thickness HintPillPadding { get; } = new Thickness(18, 7);

    }
}
