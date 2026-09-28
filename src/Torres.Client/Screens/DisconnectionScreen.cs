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
        private const string PreviewClock = "0:41";

        private static readonly DisconnectionPlayer[] _previewPlayers =
        {
            new DisconnectionPlayer("jesus", true, false, true, false, Theme.MintBrush),
            new DisconnectionPlayer("valentin", false, false, false, false, Theme.LavenderBrush),
            new DisconnectionPlayer("salma", false, true, true, true, Theme.ButterBrush),
            new DisconnectionPlayer("scarleth", false, false, true, false, Theme.SkyBrush),
        };

        internal DisconnectionScreen()
            : base(TextKeys.Disconnection.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel center = BuildCenter();
            HorizontalStackPanel columns = Columns();
            columns.Widgets.Add(center);
            columns.Widgets.Add(BuildSideColumn());
            StackPanel.SetProportionType(center, ProportionType.Fill);

            return columns;
        }

        private VerticalStackPanel BuildCenter()
        {
            var message = new LocalizedLabel(TextKeys.Disconnection.ResumeTurnMessage)
            {
                Width = Sizes.DisconnectionMessageWidth,
                Font = Fonts.Body,
                TextColor = Theme.SoftInk,
                Wrap = true,
                TextAlign = TextHorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var center = new VerticalStackPanel
            {
                Spacing = CenterSpacing,
                VerticalAlignment = VerticalAlignment.Center,
            };
            center.Widgets.Add(new Panel
            {
                Width = Sizes.DisconnectionIconSize,
                Height = Sizes.DisconnectionIconSize,
                Background = Theme.MintAltBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Sizes.Border,
                Opacity = MutedIconOpacity,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            center.Widgets.Add(new LocalizedLabel(TextKeys.Disconnection.ConnectionLostTitle)
            {
                Font = Fonts.Notice,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            center.Widgets.Add(new Label
            {
                Text = PreviewClock,
                Font = Fonts.LargeClock,
                TextColor = Theme.BlushInk,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            center.Widgets.Add(message);
            center.Widgets.Add(BuildActions());

            return center;
        }

        private HorizontalStackPanel BuildActions()
        {
            LocalizedButton retryButton = PrimaryButton(TextKeys.Disconnection.RetryButton);
            LocalizedButton exitButton = SecondaryButton(TextKeys.Disconnection.ExitMatchButton);
            retryButton.Click += RetryButtonOnClick;
            exitButton.Click += ExitButtonOnClick;

            HorizontalStackPanel actions = Row();
            actions.HorizontalAlignment = HorizontalAlignment.Center;
            actions.Widgets.Add(retryButton);
            actions.Widgets.Add(exitButton);

            return actions;
        }

        private static VerticalStackPanel BuildSideColumn()
        {
            var players = new VerticalStackPanel();
            foreach (DisconnectionPlayer player in _previewPlayers)
            {
                players.Widgets.Add(BuildPlayerRow(player));
            }

            var sideColumn = new VerticalStackPanel
            {
                Width = Sizes.DisconnectionSideWidth,
                VerticalAlignment = VerticalAlignment.Top,
            };
            sideColumn.Widgets.Add(GameSection(TextKeys.Disconnection.PlayersLabel, players));

            return sideColumn;
        }

        private static HorizontalStackPanel BuildPlayerRow(DisconnectionPlayer player)
        {
            var identity = new VerticalStackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
            };
            identity.Widgets.Add(PlayerNameLabel(player.Name, Theme.Ink));

            HorizontalStackPanel tags = BuildTags(player);
            if (tags.Widgets.Count > 0)
            {
                identity.Widgets.Add(tags);
            }

            var row = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerRowPadding,
                Opacity = player.HasWithdrawn ? WithdrawnOpacity : FullOpacity,
            };
            row.Widgets.Add(new Panel
            {
                Width = MatchLayout.PlayerIconSize,
                Height = MatchLayout.PlayerIconSize,
                Background = player.ColorBrush,
                VerticalAlignment = VerticalAlignment.Center,
            });
            row.Widgets.Add(identity);
            StackPanel.SetProportionType(identity, ProportionType.Fill);

            return row;
        }

        private static HorizontalStackPanel BuildTags(DisconnectionPlayer player)
        {
            var tags = new HorizontalStackPanel
            {
                Spacing = TagSpacing,
            };

            if (player.IsYou)
            {
                tags.Widgets.Add(BuildTag(TextKeys.Common.YouTag, Theme.MutedInk));
            }

            if (player.IsGuest)
            {
                tags.Widgets.Add(BuildTag(TextKeys.Common.GuestTag, Theme.LavenderInk));
            }

            if (!player.IsConnected)
            {
                tags.Widgets.Add(BuildTag(TextKeys.Common.DisconnectedTag, Theme.ButterInk));
            }

            if (player.HasWithdrawn)
            {
                tags.Widgets.Add(BuildTag(TextKeys.Common.WithdrewTag, Theme.BlushInk));
            }

            return tags;
        }

        private static LocalizedLabel BuildTag(string textKey, Color textColor)
        {
            return new LocalizedLabel(textKey)
            {
                Font = Fonts.Label,
                TextColor = textColor,
            };
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
