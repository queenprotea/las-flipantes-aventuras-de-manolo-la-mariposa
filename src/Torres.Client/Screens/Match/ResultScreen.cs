using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class ResultScreen : Screen
    {
        private const int FirstRank = 1;

        private readonly MatchSummaryFlow _matchSummaryFlow;
        private readonly MatchPause _matchPause;
        private readonly PauseCardFactory _pauseCards;
        private readonly Image _winnerImage = new Image
        {
            Width = MatchLayout.WinnerIconWidth,
            Height = MatchLayout.WinnerIconHeight,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        private readonly LocalizedLabel _winnerLabel = LabelFactory.CreateCenteredTitle(TextKeys.Result.WinnerTitle, Fonts.Winner);
        private readonly LocalizedLabel _summaryLabel = new LocalizedLabel(TextKeys.Result.SummaryLabel)
        {
            Font = Fonts.Body,
            TextColor = Theme.MutedInk,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        private readonly VerticalStackPanel _scoreRows = new VerticalStackPanel { Spacing = MatchLayout.PauseRowSpacing };

        internal ResultScreen(MatchSummaryFlow matchSummaryFlow, MatchPause matchPause, PauseCardFactory pauseCards)
            : base(TextKeys.Result.HeaderLabel, true)
        {
            _matchSummaryFlow = matchSummaryFlow;
            _matchPause = matchPause;
            _pauseCards = pauseCards;
        }

        internal override void Open()
        {
            MatchView? matchView = _matchSummaryFlow.CurrentMatchView;
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
            LocalizedLabel roomStillOpen = LabelFactory.CreateCenteredHint(TextKeys.Result.RoomStillOpenHint, MatchLayout.ResultCardWidth);

            var winnerHeader = new VerticalStackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            winnerHeader.Widgets.Add(_winnerImage);
            winnerHeader.Widgets.Add(_winnerLabel);
            winnerHeader.Widgets.Add(_summaryLabel);

            var cardContent = new VerticalStackPanel { Spacing = MatchLayout.PauseSectionSpacing };
            cardContent.Widgets.Add(winnerHeader);
            cardContent.Widgets.Add(BuildTable());
            cardContent.Widgets.Add(BuildBackButton());
            cardContent.Widgets.Add(roomStillOpen);
            Panel cardLayer = _pauseCards.CreateCardLayer(cardContent, MatchLayout.ResultCardWidth);

            return cardLayer;
        }

        private static MatchSeat FindSeat(MatchView matchView, int seatNumber)
        {
            MatchSeat foundSeat = matchView.Seats.First(seat => seat.SeatNumber == seatNumber);

            return foundSeat;
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

        private HorizontalStackPanel BuildPlayerCell(MatchSeat seat)
        {
            HorizontalStackPanel playerCell = _pauseCards.CreatePlayerTag(seat, Fonts.Control);
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
            string butterflyText = DisplayFormats.FormatButterflyPoints(butterflyPoints);

            Label rankLabel = PauseCardFactory.CreateStatCell(rankText, Theme.Ink);
            rankLabel.Width = MatchLayout.PauseRankColumnWidth;
            HorizontalStackPanel playerCell = BuildPlayerCell(seat);
            Panel totalCell = _pauseCards.CreateTotalCell(seat.FinalScore);

            HorizontalStackPanel playerRow = _pauseCards.CreateTableRow(rankedRow.IsViewer, rankedRow.IsWinner);
            playerRow.Widgets.Add(rankLabel);
            playerRow.Widgets.Add(playerCell);
            playerRow.Widgets.Add(PauseCardFactory.CreateStatCell(DisplayFormats.FormatInteger(planterPoints), Theme.SoftInk));
            playerRow.Widgets.Add(PauseCardFactory.CreateStatCell(butterflyText, Theme.MintInk));
            playerRow.Widgets.Add(PauseCardFactory.CreateStatCell(DisplayFormats.FormatInteger(seat.CardPoints), Theme.SoftInk));
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

            HorizontalStackPanel headerRow = _pauseCards.CreateTableRow(false, false);
            headerRow.Widgets.Add(new Panel { Width = MatchLayout.PauseRankColumnWidth });
            headerRow.Widgets.Add(playerHeader);
            headerRow.Widgets.Add(PauseCardFactory.CreateHeaderCell(TextKeys.Result.PlantersColumn));
            headerRow.Widgets.Add(PauseCardFactory.CreateHeaderCell(TextKeys.Result.ButterflyColumn));
            headerRow.Widgets.Add(PauseCardFactory.CreateHeaderCell(TextKeys.Result.CardsColumn));
            headerRow.Widgets.Add(PauseCardFactory.CreateHeaderCell(TextKeys.Result.TotalColumn));
            StackPanel.SetProportionType(playerHeader, ProportionType.Fill);

            var scoreTable = new VerticalStackPanel();
            scoreTable.Widgets.Add(headerRow);
            scoreTable.Widgets.Add(_scoreRows);

            return scoreTable;
        }

        private HorizontalStackPanel BuildBackButton()
        {
            LocalizedButton backButton = ButtonFactory.CreatePrimaryButton(TextKeys.Common.BackToRoomButton);
            _pauseCards.StylePrimaryPill(backButton);
            backButton.Click += BackButtonOnClick;

            HorizontalStackPanel resultActions = PanelFactory.CreateButtonRow();
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
            RequestedScreen = _matchSummaryFlow.LeaveMatch();
        }

        private readonly record struct RankedRow(bool IsViewer, bool IsWinner);
    }
}
