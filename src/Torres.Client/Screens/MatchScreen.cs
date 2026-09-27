using System.ComponentModel;
using System.Globalization;
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
        private const int PreviewTurn = 2;
        private const int PreviewActionPoints = 5;
        private const int PreviewCaterpillarsLeft = 3;
        private const int PreviewBuildings = 3;
        private const string PreviewYourName = "ana_torres";
        private const int PreviewYourPoints = 34;
        private const string PreviewRivalName = "sofia99";
        private const int PreviewRivalPoints = 41;

        private static readonly int[] _previewCardNumbers = { 1, 2, 4, 5, 6, 7, 8 };
        
        internal MatchScreen() : base(TextKeys.Match.HeaderLabel, true)
        {
            HeaderArguments = new object[] { PreviewRoomCode };
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
            page.Widgets.Add(columns);
            StackPanel.SetProportionType(columns, ProportionType.Fill);
            
            return page;
        }

        private static VerticalStackPanel BuildSection(string labelKey, Widget content)
        {
            var section = new VerticalStackPanel()
            {
                Spacing = MatchLayout.SectionLabelSpacing,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            section.Widgets.Add(new LocalizedLabel(labelKey)
            {
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
            });
            
            section.Widgets.Add(content);
            
            return section;
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
            players.Widgets.Add(BuildPlayerRow(PreviewYourName, PreviewYourPoints, true));
            players.Widgets.Add(BuildPlayerRow(PreviewRivalName, PreviewRivalPoints, false));
            
            var sideBar = new VerticalStackPanel()
            {
                Spacing = Metrics.ColumnSpacing,
                Width = MatchLayout.SidebarWidth,
                VerticalAlignment = VerticalAlignment.Top,
            };

            sideBar.Widgets.Add(BuildSection(TextKeys.Match.PlayersLabel, players));
            sideBar.Widgets.Add(Divider());
            sideBar.Widgets.Add(BuildSection(TextKeys.Match.YourCaterpillarsLabel, BuildPipRow(CaterpillarsPerPlayer, PreviewCaterpillarsLeft, MatchLayout.CaterpillarPipSize)));
            sideBar.Widgets.Add(BuildSection(TextKeys.Match.BuildingsLabel, BuildPipRow(PreviewBuildings, PreviewBuildings, MatchLayout.BuildingPipSize)));

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
                VerticalAlignment =  VerticalAlignment.Center,
            };
            
            identity.Widgets.Add(new Label()
            {
                Text = name,
                Font = Fonts.Control,
                TextColor = isYou ? Theme.MintInk : Theme.Ink,
            });

            var spacer = new Panel();
            var row = new HorizontalStackPanel()
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerRowPadding,
            };
            
            row.Widgets.Add(icon);
            row.Widgets.Add(identity);
            row.Widgets.Add(spacer);
            row.Widgets.Add(new Label()
            {
                Text = points.ToString(CultureInfo.CurrentCulture),
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
                VerticalAlignment =  VerticalAlignment.Center,
            });
            
            StackPanel.SetProportionType(spacer, ProportionType.Fill);

            if (isYou)
            {
                row.Background = Theme.MintTintBrush;
                identity.Widgets.Add((new LocalizedLabel(TextKeys.Common.YouTag)
                {
                    Font =  Fonts.Label,
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

        private static VerticalStackPanel BuildCenter()
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

        private static HorizontalStackPanel BuildTurnStatus()
        {
            var status = new HorizontalStackPanel
            {
                Spacing = Metrics.FieldLabelSpacing,
                HorizontalAlignment =  HorizontalAlignment.Center,
            };
            
            status.Widgets.Add(new LocalizedLabel(TextKeys.Match.RoundTurnInstruction)
            {
                Font =  Fonts.Small,
                TextColor = Theme.MutedInk,
                TextArguments = new object[] { PreviewRound, PreviewTurn}
            }); 
            
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
                TextColor =  Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            });

            var contentButton = BuildContentButton(content);
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
            Panel cardBack = CreatePlaceholder();
            cardBack.Width = MatchLayout.CardWidth;
            cardBack.Height = MatchLayout.CardHeight;

            var drawLabel = new LocalizedLabel(TextKeys.Match.DrawButton)
            {
                Font =  Fonts.Small,
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

        private static VerticalStackPanel BuildSideColumn()
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

            LocalizedButton forfeit = PrimaryButton(TextKeys.Match.ForfeitButton);
            forfeit.LabelFont = Fonts.Small;
            forfeit.Padding = Metrics.SmallButtonPadding;
            forfeit.HorizontalAlignment = HorizontalAlignment.Stretch;

            VerticalStackPanel chat = BuildChat();
            
            sideColumn.Widgets.Add(chat);
            sideColumn.Widgets.Add(BuildSection(TextKeys.Match.DeckLabel, BuildDeck()));
            sideColumn.Widgets.Add(endTurn);
            sideColumn.Widgets.Add(BuildSection(TextKeys.Match.MatchLabel, forfeit));
           
            
            StackPanel.SetProportionType(chat, ProportionType.Fill);
            return sideColumn;
        }

        private static VerticalStackPanel BuildChat()
        {
            var chat = new VerticalStackPanel
            {
                Spacing = MatchLayout.ChatSpacing,
                Padding =  MatchLayout.ChatPadding,
                MinHeight = MatchLayout.ChatMinHeight,
                Background = Theme.SurfaceSunkenBrush,
                
            };

            var spacer = new Panel();

            chat.Widgets.Add(new LocalizedLabel(TextKeys.Chat.NoMessages)
            {
                Font = Fonts.Small,
                TextColor = Theme.MintInk,
            });
            
            chat.Widgets.Add(spacer);
            chat.Widgets.Add(Divider());

            chat.Widgets.Add(new LocalizedLabel(TextKeys.Chat.InputPlaceholder)
            {
                Font = Fonts.Small,
                TextColor = Theme.MintInk,
            });
            
            StackPanel.SetProportionType(spacer, ProportionType.Fill);
            
            return chat;
        }
    }
}

