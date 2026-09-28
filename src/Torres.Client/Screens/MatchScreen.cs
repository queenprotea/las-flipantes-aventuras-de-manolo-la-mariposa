using System.Globalization;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MatchScreen : Screen
    {
        private const int CaterpillarsPerPlayer = 5;
        private const int NewCaterpillarCost = 2;
        private const int MoveCost = 1;
        private const int GrowCost = 1;
        private const int BuildCost = 1;
        private const int DrawCost = 1;

        private const float SpentPipOpacity = 0.28f;
        private const string Separator = "·";

        private const string PreviewRoomCode = "K7QM";
        private const string PreviewClock = "1:12";
        private const int PreviewRound = 2;
        private const int LastRound = 3;
        private const int PreviewTurn = 2;
        private const int PreviewActionPoints = 5;
        private const int PreviewCaterpillarsLeft = 3;
        private const int PreviewBuildings = 3;
        private const int PreviewYourSeat = 0;

        private static readonly int[] _previewCardNumbers = { 1, 2, 4, 5, 6, 7, 8 };
        private static readonly string[] _previewPlayerNames = { "jesus", "valentin", "salma", "scarleth" };
        private static readonly int[] _previewPlayerPoints = { 34, 41, 28, 19 };

        private readonly LocalizedLabel _roundTurnLabel = new LocalizedLabel(TextKeys.Match.RoundTurnInstruction)
        {
            Font = Fonts.Small,
            TextColor = Theme.MutedInk,
        };

        private int _previewRound = PreviewRound;

        internal MatchScreen() : base(TextKeys.Match.HeaderLabel, true)
        {
            HeaderArguments = new object[] { PreviewRoomCode };
        }

        internal override void Open()
        {
            _roundTurnLabel.TextArguments = new object[] { _previewRound, PreviewTurn };
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = Columns();

            VerticalStackPanel center = BuildCenter();
            columns.Widgets.Add(BuildSideBar());
            columns.Widgets.Add(center);
            columns.Widgets.Add(BuildSideColumn());

            StackPanel.SetProportionType(center, ProportionType.Fill);

            VerticalStackPanel page = Page();
            page.Spacing = MatchLayout.HudSpacing;
            page.Widgets.Add(columns);
            page.Widgets.Add(Divider());
            page.Widgets.Add(BuildHud());
            StackPanel.SetProportionType(columns, ProportionType.Fill);

            return page;
        }

        private static Panel CreatePlaceholder()
        {
            var panel = new Panel()
            {
                Background = Theme.SurfaceAltBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Sizes.Border,
            };

            return panel;
        }

        private static VerticalStackPanel BuildSideBar()
        {
            var players = new VerticalStackPanel();
            for (var i = 0; i < _previewPlayerNames.Length; i++)
            {
                players.Widgets.Add(BuildPlayerRow(_previewPlayerNames[i], _previewPlayerPoints[i], i == PreviewYourSeat));
            }

            var sideBar = new VerticalStackPanel()
            {
                Spacing = Metrics.ColumnSpacing,
                Width = MatchLayout.SidebarWidth,
                VerticalAlignment = VerticalAlignment.Top,
            };

            sideBar.Widgets.Add(GameSection(TextKeys.Match.PlayersLabel, players));
            sideBar.Widgets.Add(Divider());
            sideBar.Widgets.Add(GameSection(TextKeys.Match.YourCaterpillarsLabel, BuildPipRow(CaterpillarsPerPlayer, PreviewCaterpillarsLeft, MatchLayout.CaterpillarPipSize)));
            sideBar.Widgets.Add(GameSection(TextKeys.Match.BuildingsLabel, BuildPipRow(PreviewBuildings, PreviewBuildings, MatchLayout.BuildingPipSize)));

            return sideBar;
        }

        private static HorizontalStackPanel BuildPlayerRow(string name, int points, bool isYou)
        {
            Panel icon = CreatePlaceholder();
            icon.Width = MatchLayout.PlayerIconSize;
            icon.Height = MatchLayout.PlayerIconSize;
            icon.VerticalAlignment = VerticalAlignment.Center;

            var identity = new VerticalStackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
            };

            identity.Widgets.Add(PlayerNameLabel(name, isYou ? Theme.MintInk : Theme.Ink));

            var row = new HorizontalStackPanel()
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerRowPadding,
            };

            row.Widgets.Add(icon);
            row.Widgets.Add(identity);
            row.Widgets.Add(new Label()
            {
                Text = points.ToString(CultureInfo.CurrentCulture),
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
                VerticalAlignment = VerticalAlignment.Center,
            });

            StackPanel.SetProportionType(identity, ProportionType.Fill);

            if (isYou)
            {
                row.Background = Theme.MintTintBrush;
                identity.Widgets.Add((new LocalizedLabel(TextKeys.Common.YouTag)
                {
                    Font = Fonts.Label,
                    TextColor = Theme.MutedInk,
                }));
            }

            return row;
        }

        private static HorizontalStackPanel BuildPipRow(int total, int available, int size)
        {
            var row = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PipSpacing,
            };

            for (var i = 0; i < total; i++)
            {
                Panel pip = CreatePlaceholder();
                pip.Width = size;
                pip.Height = size;
                pip.Opacity = i < available ? 1f : SpentPipOpacity;

                row.Widgets.Add(pip);
            }

            return row;
        }

        private VerticalStackPanel BuildCenter()
        {
            var center = new VerticalStackPanel()
            {
                Spacing = MatchLayout.CenterSpacing,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var clock = new Label()
            {
                Text = PreviewClock,
                Font = Fonts.Clock,
                TextColor = Theme.MintInk,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            Panel board = CreatePlaceholder();
            board.HorizontalAlignment = HorizontalAlignment.Stretch;
            board.VerticalAlignment = VerticalAlignment.Stretch;

            center.Widgets.Add(clock);
            center.Widgets.Add(BuildTurnStatus());
            center.Widgets.Add(board);
            center.Widgets.Add(BuildAction());

            StackPanel.SetProportionType(board, ProportionType.Fill);

            return center;
        }

        private HorizontalStackPanel BuildTurnStatus()
        {
            var status = new HorizontalStackPanel
            {
                Spacing = Metrics.FieldLabelSpacing,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            status.Widgets.Add(_roundTurnLabel);

            status.Widgets.Add(new LocalizedLabel(TextKeys.Match.YourTurnStatus)
            {
                Font = Fonts.Small,
                TextColor = Theme.Ink,
            });

            return status;
        }

        private static HorizontalStackPanel BuildAction()
        {
            var actions = new HorizontalStackPanel
            {
                Spacing = MatchLayout.ActionSpacing,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            actions.Widgets.Add(BuildActionButton(TextKeys.Match.CaterpillarActionButton,
                NewCaterpillarCost));
            actions.Widgets.Add(BuildActionButton(TextKeys.Match.MoveActionButton,
                MoveCost));
            actions.Widgets.Add(BuildActionButton(TextKeys.Match.GrowActionButton,
                GrowCost));
            actions.Widgets.Add(BuildActionButton(TextKeys.Match.BuildActionButton,
                BuildCost));

            return actions;
        }

        private static Button BuildActionButton(string textKey, int cost)
        {
            var content = new HorizontalStackPanel
            {
                Spacing = Metrics.FieldLabelSpacing
            };

            content.Widgets.Add(new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
            });

            content.Widgets.Add(new Label()
            {
                Text = $"{Separator} {cost}",
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            });

            Button contentButton = BuildContentButton(content);

            return contentButton;
        }

        private static Button BuildContentButton(Widget content)
        {
            var button = new Button
            {
                Content = content,
                Padding = Metrics.SmallButtonPadding,
                Background = Theme.SurfaceBrush,
                OverBackground = Theme.SurfaceSunkenBrush,
                PressedBackground = Theme.SurfaceSunkenBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Sizes.Border,
            };

            return button;
        }

        private static HorizontalStackPanel BuildDeck()
        {
            var cardBack = new Panel
            {
                Width = MatchLayout.CardWidth,
                Height = MatchLayout.CardHeight,
                Background = Theme.LavenderBrush,
                Border = Theme.LavenderLineBrush,
                BorderThickness = Sizes.Border,
            };

            var drawLabel = new LocalizedLabel(TextKeys.Match.DrawButton)
            {
                Font = Fonts.Small,
                TextColor = Theme.Ink,
                TextArguments = new object[] {DrawCost},
            };

            Button draw = BuildContentButton(drawLabel);
            draw.VerticalAlignment = VerticalAlignment.Center;

            var deck = new HorizontalStackPanel
            {
                Spacing = MatchLayout.DeckSpacing,
            };

            deck.Widgets.Add(cardBack);
            deck.Widgets.Add(draw);

            return deck;
        }

        private VerticalStackPanel BuildSideColumn()
        {
            var sideColumn = new VerticalStackPanel
            {
                Spacing = Metrics.ColumnSpacing,
                Width = MatchLayout.SideColumnWidth,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            LocalizedButton endTurn = PrimaryButton(TextKeys.Match.EndTurnButton);
            endTurn.Padding = MatchLayout.EndTurnPadding;
            endTurn.HorizontalAlignment = HorizontalAlignment.Stretch;
            endTurn.LabelAlignment = HorizontalAlignment.Center;
            endTurn.Click += EndTurnButtonOnClick;

            LocalizedButton forfeit = DestructiveButton(TextKeys.Match.ForfeitButton);
            forfeit.LabelFont = Fonts.Small;
            forfeit.Padding = Metrics.SmallButtonPadding;
            forfeit.HorizontalAlignment = HorizontalAlignment.Stretch;
            forfeit.Click += ForfeitButtonOnClick;

            VerticalStackPanel chat = ChatPanel();

            sideColumn.Widgets.Add(chat);
            sideColumn.Widgets.Add(GameSection(TextKeys.Match.DeckLabel, BuildDeck()));
            sideColumn.Widgets.Add(endTurn);
            sideColumn.Widgets.Add(GameSection(TextKeys.Match.MatchLabel, forfeit));

            StackPanel.SetProportionType(chat, ProportionType.Fill);

            return sideColumn;
        }

        private static Panel BuildHud()
        {
            var hud = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            hud.Widgets.Add(BuildActionPoints());
            hud.Widgets.Add(BuildHand());

            return hud;
        }

        private static HorizontalStackPanel BuildActionPoints()
        {
            var actionPoints = new HorizontalStackPanel
            {
                Spacing = MatchLayout.ActionPointsSpacing,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };

            actionPoints.Widgets.Add(new Label
            {
                Text = PreviewActionPoints.ToString(CultureInfo.CurrentCulture),
                Font = Fonts.ActionPoints,
                TextColor = Theme.Ink,
            });

            actionPoints.Widgets.Add(new LocalizedLabel(TextKeys.Match.ActionPointsLabel)
            {
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Bottom,
            });

            return actionPoints;
        }

        private static HorizontalStackPanel BuildHand()
        {
            var hand = new HorizontalStackPanel
            {
                Spacing = MatchLayout.HandSpacing,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            foreach (int number in _previewCardNumbers)
            {
                hand.Widgets.Add(BuildCard(number));
            }

            return hand;
        }

        private static Panel BuildCard(int number)
        {
            var card = new Panel
            {
                Width = MatchLayout.CardWidth,
                Height = MatchLayout.CardHeight,
                Background = Theme.SurfaceSunkenBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Sizes.Border,
            };

            card.Widgets.Add(new Label
            {
                Text = number.ToString(CultureInfo.CurrentCulture),
                Font = Fonts.Body,
                TextColor = Theme.SoftInk,
                Margin = MatchLayout.CardNumberMargin,
            });

            return card;
        }

        private void EndTurnButtonOnClick(object sender, MyraEventArgs e)
        {
            if (_previewRound < LastRound)
            {
                _previewRound++;
                RequestedScreen = ScreenId.RoundSummary;
                return;
            }

            _previewRound = PreviewRound;
            RequestedScreen = ScreenId.Result;
        }

        private void ForfeitButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Disconnection;
        }
    }
}

