using System.Globalization;
using System.Linq;

using FontStashSharp.RichText;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class ResultScreen : Screen
    {
        private const int HeaderBottomSpacing = 18;
        private const int TableCellSpacing = 9;
        private const int PlayerCellSpacing = 8;
        private const int PlayerColorSize = 20;
        private const int BackButtonTopSpacing = 26;
        private const int HintTopSpacing = 10;

        private static readonly Thickness _headerRowPadding = new Thickness(0, 0, 0, TableCellSpacing);
        private static readonly Thickness _playerRowPadding = new Thickness(0, TableCellSpacing);

        private static readonly ResultPlayer[] _previewPlayers =
        {
            new ResultPlayer(PreviewSamples.SeatOnePlayer, true, false, 52, 15, 3, Theme.MintBrush),
            new ResultPlayer(PreviewSamples.SeatTwoPlayer, false, false, 61, 0, 1, Theme.LavenderBrush),
            new ResultPlayer(PreviewSamples.SeatThreePlayer, false, true, 44, 15, 0, Theme.ButterBrush),
            new ResultPlayer(PreviewSamples.SeatFourPlayer, false, false, 30, 0, 2, Theme.SkyBrush),
        };

        private static readonly ResultPlayer[] _rankedPlayers = _previewPlayers
            .OrderByDescending(player => player.TotalPoints)
            .ToArray();

        internal ResultScreen()
            : base(TextKeys.Result.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            var screenLayout = new VerticalStackPanel
            {
                Width = Sizes.ResultWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            screenLayout.Widgets.Add(BuildWinnerHeader());
            screenLayout.Widgets.Add(BuildTable());
            screenLayout.Widgets.Add(BuildBackButton());

            LocalizedLabel roomStillOpen = CreateHint(TextKeys.Result.RoomStillOpenHint);
            roomStillOpen.TextAlign = TextHorizontalAlignment.Center;
            roomStillOpen.HorizontalAlignment = HorizontalAlignment.Center;
            roomStillOpen.Margin = new Thickness(0, HintTopSpacing, 0, 0);
            screenLayout.Widgets.Add(roomStillOpen);

            return screenLayout;
        }

        private static VerticalStackPanel BuildWinnerHeader()
        {
            var winnerHeader = new VerticalStackPanel
            {
                Margin = new Thickness(0, 0, 0, HeaderBottomSpacing),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            winnerHeader.Widgets.Add(new Panel
            {
                Width = Sizes.ResultIconSize,
                Height = Sizes.ResultIconSize,
                Background = Theme.LavenderTintBrush,
                Border = Theme.LavenderLineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            winnerHeader.Widgets.Add(new LocalizedLabel(TextKeys.Result.WinnerTitle)
            {
                Font = Fonts.Winner,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextArguments = new object[] { _rankedPlayers[0].Name },
            });
            winnerHeader.Widgets.Add(new LocalizedLabel(TextKeys.Result.SummaryLabel)
            {
                Font = Fonts.Body,
                TextColor = Theme.MutedInk,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextArguments = new object[] { _previewPlayers.Length },
            });

            return winnerHeader;
        }

        private static VerticalStackPanel BuildTable()
        {
            var playerHeader = new LocalizedLabel(TextKeys.Result.PlayerColumn)
            {
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
            };

            var headerRow = new HorizontalStackPanel
            {
                Padding = _headerRowPadding,
            };
            headerRow.Widgets.Add(new Panel
            {
                Width = Sizes.ResultRankColumnWidth,
            });
            headerRow.Widgets.Add(playerHeader);
            headerRow.Widgets.Add(BuildHeaderCell(TextKeys.Result.PlantersColumn));
            headerRow.Widgets.Add(BuildHeaderCell(TextKeys.Result.ButterflyColumn));
            headerRow.Widgets.Add(BuildHeaderCell(TextKeys.Result.CardsColumn));
            headerRow.Widgets.Add(BuildHeaderCell(TextKeys.Result.TotalColumn));
            StackPanel.SetProportionType(playerHeader, ProportionType.Fill);

            var scoreTable = new VerticalStackPanel();
            scoreTable.Widgets.Add(headerRow);
            for (var i = 0; i < _rankedPlayers.Length; i++)
            {
                scoreTable.Widgets.Add(CreateDivider());
                scoreTable.Widgets.Add(BuildPlayerRow(_rankedPlayers[i], i + 1));
            }

            return scoreTable;
        }

        private static LocalizedLabel BuildHeaderCell(string textKey)
        {
            var headerCell = new LocalizedLabel(textKey)
            {
                Width = Sizes.RankingStatColumnWidth,
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
            };

            return headerCell;
        }

        private static HorizontalStackPanel BuildPlayerRow(ResultPlayer player, int place)
        {
            HorizontalStackPanel playerCell = BuildPlayerCell(player);
            string butterflyText = player.ButterflyPoints > 0
                ? DisplayFormats.FormatBonus(player.ButterflyPoints)
                : DisplaySymbols.MissingValue;

            Label placeLabel = BuildStatCell(place.ToString(CultureInfo.CurrentCulture), Theme.MutedInk);
            placeLabel.Width = Sizes.ResultRankColumnWidth;

            var playerRow = new HorizontalStackPanel
            {
                Padding = _playerRowPadding,
            };
            playerRow.Widgets.Add(placeLabel);
            playerRow.Widgets.Add(playerCell);
            playerRow.Widgets.Add(BuildStatCell(player.PlanterPoints.ToString(CultureInfo.CurrentCulture), Theme.SoftInk));
            playerRow.Widgets.Add(BuildStatCell(butterflyText, Theme.SoftInk));
            playerRow.Widgets.Add(BuildStatCell(DisplayFormats.FormatBonus(player.CardPoints), Theme.SoftInk));
            playerRow.Widgets.Add(BuildStatCell(player.TotalPoints.ToString(CultureInfo.CurrentCulture), Theme.Ink));
            StackPanel.SetProportionType(playerCell, ProportionType.Fill);
            if (player.IsYou)
            {
                playerRow.Background = Theme.MintTintBrush;
            }

            return playerRow;
        }

        private static HorizontalStackPanel BuildPlayerCell(ResultPlayer player)
        {
            var playerCell = new HorizontalStackPanel
            {
                Spacing = PlayerCellSpacing,
            };
            playerCell.Widgets.Add(new Panel
            {
                Width = PlayerColorSize,
                Height = PlayerColorSize,
                Background = player.ColorBrush,
                VerticalAlignment = VerticalAlignment.Center,
            });
            playerCell.Widgets.Add(CreatePlayerNameLabel(player.Name, Theme.Ink));

            if (player.IsGuest)
            {
                playerCell.Widgets.Add(new LocalizedLabel(TextKeys.Common.GuestTag)
                {
                    Font = Fonts.Label,
                    TextColor = Theme.LavenderInk,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }

            return playerCell;
        }

        private static Label BuildStatCell(string cellText, Color textColor)
        {
            var statCell = new Label
            {
                Text = cellText,
                Width = Sizes.RankingStatColumnWidth,
                Font = Fonts.Body,
                TextColor = textColor,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return statCell;
        }

        private HorizontalStackPanel BuildBackButton()
        {
            LocalizedButton backButton = CreatePrimaryButton(TextKeys.Common.BackToRoomButton);
            backButton.Click += BackButtonOnClick;

            HorizontalStackPanel resultActions = CreateButtonRow();
            resultActions.Margin = new Thickness(0, BackButtonTopSpacing, 0, 0);
            resultActions.HorizontalAlignment = HorizontalAlignment.Center;
            resultActions.Widgets.Add(backButton);

            return resultActions;
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Room;
        }

        private sealed record ResultPlayer(string Name, bool IsYou, bool IsGuest, int PlanterPoints, int ButterflyPoints, int CardPoints, IBrush ColorBrush)
        {
            internal int TotalPoints => PlanterPoints + ButterflyPoints + CardPoints;
        }
    }
}
