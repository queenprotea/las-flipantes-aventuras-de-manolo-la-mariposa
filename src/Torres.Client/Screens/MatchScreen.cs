using System.Globalization;

using Microsoft.Xna.Framework;

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

        private const int PreviewRound = 2;
        private const int LastRound = 3;
        private const int PreviewTurn = 2;
        private const int PreviewActionPoints = 5;
        private const int PreviewCaterpillarsLeft = 3;
        private const int PreviewBuildings = 3;
        private const int PreviewYourSeat = 0;

        private static readonly int[] _previewCardNumbers = { 1, 2, 4, 5, 6, 7, 8 };
        private static readonly int[] _previewPlayerPoints = { 34, 41, 28, 19 };

        private readonly LocalizedLabel _roundTurnLabel = new LocalizedLabel(TextKeys.Match.RoundTurnInstruction)
        {
            Font = Fonts.Small,
            TextColor = Theme.MutedInk,
        };

        private int _previewRound = PreviewRound;

        internal MatchScreen() : base(TextKeys.Match.HeaderLabel, true)
        {
            HeaderArguments = new object[] { PreviewSamples.MatchRoomCode };
        }

        internal override void Open()
        {
            _roundTurnLabel.TextArguments = new object[] { _previewRound, PreviewTurn };
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = CreateColumns();

            VerticalStackPanel centerPanel = BuildCenter();
            columns.Widgets.Add(BuildSideBar());
            columns.Widgets.Add(centerPanel);
            columns.Widgets.Add(BuildSideColumn());

            StackPanel.SetProportionType(centerPanel, ProportionType.Fill);

            VerticalStackPanel page = CreatePage();
            page.Spacing = MatchLayout.HudSpacing;
            page.Widgets.Add(columns);
            page.Widgets.Add(CreateDivider());
            page.Widgets.Add(BuildHud());
            StackPanel.SetProportionType(columns, ProportionType.Fill);

            return page;
        }

        private static Panel CreatePlaceholder()
        {
            var placeholderPanel = new Panel()
            {
                Background = Theme.SurfaceAltBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Sizes.Border,
            };

            return placeholderPanel;
        }

        private static VerticalStackPanel BuildSideBar()
        {
            var playersPanel = new VerticalStackPanel();
            for (var i = 0; i < PreviewSamples.SeatPlayers.Count; i++)
            {
                playersPanel.Widgets.Add(BuildPlayerRow(PreviewSamples.SeatPlayers[i], _previewPlayerPoints[i], i == PreviewYourSeat));
            }

            var sideBar = new VerticalStackPanel()
            {
                Spacing = Metrics.ColumnSpacing,
                Width = MatchLayout.SidebarWidth,
                VerticalAlignment = VerticalAlignment.Top,
            };

            sideBar.Widgets.Add(CreateGameSection(TextKeys.Match.PlayersLabel, playersPanel));
            sideBar.Widgets.Add(CreateDivider());
            sideBar.Widgets.Add(CreateGameSection(TextKeys.Match.YourCaterpillarsLabel, BuildPipRow(CaterpillarsPerPlayer, PreviewCaterpillarsLeft, MatchLayout.CaterpillarPipSize)));
            sideBar.Widgets.Add(CreateGameSection(TextKeys.Match.BuildingsLabel, BuildPipRow(PreviewBuildings, PreviewBuildings, MatchLayout.BuildingPipSize)));

            return sideBar;
        }

        private static HorizontalStackPanel BuildPlayerRow(string playerName, int points, bool isYou)
        {
            Panel playerIcon = CreatePlaceholder();
            playerIcon.Width = MatchLayout.PlayerIconSize;
            playerIcon.Height = MatchLayout.PlayerIconSize;
            playerIcon.VerticalAlignment = VerticalAlignment.Center;

            var playerIdentity = new VerticalStackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
            };

            Color nameColor = isYou ? Theme.MintInk : Theme.Ink;

            playerIdentity.Widgets.Add(CreatePlayerNameLabel(playerName, nameColor));

            var playerRow = new HorizontalStackPanel()
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerRowPadding,
            };

            playerRow.Widgets.Add(playerIcon);
            playerRow.Widgets.Add(playerIdentity);
            playerRow.Widgets.Add(new Label()
            {
                Text = points.ToString(CultureInfo.CurrentCulture),
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
                VerticalAlignment = VerticalAlignment.Center,
            });

            StackPanel.SetProportionType(playerIdentity, ProportionType.Fill);

            if (isYou)
            {
                playerRow.Background = Theme.MintTintBrush;
                playerIdentity.Widgets.Add((new LocalizedLabel(TextKeys.Common.YouTag)
                {
                    Font = Fonts.Label,
                    TextColor = Theme.MutedInk,
                }));
            }

            return playerRow;
        }

        private static HorizontalStackPanel BuildPipRow(int total, int available, int size)
        {
            var pipRow = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PipSpacing,
            };

            for (var i = 0; i < total; i++)
            {
                Panel pip = CreatePlaceholder();
                pip.Width = size;
                pip.Height = size;
                pip.Opacity = i < available ? 1f : SpentPipOpacity;

                pipRow.Widgets.Add(pip);
            }

            return pipRow;
        }

        private VerticalStackPanel BuildCenter()
        {
            var centerPanel = new VerticalStackPanel()
            {
                Spacing = MatchLayout.CenterSpacing,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var turnClock = new Label()
            {
                Text = PreviewSamples.MatchClock,
                Font = Fonts.Clock,
                TextColor = Theme.MintInk,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            Panel boardPlaceholder = CreatePlaceholder();
            boardPlaceholder.HorizontalAlignment = HorizontalAlignment.Stretch;
            boardPlaceholder.VerticalAlignment = VerticalAlignment.Stretch;

            centerPanel.Widgets.Add(turnClock);
            centerPanel.Widgets.Add(BuildTurnStatus());
            centerPanel.Widgets.Add(boardPlaceholder);
            centerPanel.Widgets.Add(BuildAction());

            StackPanel.SetProportionType(boardPlaceholder, ProportionType.Fill);

            return centerPanel;
        }

        private HorizontalStackPanel BuildTurnStatus()
        {
            var turnStatus = new HorizontalStackPanel
            {
                Spacing = Metrics.FieldLabelSpacing,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            turnStatus.Widgets.Add(_roundTurnLabel);

            turnStatus.Widgets.Add(new LocalizedLabel(TextKeys.Match.YourTurnStatus)
            {
                Font = Fonts.Small,
                TextColor = Theme.Ink,
            });

            return turnStatus;
        }

        private static HorizontalStackPanel BuildAction()
        {
            var actionRow = new HorizontalStackPanel
            {
                Spacing = MatchLayout.ActionSpacing,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            actionRow.Widgets.Add(BuildActionButton(TextKeys.Match.CaterpillarActionButton,
                NewCaterpillarCost));
            actionRow.Widgets.Add(BuildActionButton(TextKeys.Match.MoveActionButton,
                MoveCost));
            actionRow.Widgets.Add(BuildActionButton(TextKeys.Match.GrowActionButton,
                GrowCost));
            actionRow.Widgets.Add(BuildActionButton(TextKeys.Match.BuildActionButton,
                BuildCost));

            return actionRow;
        }

        private static Button BuildActionButton(string textKey, int cost)
        {
            var buttonContent = new HorizontalStackPanel
            {
                Spacing = Metrics.FieldLabelSpacing
            };

            buttonContent.Widgets.Add(new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
            });

            buttonContent.Widgets.Add(new Label()
            {
                Text = $"{DisplaySymbols.InlineSeparator} {cost}",
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            });

            Button contentButton = BuildContentButton(buttonContent);

            return contentButton;
        }

        private static Button BuildContentButton(Widget buttonContent)
        {
            var contentButton = new Button
            {
                Content = buttonContent,
                Padding = Metrics.SmallButtonPadding,
                Background = Theme.SurfaceBrush,
                OverBackground = Theme.SurfaceSunkenBrush,
                PressedBackground = Theme.SurfaceSunkenBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Sizes.Border,
            };

            return contentButton;
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

            LocalizedButton endTurn = CreatePrimaryButton(TextKeys.Match.EndTurnButton);
            endTurn.Padding = MatchLayout.EndTurnPadding;
            endTurn.HorizontalAlignment = HorizontalAlignment.Stretch;
            endTurn.LabelAlignment = HorizontalAlignment.Center;
            endTurn.Click += EndTurnButtonOnClick;

            LocalizedButton forfeit = CreateDestructiveButton(TextKeys.Match.ForfeitButton);
            forfeit.LabelFont = Fonts.Small;
            forfeit.Padding = Metrics.SmallButtonPadding;
            forfeit.HorizontalAlignment = HorizontalAlignment.Stretch;
            forfeit.Click += ForfeitButtonOnClick;

            VerticalStackPanel chat = CreateChatPanel();

            sideColumn.Widgets.Add(chat);
            sideColumn.Widgets.Add(CreateGameSection(TextKeys.Match.DeckLabel, BuildDeck()));
            sideColumn.Widgets.Add(endTurn);
            sideColumn.Widgets.Add(CreateGameSection(TextKeys.Match.MatchLabel, forfeit));

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

