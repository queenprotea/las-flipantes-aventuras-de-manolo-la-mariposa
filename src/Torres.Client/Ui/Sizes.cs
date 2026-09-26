using Myra.Graphics2D;

namespace Torres.Client.Ui
{
    internal static class Sizes
    {
        internal const int WindowWidth = 1024;
        internal const int WindowHeight = 660;

        internal const int BorderSize = 1;

        internal const int LoginCardWidth = 348;
        internal const int RegisterCardWidth = 360;
        internal const int GuestCardWidth = 340;
        internal const int LanguageCardWidth = 340;
        internal const int RecoveryCardWidth = 290;
        internal const int ProfileAvatarCardWidth = 230;
        internal const int ProfileAccountCardWidth = 360;
        internal const int AccountSettingsCardWidth = 400;

        internal const int MenuListWidth = 268;
        internal const int LargeAvatarSize = 92;

        internal const int RankingTableWidth = 1000;
        internal const int RankingRankColumnWidth = 40;
        internal const int RankingStatColumnWidth = 96;

        internal const int EmptyStateWidth = 360;
        internal const int EmptyStateMarkSize = 64;

        internal static Thickness Border { get; } = new Thickness(BorderSize);
    }
}
