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
        private const string NoPointsMark = "—";
        private const string BonusSign = "+";

        private static readonly Thickness _headerRowPadding = new Thickness(0, 0, 0, TableCellSpacing);
        private static readonly Thickness _playerRowPadding = new Thickness(0, TableCellSpacing);

        private static readonly ResultPlayer[] _previewPlayers =
        {
            new ResultPlayer("jesus", true, false, 52, 15, 3, Theme.MintBrush),
            new ResultPlayer("valentin", false, false, 61, 0, 1, Theme.LavenderBrush),
            new ResultPlayer("salma", false, true, 44, 15, 0, Theme.ButterBrush),
            new ResultPlayer("scarleth", false, false, 30, 0, 2, Theme.SkyBrush),
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
            var content = new VerticalStackPanel
            {
                Width = Sizes.ResultWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            content.Widgets.Add(BuildWinnerHeader());
            content.Widgets.Add(BuildTable());
            content.Widgets.Add(BuildBackButton());

            LocalizedLabel roomStillOpen = Hint(TextKeys.Result.RoomStillOpenHint);
            roomStillOpen.TextAlign = TextHorizontalAlignment.Center;
            roomStillOpen.HorizontalAlignment = HorizontalAlignment.Center;
            roomStillOpen.Margin = new Thickness(0, HintTopSpacing, 0, 0);
            content.Widgets.Add(roomStillOpen);

            return content;
        }

        private static VerticalStackPanel BuildWinnerHeader()
        {
            var header = new VerticalStackPanel
            {
                Margin = new Thickness(0, 0, 0, HeaderBottomSpacing),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            header.Widgets.Add(new Panel
            {
                Width = Sizes.ResultIconSize,
                Height = Sizes.ResultIconSize,
                Background = Theme.LavenderTintBrush,
                Border = Theme.LavenderLineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            header.Widgets.Add(new LocalizedLabel(TextKeys.Result.WinnerTitle)
            {
                Font = Fonts.Winner,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextArguments = new object[] { _rankedPlayers[0].Name },
            });
            header.Widgets.Add(new LocalizedLabel(TextKeys.Result.SummaryLabel)
            {
                Font = Fonts.Body,
                TextColor = Theme.MutedInk,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextArguments = new object[] { _previewPlayers.Length },
            });

            return header;
        }

        private static VerticalStackPanel BuildTable()
        {
            var playerHeader = new LocalizedLabel(TextKeys.Result.PlayerColumn)
            {
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
            };

            var header = new HorizontalStackPanel
            {
                Padding = _headerRowPadding,
            };
            header.Widgets.Add(new Panel
            {
                Width = Sizes.ResultRankColumnWidth,
            });
            header.Widgets.Add(playerHeader);
            header.Widgets.Add(BuildHeaderCell(TextKeys.Result.PlantersColumn));
            header.Widgets.Add(BuildHeaderCell(TextKeys.Result.ButterflyColumn));
            header.Widgets.Add(BuildHeaderCell(TextKeys.Result.CardsColumn));
            header.Widgets.Add(BuildHeaderCell(TextKeys.Result.TotalColumn));
            StackPanel.SetProportionType(playerHeader, ProportionType.Fill);

            var table = new VerticalStackPanel();
            table.Widgets.Add(header);
            for (var i = 0; i < _rankedPlayers.Length; i++)
            {
                table.Widgets.Add(Divider());
                table.Widgets.Add(BuildPlayerRow(_rankedPlayers[i], i + 1));
            }

            return table;
        }

        private static LocalizedLabel BuildHeaderCell(string textKey)
        {
            return new LocalizedLabel(textKey)
            {
                Width = Sizes.RankingStatColumnWidth,
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
            };
        }

        private static HorizontalStackPanel BuildPlayerRow(ResultPlayer player, int place)
        {
            HorizontalStackPanel playerCell = BuildPlayerCell(player);
            string butterflyText = player.ButterflyPoints > 0
                ? BonusSign + player.ButterflyPoints.ToString(CultureInfo.CurrentCulture)
                : NoPointsMark;

            Label placeLabel = BuildStatCell(place.ToString(CultureInfo.CurrentCulture), Theme.MutedInk);
            placeLabel.Width = Sizes.ResultRankColumnWidth;

            var row = new HorizontalStackPanel
            {
                Padding = _playerRowPadding,
            };
            row.Widgets.Add(placeLabel);
            row.Widgets.Add(playerCell);
            row.Widgets.Add(BuildStatCell(player.PlanterPoints.ToString(CultureInfo.CurrentCulture), Theme.SoftInk));
            row.Widgets.Add(BuildStatCell(butterflyText, Theme.SoftInk));
            row.Widgets.Add(BuildStatCell(BonusSign + player.CardPoints.ToString(CultureInfo.CurrentCulture), Theme.SoftInk));
            row.Widgets.Add(BuildStatCell(player.TotalPoints.ToString(CultureInfo.CurrentCulture), Theme.Ink));
            StackPanel.SetProportionType(playerCell, ProportionType.Fill);
            if (player.IsYou)
            {
                row.Background = Theme.MintTintBrush;
            }

            return row;
        }

        private static HorizontalStackPanel BuildPlayerCell(ResultPlayer player)
        {
            var cell = new HorizontalStackPanel
            {
                Spacing = PlayerCellSpacing,
            };
            cell.Widgets.Add(new Panel
            {
                Width = PlayerColorSize,
                Height = PlayerColorSize,
                Background = player.ColorBrush,
                VerticalAlignment = VerticalAlignment.Center,
            });
            cell.Widgets.Add(PlayerNameLabel(player.Name, Theme.Ink));

            if (player.IsGuest)
            {
                cell.Widgets.Add(new LocalizedLabel(TextKeys.Common.GuestTag)
                {
                    Font = Fonts.Label,
                    TextColor = Theme.LavenderInk,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }

            return cell;
        }

        private static Label BuildStatCell(string text, Color textColor)
        {
            return new Label
            {
                Text = text,
                Width = Sizes.RankingStatColumnWidth,
                Font = Fonts.Body,
                TextColor = textColor,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        private HorizontalStackPanel BuildBackButton()
        {
            LocalizedButton backButton = PrimaryButton(TextKeys.Common.BackToRoomButton);
            backButton.Click += BackButtonOnClick;

            HorizontalStackPanel actions = Row();
            actions.Margin = new Thickness(0, BackButtonTopSpacing, 0, 0);
            actions.HorizontalAlignment = HorizontalAlignment.Center;
            actions.Widgets.Add(backButton);

            return actions;
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
