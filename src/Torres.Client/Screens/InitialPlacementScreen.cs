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
        private const string PlacedMark = "✓";
        private const string PreviewClock = "1:30";
        private const double PreviewSecondsBeforeMatch = 3;

        private static readonly PlacementPlayer[] _previewPlayers =
        {
            new PlacementPlayer("jesus", true, true, false, Theme.MintBrush),
            new PlacementPlayer("valentin", false, false, true, Theme.LavenderBrush),
            new PlacementPlayer("salma", false, false, false, Theme.ButterBrush),
            new PlacementPlayer("scarleth", false, false, false, Theme.SkyBrush),
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
            VerticalStackPanel center = BuildCenter();
            HorizontalStackPanel columns = Columns();
            columns.Widgets.Add(BuildSideBar());
            columns.Widgets.Add(center);
            columns.Widgets.Add(BuildSideColumn());
            StackPanel.SetProportionType(center, ProportionType.Fill);

            return columns;
        }

        private static VerticalStackPanel BuildSideBar()
        {
            var players = new VerticalStackPanel();
            var turnOrder = new VerticalStackPanel();
            for (var i = 0; i < _previewPlayers.Length; i++)
            {
                players.Widgets.Add(BuildPlayerRow(_previewPlayers[i]));
                turnOrder.Widgets.Add(BuildTurnOrderRow(_previewPlayers[i], i + 1));
            }

            var sideBar = new VerticalStackPanel
            {
                Spacing = Metrics.ColumnSpacing,
                Width = MatchLayout.SidebarWidth,
                VerticalAlignment = VerticalAlignment.Top,
            };
            sideBar.Widgets.Add(GameSection(TextKeys.InitialPlacement.PlayersLabel, players));
            sideBar.Widgets.Add(Divider());
            sideBar.Widgets.Add(GameSection(TextKeys.InitialPlacement.TurnOrderLabel, turnOrder));

            return sideBar;
        }

        private static HorizontalStackPanel BuildPlayerRow(PlacementPlayer player)
        {
            var identity = new VerticalStackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
            };
            identity.Widgets.Add(PlayerNameLabel(player.Name, player.IsPlacing ? Theme.MintInk : Theme.Ink));

            if (player.IsYou)
            {
                identity.Widgets.Add(new LocalizedLabel(TextKeys.Common.YouTag)
                {
                    Font = Fonts.Label,
                    TextColor = Theme.MutedInk,
                });
            }

            HorizontalStackPanel row = BuildColorRow(player.ColorBrush, MatchLayout.PlayerIconSize);
            row.Widgets.Add(identity);
            StackPanel.SetProportionType(identity, ProportionType.Fill);
            if (player.IsPlacing)
            {
                row.Background = Theme.MintTintBrush;
            }

            return row;
        }

        private static HorizontalStackPanel BuildTurnOrderRow(PlacementPlayer player, int seatNumber)
        {
            Label name = PlayerNameLabel(player.Name, Theme.Ink);
            HorizontalStackPanel row = BuildColorRow(player.ColorBrush, MatchLayout.TurnOrderIconSize);
            row.Widgets.Add(name);
            row.Widgets.Add(new Label
            {
                Text = player.HasPlaced ? PlacedMark : seatNumber.ToString(CultureInfo.CurrentCulture),
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
                VerticalAlignment = VerticalAlignment.Center,
            });
            StackPanel.SetProportionType(name, ProportionType.Fill);

            return row;
        }

        private static HorizontalStackPanel BuildColorRow(IBrush colorBrush, int iconSize)
        {
            var row = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerRowPadding,
            };
            row.Widgets.Add(new Panel
            {
                Width = iconSize,
                Height = iconSize,
                Background = colorBrush,
                VerticalAlignment = VerticalAlignment.Center,
            });

            return row;
        }

        private static VerticalStackPanel BuildCenter()
        {
            var board = new Panel
            {
                Background = Theme.SurfaceAltBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var center = new VerticalStackPanel
            {
                Spacing = MatchLayout.CenterSpacing,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            center.Widgets.Add(new Label
            {
                Text = PreviewClock,
                Font = Fonts.Clock,
                TextColor = Theme.MintInk,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            center.Widgets.Add(new LocalizedLabel(TextKeys.InitialPlacement.PlaceCaterpillarInstruction)
            {
                Font = Fonts.Title,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            center.Widgets.Add(new LocalizedLabel(TextKeys.InitialPlacement.CurrentTurnInstruction)
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextArguments = new object[] { FindPlacingPlayerName() },
            });
            center.Widgets.Add(board);
            StackPanel.SetProportionType(board, ProportionType.Fill);

            return center;
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
            VerticalStackPanel chat = ChatPanel();
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
