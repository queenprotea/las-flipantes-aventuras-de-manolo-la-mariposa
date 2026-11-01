using System.Globalization;

using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class InitialPlacementScreen : Screen
    {
        private const double PreviewSecondsBeforeMatch = 3;

        private static readonly PlacementPlayer[] _previewPlayers =
        {
            new PlacementPlayer(PreviewSamples.SeatOnePlayer, true, true, false, Theme.MintBrush),
            new PlacementPlayer(PreviewSamples.SeatTwoPlayer, false, false, true, Theme.LavenderBrush),
            new PlacementPlayer(PreviewSamples.SeatThreePlayer, false, false, false, Theme.ButterBrush),
            new PlacementPlayer(PreviewSamples.SeatFourPlayer, false, false, false, Theme.SkyBrush),
        };

        private double _secondsShown;

        internal InitialPlacementScreen()
            : base(TextKeys.InitialPlacement.HeaderLabel, true)
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
            VerticalStackPanel centerPanel = BuildCenter();
            HorizontalStackPanel columns = CreateColumns();
            columns.Widgets.Add(BuildSideBar());
            columns.Widgets.Add(centerPanel);
            columns.Widgets.Add(BuildSideColumn());
            StackPanel.SetProportionType(centerPanel, ProportionType.Fill);

            return columns;
        }

        private static VerticalStackPanel BuildSideBar()
        {
            var playersPanel = new VerticalStackPanel();
            var turnOrder = new VerticalStackPanel();
            for (var i = 0; i < _previewPlayers.Length; i++)
            {
                playersPanel.Widgets.Add(BuildPlayerRow(_previewPlayers[i]));
                turnOrder.Widgets.Add(BuildTurnOrderRow(_previewPlayers[i], i + 1));
            }

            var sideBar = new VerticalStackPanel
            {
                Spacing = Metrics.ColumnSpacing,
                Width = MatchLayout.SidebarWidth,
                VerticalAlignment = VerticalAlignment.Top,
            };
            sideBar.Widgets.Add(CreateGameSection(TextKeys.InitialPlacement.PlayersLabel, playersPanel));
            sideBar.Widgets.Add(CreateDivider());
            sideBar.Widgets.Add(CreateGameSection(TextKeys.InitialPlacement.TurnOrderLabel, turnOrder));

            return sideBar;
        }

        private static HorizontalStackPanel BuildPlayerRow(PlacementPlayer player)
        {
            var playerIdentity = new VerticalStackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
            };
            Color nameColor = player.IsPlacing ? Theme.MintInk : Theme.Ink;
            playerIdentity.Widgets.Add(CreatePlayerNameLabel(player.Name, nameColor));

            if (player.IsYou)
            {
                playerIdentity.Widgets.Add(new LocalizedLabel(TextKeys.Common.YouTag)
                {
                    Font = Fonts.Label,
                    TextColor = Theme.MutedInk,
                });
            }

            HorizontalStackPanel playerRow = BuildColorRow(player.ColorBrush, MatchLayout.PlayerIconSize);
            playerRow.Widgets.Add(playerIdentity);
            StackPanel.SetProportionType(playerIdentity, ProportionType.Fill);
            if (player.IsPlacing)
            {
                playerRow.Background = Theme.MintTintBrush;
            }

            return playerRow;
        }

        private static HorizontalStackPanel BuildTurnOrderRow(PlacementPlayer player, int seatNumber)
        {
            Label nameLabel = CreatePlayerNameLabel(player.Name, Theme.Ink);
            HorizontalStackPanel turnOrderRow = BuildColorRow(player.ColorBrush, MatchLayout.TurnOrderIconSize);
            turnOrderRow.Widgets.Add(nameLabel);
            turnOrderRow.Widgets.Add(new Label
            {
                Text = player.HasPlaced ? DisplaySymbols.CheckMark : seatNumber.ToString(CultureInfo.CurrentCulture),
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
                VerticalAlignment = VerticalAlignment.Center,
            });
            StackPanel.SetProportionType(nameLabel, ProportionType.Fill);

            return turnOrderRow;
        }

        private static HorizontalStackPanel BuildColorRow(IBrush colorBrush, int iconSize)
        {
            var colorRow = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerRowPadding,
            };
            colorRow.Widgets.Add(new Panel
            {
                Width = iconSize,
                Height = iconSize,
                Background = colorBrush,
                VerticalAlignment = VerticalAlignment.Center,
            });

            return colorRow;
        }

        private static VerticalStackPanel BuildCenter()
        {
            var boardPlaceholder = new Panel
            {
                Background = Theme.SurfaceAltBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var centerPanel = new VerticalStackPanel
            {
                Spacing = MatchLayout.CenterSpacing,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            centerPanel.Widgets.Add(new Label
            {
                Text = PreviewSamples.PlacementClock,
                Font = Fonts.Clock,
                TextColor = Theme.MintInk,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            centerPanel.Widgets.Add(new LocalizedLabel(TextKeys.InitialPlacement.PlaceCaterpillarInstruction)
            {
                Font = Fonts.Title,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            centerPanel.Widgets.Add(new LocalizedLabel(TextKeys.InitialPlacement.CurrentTurnInstruction)
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextArguments = new[] { FindPlacingPlayerName() },
            });
            centerPanel.Widgets.Add(boardPlaceholder);
            StackPanel.SetProportionType(boardPlaceholder, ProportionType.Fill);

            return centerPanel;
        }

        private static string FindPlacingPlayerName()
        {
            string placingName = string.Empty;
            foreach (PlacementPlayer player in _previewPlayers)
            {
                if (player.IsPlacing)
                {
                    placingName = player.Name;
                }
            }

            return placingName;
        }

        private static VerticalStackPanel BuildSideColumn()
        {
            VerticalStackPanel chat = CreateChatPanel();
            var sideColumn = new VerticalStackPanel
            {
                Width = MatchLayout.SideColumnWidth,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            sideColumn.Widgets.Add(chat);
            StackPanel.SetProportionType(chat, ProportionType.Fill);

            return sideColumn;
        }

        private sealed record PlacementPlayer(string Name, bool IsYou, bool HasPlaced, bool IsPlacing, IBrush ColorBrush);
    }
}
