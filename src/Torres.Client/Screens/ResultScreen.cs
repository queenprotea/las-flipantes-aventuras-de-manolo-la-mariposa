using System;
using System.Collections.Generic;
using System.Linq;

using FontStashSharp.RichText;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
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

        private readonly PlayerSession _playerSession;
        private readonly LocalizedLabel _winnerLabel = new LocalizedLabel(TextKeys.Result.WinnerTitle) { Font = Fonts.Winner, TextColor = Theme.Ink, HorizontalAlignment = HorizontalAlignment.Center };
        private readonly LocalizedLabel _summaryLabel = new LocalizedLabel(TextKeys.Result.SummaryLabel) { Font = Fonts.Body, TextColor = Theme.MutedInk, HorizontalAlignment = HorizontalAlignment.Center };
        private readonly VerticalStackPanel _scoreRows = new VerticalStackPanel();

        internal ResultScreen(PlayerSession playerSession)
            : base(TextKeys.Result.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(playerSession);

            _playerSession = playerSession;
        }

        internal override void Open()
        {
            MatchView? matchView = _playerSession.CurrentMatchView;
            if (matchView is null)
            {
                RequestedScreen = ScreenId.MainMenu;
            }
            else
            {
                ShowMatchView(matchView);
            }
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

        private static MatchSeat FindSeat(MatchView matchView, int seatNumber)
        {
            MatchSeat foundSeat = matchView.Seats.First(seat => seat.SeatNumber == seatNumber);

            return foundSeat;
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

        private static HorizontalStackPanel BuildPlayerRow(MatchSeat seat, string rankText, bool isViewer)
        {
            int planterPoints = seat.RoundScores.Sum(roundScore => roundScore.PlanterPoints);
            int butterflyPoints = seat.RoundScores.Sum(roundScore => roundScore.ButterflyPoints);
            string butterflyText = DisplaySymbols.MissingValue;
            if (butterflyPoints > 0)
            {
                butterflyText = DisplayFormats.FormatBonus(butterflyPoints);
            }

            Label rankLabel = BuildStatCell(rankText, Theme.MutedInk);
            rankLabel.Width = Sizes.ResultRankColumnWidth;
            HorizontalStackPanel playerCell = BuildPlayerCell(seat);

            var playerRow = new HorizontalStackPanel
            {
                Padding = _playerRowPadding,
            };
            playerRow.Widgets.Add(rankLabel);
            playerRow.Widgets.Add(playerCell);
            playerRow.Widgets.Add(BuildStatCell(DisplayFormats.FormatInteger(planterPoints), Theme.SoftInk));
            playerRow.Widgets.Add(BuildStatCell(butterflyText, Theme.SoftInk));
            playerRow.Widgets.Add(BuildStatCell(DisplayFormats.FormatInteger(seat.TotalScore), Theme.Ink));
            StackPanel.SetProportionType(playerCell, ProportionType.Fill);
            if (isViewer)
            {
                playerRow.Background = Theme.MintTintBrush;
            }

            return playerRow;
        }

        private static HorizontalStackPanel BuildPlayerCell(MatchSeat seat)
        {
            var playerCell = new HorizontalStackPanel
            {
                Spacing = PlayerCellSpacing,
            };
            playerCell.Widgets.Add(new Panel
            {
                Width = PlayerColorSize,
                Height = PlayerColorSize,
                Background = SeatColors.BrushFor(seat.Color),
                VerticalAlignment = VerticalAlignment.Center,
            });
            playerCell.Widgets.Add(CreatePlayerNameLabel(seat.DisplayName, Theme.Ink));
            if (seat.IsGuest)
            {
                playerCell.Widgets.Add(CreateSeatTag(TextKeys.Common.GuestTag, Theme.LavenderInk));
            }

            if (seat.IsRetired)
            {
                playerCell.Widgets.Add(CreateSeatTag(TextKeys.Common.WithdrewTag, Theme.BlushInk));
            }

            return playerCell;
        }

        private static LocalizedLabel CreateSeatTag(string textKey, Color textColor)
        {
            var seatTag = new LocalizedLabel(textKey)
            {
                Font = Fonts.Label,
                TextColor = textColor,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return seatTag;
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

        private VerticalStackPanel BuildWinnerHeader()
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
            winnerHeader.Widgets.Add(_winnerLabel);
            winnerHeader.Widgets.Add(_summaryLabel);

            return winnerHeader;
        }

        private VerticalStackPanel BuildTable()
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
            headerRow.Widgets.Add(BuildHeaderCell(TextKeys.Result.TotalColumn));
            StackPanel.SetProportionType(playerHeader, ProportionType.Fill);

            var scoreTable = new VerticalStackPanel();
            scoreTable.Widgets.Add(headerRow);
            scoreTable.Widgets.Add(_scoreRows);

            return scoreTable;
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

        private void ShowMatchView(MatchView matchView)
        {
            List<MatchSeat> rankedSeats = matchView.RankedSeatNumbers.Select(seatNumber => FindSeat(matchView, seatNumber)).ToList();
            List<MatchSeat> retiredSeats = matchView.Seats.Where(seat => !matchView.RankedSeatNumbers.Contains(seat.SeatNumber)).ToList();
            string winnerName = rankedSeats.Count > 0 ? rankedSeats[0].DisplayName : string.Empty;
            _winnerLabel.TextArguments = new string[] { winnerName };
            _summaryLabel.TextKey = matchView.WasAbandoned ? TextKeys.Result.AbandonedSummaryLabel : TextKeys.Result.SummaryLabel;
            _summaryLabel.TextArguments = new string[] { DisplayFormats.FormatInteger(matchView.Seats.Count) };

            _scoreRows.Widgets.Clear();
            for (var i = 0; i < rankedSeats.Count; i++)
            {
                _scoreRows.Widgets.Add(CreateDivider());
                _scoreRows.Widgets.Add(BuildPlayerRow(rankedSeats[i], DisplayFormats.FormatInteger(i + 1), rankedSeats[i].SeatNumber == matchView.ViewerSeatNumber));
            }

            foreach (MatchSeat retiredSeat in retiredSeats)
            {
                _scoreRows.Widgets.Add(CreateDivider());
                _scoreRows.Widgets.Add(BuildPlayerRow(retiredSeat, DisplaySymbols.MissingValue, retiredSeat.SeatNumber == matchView.ViewerSeatNumber));
            }
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            _playerSession.LeaveMatch();
            RequestedScreen = _playerSession.CurrentRoom is null ? ScreenId.MainMenu : ScreenId.Room;
        }
    }
}
