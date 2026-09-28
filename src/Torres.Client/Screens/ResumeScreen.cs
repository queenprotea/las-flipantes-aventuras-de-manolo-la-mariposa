using FontStashSharp.RichText;

using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class ResumeScreen : Screen
    {
        private const int TitleTopSpacing = 4;
        private const int StatusBottomSpacing = 14;
        private const int PlayersTopSpacing = 22;
        private const int PlayersTopPadding = 16;
        private const int HintTopSpacing = 16;
        private const string ReturnedMark = "✓";
        private const string WaitingMark = "◷";
        private const string PreviewClock = "2:14";
        private const double PreviewSecondsBeforeMatch = 5;

        private static readonly ResumePlayer[] _previewPlayers =
        {
            new ResumePlayer("jesus", true, Theme.MintBrush),
            new ResumePlayer("valentin", true, Theme.LavenderBrush),
            new ResumePlayer("salma", false, Theme.ButterBrush),
            new ResumePlayer("scarleth", false, Theme.SkyBrush),
        };

        private double _secondsShown;

        internal ResumeScreen()
            : base(TextKeys.Resume.HeaderLabel, true)
        {
        }

        internal override void Open()
        {
            _secondsShown = 0;
        }

        internal override void Update(GameTime gameTime)
        {
            _secondsShown += gameTime.ElapsedGameTime.TotalSeconds;
            if (_secondsShown < PreviewSecondsBeforeMatch)
            {
                return;
            }

            RequestedScreen = ScreenId.Match;
        }

        protected override Widget Build()
        {
            LocalizedLabel noActionNeeded = Hint(TextKeys.Resume.NoActionNeededMessage);
            noActionNeeded.TextAlign = TextHorizontalAlignment.Center;
            noActionNeeded.HorizontalAlignment = HorizontalAlignment.Center;
            noActionNeeded.Margin = new Thickness(0, HintTopSpacing, 0, 0);

            var content = new VerticalStackPanel
            {
                Width = Sizes.ResumeWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            content.Widgets.Add(new Panel
            {
                Width = Sizes.ResumeIconSize,
                Height = Sizes.ResumeIconSize,
                Background = Theme.MintAltBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            content.Widgets.Add(new LocalizedLabel(TextKeys.Resume.Title)
            {
                Font = Fonts.Notice,
                TextColor = Theme.Ink,
                Margin = new Thickness(0, TitleTopSpacing, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            content.Widgets.Add(new LocalizedLabel(TextKeys.Resume.WaitingStatus)
            {
                Font = Fonts.Body,
                TextColor = Theme.MutedInk,
                Margin = new Thickness(0, 0, 0, StatusBottomSpacing),
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            content.Widgets.Add(new Label
            {
                Text = PreviewClock,
                Font = Fonts.Clock,
                TextColor = Theme.MintInk,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            content.Widgets.Add(BuildPlayers());
            content.Widgets.Add(noActionNeeded);

            return content;
        }

        private static VerticalStackPanel BuildPlayers()
        {
            var rows = new VerticalStackPanel
            {
                Margin = new Thickness(0, PlayersTopPadding, 0, 0),
            };
            foreach (ResumePlayer player in _previewPlayers)
            {
                rows.Widgets.Add(BuildPlayerRow(player));
            }

            var players = new VerticalStackPanel
            {
                Margin = new Thickness(0, PlayersTopSpacing, 0, 0),
            };
            players.Widgets.Add(Divider());
            players.Widgets.Add(rows);

            return players;
        }

        private static HorizontalStackPanel BuildPlayerRow(ResumePlayer player)
        {
            Label name = PlayerNameLabel(player.Name, Theme.Ink);
            var row = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerRowPadding,
            };
            row.Widgets.Add(new Panel
            {
                Width = MatchLayout.PlayerIconSize,
                Height = MatchLayout.PlayerIconSize,
                Background = player.ColorBrush,
                VerticalAlignment = VerticalAlignment.Center,
            });
            row.Widgets.Add(name);
            row.Widgets.Add(new Label
            {
                Text = player.HasReturned ? ReturnedMark : WaitingMark,
                Font = Fonts.Small,
                TextColor = player.HasReturned ? Theme.MintInk : Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            });
            StackPanel.SetProportionType(name, ProportionType.Fill);

            return row;
        }

        private sealed record ResumePlayer(string Name, bool HasReturned, IBrush ColorBrush);
    }
}
