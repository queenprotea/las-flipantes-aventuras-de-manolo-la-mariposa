using FontStashSharp.RichText;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class DisconnectionScreen : Screen
    {
        private const int CenterSpacing = 16;
        private const int TagSpacing = 6;
        private const float MutedIconOpacity = 0.5f;
        private const float WithdrawnOpacity = 0.45f;
        private const float FullOpacity = 1f;

        private static readonly DisconnectionPlayer[] _previewPlayers =
        {
            new DisconnectionPlayer(PreviewSamples.SeatOnePlayer, true, false, true, false, Theme.MintBrush),
            new DisconnectionPlayer(PreviewSamples.SeatTwoPlayer, false, false, false, false, Theme.LavenderBrush),
            new DisconnectionPlayer(PreviewSamples.SeatThreePlayer, false, true, true, true, Theme.ButterBrush),
            new DisconnectionPlayer(PreviewSamples.SeatFourPlayer, false, false, true, false, Theme.SkyBrush),
        };

        internal DisconnectionScreen()
            : base(TextKeys.Disconnection.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel centerPanel = BuildCenter();
            HorizontalStackPanel columns = CreateColumns();
            columns.Widgets.Add(centerPanel);
            columns.Widgets.Add(BuildSideColumn());
            StackPanel.SetProportionType(centerPanel, ProportionType.Fill);

            return columns;
        }

        private VerticalStackPanel BuildCenter()
        {
            var resumeTurnMessage = new LocalizedLabel(TextKeys.Disconnection.ResumeTurnMessage)
            {
                Width = Sizes.DisconnectionMessageWidth,
                Font = Fonts.Body,
                TextColor = Theme.SoftInk,
                Wrap = true,
                TextAlign = TextHorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var centerPanel = new VerticalStackPanel
            {
                Spacing = CenterSpacing,
                VerticalAlignment = VerticalAlignment.Center,
            };
            centerPanel.Widgets.Add(new Panel
            {
                Width = Sizes.DisconnectionIconSize,
                Height = Sizes.DisconnectionIconSize,
                Background = Theme.MintAltBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Sizes.Border,
                Opacity = MutedIconOpacity,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            centerPanel.Widgets.Add(new LocalizedLabel(TextKeys.Disconnection.ConnectionLostTitle)
            {
                Font = Fonts.Notice,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            centerPanel.Widgets.Add(new Label
            {
                Text = PreviewSamples.DisconnectionClock,
                Font = Fonts.LargeClock,
                TextColor = Theme.BlushInk,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            centerPanel.Widgets.Add(resumeTurnMessage);
            centerPanel.Widgets.Add(BuildActions());

            return centerPanel;
        }

        private HorizontalStackPanel BuildActions()
        {
            LocalizedButton retryButton = CreatePrimaryButton(TextKeys.Disconnection.RetryButton);
            LocalizedButton exitButton = CreateSecondaryButton(TextKeys.Disconnection.ExitMatchButton);
            retryButton.Click += RetryButtonOnClick;
            exitButton.Click += ExitButtonOnClick;

            HorizontalStackPanel actionButtons = CreateButtonRow();
            actionButtons.HorizontalAlignment = HorizontalAlignment.Center;
            actionButtons.Widgets.Add(retryButton);
            actionButtons.Widgets.Add(exitButton);

            return actionButtons;
        }

        private static VerticalStackPanel BuildSideColumn()
        {
            var playersPanel = new VerticalStackPanel();
            foreach (DisconnectionPlayer player in _previewPlayers)
            {
                playersPanel.Widgets.Add(BuildPlayerRow(player));
            }

            var sideColumn = new VerticalStackPanel
            {
                Width = Sizes.DisconnectionSideWidth,
                VerticalAlignment = VerticalAlignment.Top,
            };
            sideColumn.Widgets.Add(CreateGameSection(TextKeys.Disconnection.PlayersLabel, playersPanel));

            return sideColumn;
        }

        private static HorizontalStackPanel BuildPlayerRow(DisconnectionPlayer player)
        {
            var playerIdentity = new VerticalStackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
            };
            playerIdentity.Widgets.Add(CreatePlayerNameLabel(player.Name, Theme.Ink));

            HorizontalStackPanel playerTags = BuildTags(player);
            if (playerTags.Widgets.Count > 0)
            {
                playerIdentity.Widgets.Add(playerTags);
            }

            var playerRow = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerRowPadding,
                Opacity = player.HasWithdrawn ? WithdrawnOpacity : FullOpacity,
            };
            playerRow.Widgets.Add(new Panel
            {
                Width = MatchLayout.PlayerIconSize,
                Height = MatchLayout.PlayerIconSize,
                Background = player.ColorBrush,
                VerticalAlignment = VerticalAlignment.Center,
            });
            playerRow.Widgets.Add(playerIdentity);
            StackPanel.SetProportionType(playerIdentity, ProportionType.Fill);

            return playerRow;
        }

        private static HorizontalStackPanel BuildTags(DisconnectionPlayer player)
        {
            var playerTags = new HorizontalStackPanel
            {
                Spacing = TagSpacing,
            };

            if (player.IsYou)
            {
                playerTags.Widgets.Add(BuildTag(TextKeys.Common.YouTag, Theme.MutedInk));
            }

            if (player.IsGuest)
            {
                playerTags.Widgets.Add(BuildTag(TextKeys.Common.GuestTag, Theme.LavenderInk));
            }

            if (!player.IsConnected)
            {
                playerTags.Widgets.Add(BuildTag(TextKeys.Common.DisconnectedTag, Theme.ButterInk));
            }

            if (player.HasWithdrawn)
            {
                playerTags.Widgets.Add(BuildTag(TextKeys.Common.WithdrewTag, Theme.BlushInk));
            }

            return playerTags;
        }

        private static LocalizedLabel BuildTag(string textKey, Color textColor)
        {
            var playerTag = new LocalizedLabel(textKey)
            {
                Font = Fonts.Label,
                TextColor = textColor,
            };

            return playerTag;
        }

        private void RetryButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Resume;
        }

        private void ExitButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MatchInProgress;
        }

        private sealed record DisconnectionPlayer(string Name, bool IsYou, bool IsGuest, bool IsConnected, bool HasWithdrawn, IBrush ColorBrush);
    }
}
