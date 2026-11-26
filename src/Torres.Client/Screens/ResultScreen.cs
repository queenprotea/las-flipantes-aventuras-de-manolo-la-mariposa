using System.Collections.Generic;
using System.Linq;

using FontStashSharp.RichText;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class ResultScreen : Screen
    {
        private const int FirstRank = 1;

        private readonly PlayerSession _playerSession;
        private readonly MatchPause _matchPause;
        private readonly Image _winnerImage = new Image { Width = MatchLayout.WinnerIconWidth, Height = MatchLayout.WinnerIconHeight, HorizontalAlignment = HorizontalAlignment.Center };
        private readonly LocalizedLabel _winnerLabel = new LocalizedLabel(TextKeys.Result.WinnerTitle) { Font = Fonts.Winner, TextColor = Theme.Ink, HorizontalAlignment = HorizontalAlignment.Center };
        private readonly LocalizedLabel _summaryLabel = new LocalizedLabel(TextKeys.Result.SummaryLabel) { Font = Fonts.Body, TextColor = Theme.MutedInk, HorizontalAlignment = HorizontalAlignment.Center };
        private readonly VerticalStackPanel _scoreRows = new VerticalStackPanel { Spacing = MatchLayout.PauseRowSpacing };

        internal ResultScreen(PlayerSession playerSession, MatchPause matchPause)
            : base(TextKeys.Result.HeaderLabel, true)
        {
            _playerSession = playerSession;
            _matchPause = matchPause;
        }

        internal override void Open()
        {
            MatchView? matchView = _playerSession.CurrentMatchView;
            _matchPause.ShowTable(Root, matchView);
            if (matchView is null)
            {
                RequestedScreen = ScreenId.MainMenu;
            }
            else
            {
                ShowMatchView(matchView);
            }
        }

        internal override void Update(GameTime gameTime)
        {
            _matchPause.Advance(gameTime.ElapsedGameTime);
        }

        internal override void RenderScene()
        {
            _matchPause.RenderScene();
        }

        protected override Widget Build()
        {
            LocalizedLabel roomStillOpen = CreateHint(TextKeys.Result.RoomStillOpenHint);
            roomStillOpen.Width = MatchLayout.ResultCardWidth;
            roomStillOpen.Wrap = true;
            roomStillOpen.TextAlign = TextHorizontalAlignment.Center;
            roomStillOpen.HorizontalAlignment = HorizontalAlignment.Center;

            var winnerHeader = new VerticalStackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            winnerHeader.Widgets.Add(_winnerImage);
            winnerHeader.Widgets.Add(_winnerLabel);
            winnerHeader.Widgets.Add(_summaryLabel);

            var cardContent = new VerticalStackPanel { Spacing = MatchLayout.PauseSectionSpacing };
            cardContent.Widgets.Add(winnerHeader);
            cardContent.Widgets.Add(BuildTable());
            cardContent.Widgets.Add(BuildBackButton());
            cardContent.Widgets.Add(roomStillOpen);
            Panel cardLayer = _matchPause.CreateCardLayer(cardContent, MatchLayout.ResultCardWidth);

            return cardLayer;
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
                Width = MatchLayout.PauseStatColumnWidth,
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
                TextAlign = TextHorizontalAlignment.Center,
            };

            return headerCell;
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
                Width = MatchLayout.PauseStatColumnWidth,
                Font = Fonts.Body,
                TextColor = textColor,
                TextAlign = TextHorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return statCell;
        }

        private HorizontalStackPanel BuildPlayerCell(MatchSeat seat)
        {
            HorizontalStackPanel playerCell = _matchPause.CreatePlayerTag(seat, Fonts.Control);
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

        private HorizontalStackPanel BuildPlayerRow(MatchSeat seat, string rankText, RankedRow rankedRow)
        {
            int planterPoints = seat.RoundScores.Sum(roundScore => roundScore.PlanterPoints);
            int butterflyPoints = seat.RoundScores.Sum(roundScore => roundScore.ButterflyPoints);
            string butterflyText = DisplaySymbols.MissingValue;
            if (butterflyPoints > 0)
            {
                butterflyText = DisplayFormats.FormatBonus(butterflyPoints);
            }

            Label rankLabel = BuildStatCell(rankText, Theme.Ink);
            rankLabel.Width = MatchLayout.PauseRankColumnWidth;
            HorizontalStackPanel playerCell = BuildPlayerCell(seat);
            var totalCell = new Panel { Width = MatchLayout.PauseStatColumnWidth };
            Panel totalCircle = _matchPause.CreateScoreCircle(seat.FinalScore);
            totalCircle.HorizontalAlignment = HorizontalAlignment.Center;
            totalCell.Widgets.Add(totalCircle);

            HorizontalStackPanel playerRow = _matchPause.CreateTableRow(rankedRow.IsViewer, rankedRow.IsWinner);
            playerRow.Widgets.Add(rankLabel);
            playerRow.Widgets.Add(playerCell);
            playerRow.Widgets.Add(BuildStatCell(DisplayFormats.FormatInteger(planterPoints), Theme.SoftInk));
            playerRow.Widgets.Add(BuildStatCell(butterflyText, Theme.MintInk));
            playerRow.Widgets.Add(BuildStatCell(DisplayFormats.FormatInteger(seat.CardPoints), Theme.SoftInk));
            playerRow.Widgets.Add(totalCell);
            StackPanel.SetProportionType(playerCell, ProportionType.Fill);

            return playerRow;
        }

        private VerticalStackPanel BuildTable()
        {
            var playerHeader = new LocalizedLabel(TextKeys.Result.PlayerColumn)
            {
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
            };

            HorizontalStackPanel headerRow = _matchPause.CreateTableRow(false, false);
            headerRow.Widgets.Add(new Panel { Width = MatchLayout.PauseRankColumnWidth });
            headerRow.Widgets.Add(playerHeader);
            headerRow.Widgets.Add(BuildHeaderCell(TextKeys.Result.PlantersColumn));
            headerRow.Widgets.Add(BuildHeaderCell(TextKeys.Result.ButterflyColumn));
            headerRow.Widgets.Add(BuildHeaderCell(TextKeys.Result.CardsColumn));
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
            _matchPause.StylePrimaryPill(backButton);
            backButton.Click += BackButtonOnClick;

            HorizontalStackPanel resultActions = CreateButtonRow();
            resultActions.HorizontalAlignment = HorizontalAlignment.Center;
            resultActions.Widgets.Add(backButton);

            return resultActions;
        }

        private void ShowMatchView(MatchView matchView)
        {
            List<MatchSeat> rankedSeats = matchView.RankedSeatNumbers.Select(seatNumber => FindSeat(matchView, seatNumber)).ToList();
            List<MatchSeat> retiredSeats = matchView.Seats.Where(seat => !matchView.RankedSeatNumbers.Contains(seat.SeatNumber)).ToList();
            ShowWinner(rankedSeats);
            _summaryLabel.TextKey = matchView.WasAbandoned ? TextKeys.Result.AbandonedSummaryLabel : TextKeys.Result.SummaryLabel;
            _summaryLabel.TextArguments = new string[] { DisplayFormats.FormatInteger(matchView.Seats.Count) };

            _scoreRows.Widgets.Clear();
            for (var rankIndex = 0; rankIndex < rankedSeats.Count; rankIndex++)
            {
                MatchSeat rankedSeat = rankedSeats[rankIndex];
                var rankedRow = new RankedRow(rankedSeat.SeatNumber == matchView.ViewerSeatNumber, rankIndex == 0);
                _scoreRows.Widgets.Add(BuildPlayerRow(rankedSeat, DisplayFormats.FormatInteger(rankIndex + FirstRank), rankedRow));
            }

            foreach (MatchSeat retiredSeat in retiredSeats)
            {
                var retiredRow = new RankedRow(retiredSeat.SeatNumber == matchView.ViewerSeatNumber, false);
                _scoreRows.Widgets.Add(BuildPlayerRow(retiredSeat, DisplaySymbols.MissingValue, retiredRow));
            }
        }

        private void ShowWinner(IReadOnlyList<MatchSeat> rankedSeats)
        {
            bool hasWinner = rankedSeats.Count > 0;
            _winnerImage.Visible = hasWinner;
            _winnerLabel.TextArguments = new string[] { hasWinner ? rankedSeats[0].DisplayName : string.Empty };
            if (hasWinner)
            {
                _winnerImage.Renderable = _matchPause.FigureIcons.FindCaterpillarIcon(rankedSeats[0].Color);
            }
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            _playerSession.LeaveMatch();
            RequestedScreen = _playerSession.CurrentRoom is null ? ScreenId.MainMenu : ScreenId.Room;
        }

        private readonly record struct RankedRow(bool IsViewer, bool IsWinner);
    }
}
