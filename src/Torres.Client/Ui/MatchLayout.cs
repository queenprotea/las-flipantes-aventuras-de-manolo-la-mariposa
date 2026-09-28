using Myra.Graphics2D;

namespace Torres.Client.Ui
{
    internal static class MatchLayout
    {
        internal const int SidebarWidth = 152;
        internal const int SideColumnWidth = 194;
        internal const int CenterSpacing = 12;
        internal const int SectionLabelSpacing = 8;
        internal const int PlayerRowSpacing = 8;
        internal const int PlayerIconSize = 20;
        internal const int TurnOrderIconSize = 18;
        internal const int CaterpillarPipSize = 23;
        internal const int BuildingPipSize = 24;
        internal const int PipSpacing = 5;
        internal const int ActionSpacing = 7;
        internal const int DeckSpacing = 11;
        internal const int HandSpacing = 6;
        internal const int CardWidth = 48;
        internal const int CardHeight = 66;
        internal const int HudSpacing = 14;
        internal const int ActionPointsSpacing = 7;

        internal static Thickness PlayerRowPadding { get; } = new Thickness(6, 4);

        internal static Thickness CardNumberMargin { get; } = new Thickness(6, 3, 0, 0);

        internal static Thickness EndTurnPadding { get; } = new Thickness(16, 12);
    }
}
