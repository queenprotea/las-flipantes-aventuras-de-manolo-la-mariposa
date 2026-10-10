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
        private const double PreviewSecondsBeforeMatch = 5;

        private static readonly ResumePlayer[] _previewPlayers =
        {
            new ResumePlayer(PreviewSamples.SeatOnePlayer, true, Theme.MintBrush),
            new ResumePlayer(PreviewSamples.SeatTwoPlayer, true, Theme.LavenderBrush),
            new ResumePlayer(PreviewSamples.SeatThreePlayer, false, Theme.ButterBrush),
            new ResumePlayer(PreviewSamples.SeatFourPlayer, false, Theme.SkyBrush),
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
            LocalizedLabel noActionNeeded = CreateHint(TextKeys.Resume.NoActionNeededMessage);
            noActionNeeded.TextAlign = TextHorizontalAlignment.Center;
            noActionNeeded.HorizontalAlignment = HorizontalAlignment.Center;
            noActionNeeded.Margin = new Thickness(0, HintTopSpacing, 0, 0);

            var screenLayout = new VerticalStackPanel
            {
                Width = Sizes.ResumeWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            screenLayout.Widgets.Add(new Panel
            {
                Width = Sizes.ResumeIconSize,
                Height = Sizes.ResumeIconSize,
                Background = Theme.MintAltBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            screenLayout.Widgets.Add(new LocalizedLabel(TextKeys.Resume.Title)
            {
                Font = Fonts.Notice,
                TextColor = Theme.Ink,
                Margin = new Thickness(0, TitleTopSpacing, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            screenLayout.Widgets.Add(new LocalizedLabel(TextKeys.Resume.WaitingStatus)
            {
                Font = Fonts.Body,
                TextColor = Theme.MutedInk,
                Margin = new Thickness(0, 0, 0, StatusBottomSpacing),
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            screenLayout.Widgets.Add(new Label
            {
                Text = PreviewSamples.ResumeClock,
                Font = Fonts.Clock,
                TextColor = Theme.MintInk,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            screenLayout.Widgets.Add(BuildPlayers());
            screenLayout.Widgets.Add(noActionNeeded);

            return screenLayout;
        }

        private static VerticalStackPanel BuildPlayers()
        {
            var playerRows = new VerticalStackPanel
            {
                Margin = new Thickness(0, PlayersTopPadding, 0, 0),
            };
            foreach (ResumePlayer player in _previewPlayers)
            {
                playerRows.Widgets.Add(BuildPlayerRow(player));
            }

            var playersPanel = new VerticalStackPanel
            {
                Margin = new Thickness(0, PlayersTopSpacing, 0, 0),
            };
            playersPanel.Widgets.Add(CreateDivider());
            playersPanel.Widgets.Add(playerRows);

            return playersPanel;
        }

        private static HorizontalStackPanel BuildPlayerRow(ResumePlayer player)
        {
            Label nameLabel = CreatePlayerNameLabel(player.Name, Theme.Ink);
            var playerRow = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerRowPadding,
            };
            playerRow.Widgets.Add(new Panel
            {
                Width = MatchLayout.PlayerIconSize,
                Height = MatchLayout.PlayerIconSize,
                Background = player.ColorBrush,
                VerticalAlignment = VerticalAlignment.Center,
            });
            playerRow.Widgets.Add(nameLabel);
            playerRow.Widgets.Add(new Label
            {
                Text = player.HasReturned ? DisplaySymbols.CheckMark : DisplaySymbols.WaitingClock,
                Font = Fonts.Small,
                TextColor = player.HasReturned ? Theme.MintInk : Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            });
            StackPanel.SetProportionType(nameLabel, ProportionType.Fill);

            return playerRow;
        }

        private sealed record ResumePlayer(string Name, bool HasReturned, IBrush ColorBrush);
    }
}
