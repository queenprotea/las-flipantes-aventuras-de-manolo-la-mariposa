using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
    internal sealed class MatchDetailScreen : Screen
    {
        private const int CardWidth = 420;
        private const int RankColumnWidth = 90;
        private const int PointsColumnWidth = 110;

        private const int NoSpacing = 0;
        private const int MinimumShownMinutes = 1;

        private const int ColumnCount = 3;
        private const int ColumnIndexRank = 0;
        private const int ColumnIndexPlayer = 1;
        private const int ColumnIndexPoints = 2;

        private const int HeaderRowIndex = 0;
        private const int HeaderDividerRowIndex = 1;
        private const int FirstParticipantRowIndex = 2;
        private const int RowsPerParticipant = 2;
        private const int DividerRowOffset = 1;

        private const string LeadingWeekdayPattern = @"^dddd[,\s]*";

        private static readonly Thickness _cellPadding = new Thickness(NoSpacing, Metrics.ListSpacing, NoSpacing, Metrics.ListSpacing);

        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _session;
        private readonly LocalizedLabel _loadingLabel;
        private readonly VerticalStackPanel _matchPlaceholder = new VerticalStackPanel { Spacing = Metrics.FieldSpacing, Visible = false };
        private readonly VerticalStackPanel _failureNotice;

        private Task<MatchDetailResponse>? _detailRequest;
        private int? _requestedMatchId;

        internal MatchDetailScreen(GameSessionConnection gameSessionConnection, PlayerSession session)
            : base(TextKeys.MatchDetail.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(session);

            _gameSessionConnection = gameSessionConnection;
            _session = session;
            _loadingLabel = CreateHint(TextKeys.MatchDetail.LoadingLabel);

            LocalizedButton retryButton = CreatePrimaryButton(TextKeys.Common.RetryButton);
            retryButton.Click += RetryButtonOnClick;
            _failureNotice = CreateFailureNotice(retryButton);
        }

        internal override void Open()
        {
            _detailRequest = null;
            _matchPlaceholder.Widgets.Clear();
            _matchPlaceholder.Visible = false;
            _failureNotice.Visible = false;
            _requestedMatchId = _session.MatchHistory.SelectedMatchId;

            bool canShowMatch = _session.HasAccount && _gameSessionConnection.IsOpen && _requestedMatchId.HasValue;
            if (!canShowMatch)
            {
                RequestedScreen = ScreenId.History;
                return;
            }

            StartDetailRequest(_requestedMatchId.GetValueOrDefault());
        }

        internal override void Update(GameTime gameTime)
        {
            if ((_detailRequest is null) || !_detailRequest.IsCompleted)
            {
                return;
            }

            Task<MatchDetailResponse> completedRequest = _detailRequest;
            _detailRequest = null;
            _loadingLabel.Visible = false;
            if (completedRequest.IsCompletedSuccessfully)
            {
                ShowDetail(completedRequest.Result);
            }
            else
            {
                _failureNotice.Visible = true;
            }
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();

            VerticalStackPanel card = CreateCard(CardWidth);
            card.HorizontalAlignment = HorizontalAlignment.Left;
            card.Widgets.Add(_loadingLabel);
            card.Widgets.Add(_matchPlaceholder);
            card.Widgets.Add(_failureNotice);
            card.Widgets.Add(BuildBackButton());

            page.Widgets.Add(card);

            return page;
        }

        private void StartDetailRequest(int matchId)
        {
            _failureNotice.Visible = false;
            if (!_gameSessionConnection.IsOpen)
            {
                _loadingLabel.Visible = false;
                _failureNotice.Visible = true;
                return;
            }

            _loadingLabel.Visible = true;
            _detailRequest = _gameSessionConnection.GetMatchDetailAsync(matchId);
        }

        private void ShowDetail(MatchDetailResponse detail)
        {
            if (detail.Status == MatchDetailStatus.Found)
            {
                ShowMatch(detail);
            }
            else
            {
                RequestedScreen = ScreenId.History;
            }
        }

        private void ShowMatch(MatchDetailResponse detail)
        {
            _matchPlaceholder.Widgets.Clear();
            _matchPlaceholder.Widgets.Add(BuildMatchHeading(detail));

            if (detail.Outcome == MatchOutcome.Interrupted)
            {
                _matchPlaceholder.Widgets.Add(CreateParagraph(TextKeys.MatchDetail.InterruptedHint));
            }
            else
            {
                _matchPlaceholder.Widgets.Add(BuildResultsTable(detail));
            }

            if (detail.GuestCount > 0)
            {
                _matchPlaceholder.Widgets.Add(BuildGuestNote(detail.GuestCount));
            }

            _matchPlaceholder.Visible = true;
        }

        private static ComputedLabel BuildGuestNote(int guestCount)
        {
            var guestNote = new ComputedLabel(() => FormatGuestNote(guestCount))
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                Wrap = true,
            };

            return guestNote;
        }

        private static string FormatGuestNote(int guestCount)
        {
            string guestNote = guestCount == HistoryScreen.SingleGuest
                ? LocalizedText.Get(TextKeys.MatchDetail.GuestNote)
                : LocalizedText.Format(TextKeys.MatchDetail.GuestsNote, DisplayFormats.FormatInteger(guestCount));

            return guestNote;
        }

        private static VerticalStackPanel BuildMatchHeading(MatchDetailResponse detail)
        {
            DateTime localPlayedAt = detail.PlayedAt.ToLocalTime();

            var heading = new VerticalStackPanel { Spacing = Metrics.CardHeaderSpacing };
            heading.Widgets.Add(new ComputedLabel(() => FormatLongDate(localPlayedAt))
            {
                Font = Fonts.Title,
                TextColor = Theme.Ink,
                Wrap = true,
            });
            heading.Widgets.Add(new ComputedLabel(() => FormatSummary(detail))
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                Wrap = true,
            });

            return heading;
        }

        private static string FormatSummary(MatchDetailResponse detail)
        {
            string playerCount = DisplayFormats.FormatInteger(detail.PlayerCount);
            if (!detail.DurationSeconds.HasValue)
            {
                string playersOnly = LocalizedText.Format(TextKeys.MatchDetail.PlayersOnlyLabel, playerCount);

                return playersOnly;
            }

            double durationInMinutes = TimeSpan.FromSeconds(detail.DurationSeconds.Value).TotalMinutes;
            int shownMinutes = Math.Max(MinimumShownMinutes, (int)Math.Round(durationInMinutes, MidpointRounding.AwayFromZero));
            string summary = LocalizedText.Format(TextKeys.MatchDetail.SummaryLabel, playerCount, DisplayFormats.FormatInteger(shownMinutes));

            return summary;
        }

        private static Grid BuildResultsTable(MatchDetailResponse detail)
        {
            var resultsGrid = new Grid
            {
                ColumnSpacing = Metrics.ColumnSpacing,
                RowSpacing = NoSpacing,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            resultsGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, RankColumnWidth));
            resultsGrid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            resultsGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, PointsColumnWidth));

            AddHeaderRow(resultsGrid);
            AddDivider(resultsGrid, HeaderDividerRowIndex);

            for (var participantIndex = 0; participantIndex < detail.Results.Count; participantIndex++)
            {
                int participantRowIndex = FirstParticipantRowIndex + (participantIndex * RowsPerParticipant);
                AddParticipantRow(resultsGrid, participantRowIndex, detail.Results[participantIndex]);
                AddDivider(resultsGrid, participantRowIndex + DividerRowOffset);
            }

            return resultsGrid;
        }

        private static void AddHeaderRow(Grid resultsGrid)
        {
            AddCell(resultsGrid, new CellPosition(HeaderRowIndex, ColumnIndexRank), CreateHint(TextKeys.MatchDetail.RankColumn));
            AddCell(resultsGrid, new CellPosition(HeaderRowIndex, ColumnIndexPlayer), CreateHint(TextKeys.MatchDetail.PlayerColumn));
            AddCell(resultsGrid, new CellPosition(HeaderRowIndex, ColumnIndexPoints), CreateHint(TextKeys.MatchDetail.PointsColumn));
        }

        private static void AddParticipantRow(Grid resultsGrid, int rowIndex, MatchResultEntry participant)
        {
            if (participant.IsCurrentPlayer)
            {
                AddCurrentPlayerHighlight(resultsGrid, rowIndex);
            }

            AddCell(resultsGrid, new CellPosition(rowIndex, ColumnIndexRank), CreateComputedDataLabel(() => DisplayFormats.FormatRankPosition(participant.Placement)));
            AddCell(resultsGrid, new CellPosition(rowIndex, ColumnIndexPlayer), BuildPlayerCell(participant));
            AddCell(resultsGrid, new CellPosition(rowIndex, ColumnIndexPoints), CreateComputedDataLabel(() => DisplayFormats.FormatThousands(participant.Points)));
        }

        private static Widget BuildPlayerCell(MatchResultEntry participant)
        {
            Color playerColor = participant.IsCurrentPlayer ? Theme.MintInk : Theme.Ink;
            Label playerLabel = CreateDataLabel(participant.Username, playerColor);
            if (!participant.WasWithdrawn)
            {
                return playerLabel;
            }

            HorizontalStackPanel playerCell = CreateButtonRow();
            playerCell.VerticalAlignment = VerticalAlignment.Center;
            playerCell.Widgets.Add(playerLabel);
            LocalizedLabel withdrawnTag = CreateTag(TextKeys.Common.WithdrewTag, Theme.BlushInk);
            withdrawnTag.VerticalAlignment = VerticalAlignment.Center;
            playerCell.Widgets.Add(withdrawnTag);

            return playerCell;
        }

        private static void AddCurrentPlayerHighlight(Grid resultsGrid, int rowIndex)
        {
            var highlightBand = new Panel
            {
                Background = Theme.MintTintBrush,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            Grid.SetRow(highlightBand, rowIndex);
            Grid.SetColumnSpan(highlightBand, ColumnCount);
            resultsGrid.Widgets.Add(highlightBand);
        }

        private static void AddDivider(Grid resultsGrid, int rowIndex)
        {
            Panel dividerLine = CreateDivider();
            Grid.SetRow(dividerLine, rowIndex);
            Grid.SetColumnSpan(dividerLine, ColumnCount);
            resultsGrid.Widgets.Add(dividerLine);
        }

        private static void AddCell(Grid resultsGrid, CellPosition cellPosition, Widget cellContent)
        {
            cellContent.Margin = _cellPadding;
            Grid.SetRow(cellContent, cellPosition.Row);
            Grid.SetColumn(cellContent, cellPosition.Column);
            resultsGrid.Widgets.Add(cellContent);
        }

        private static Label CreateDataLabel(string labelText, Color labelColor)
        {
            var dataLabel = new Label
            {
                Text = labelText,
                Font = Fonts.Body,
                TextColor = labelColor,
                VerticalAlignment = VerticalAlignment.Center,
                SupportsCommands = false,
            };

            return dataLabel;
        }

        private static ComputedLabel CreateComputedDataLabel(Func<string> textProvider)
        {
            var computedDataLabel = new ComputedLabel(textProvider)
            {
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return computedDataLabel;
        }

        private LocalizedButton BuildBackButton()
        {
            LocalizedButton backToHistoryButton = CreateSecondaryButton(TextKeys.MatchDetail.BackToHistoryButton);
            backToHistoryButton.HorizontalAlignment = HorizontalAlignment.Left;
            backToHistoryButton.Click += BackToHistoryButtonOnClick;

            return backToHistoryButton;
        }

        private void RetryButtonOnClick(object sender, MyraEventArgs e)
        {
            if (_requestedMatchId.HasValue)
            {
                StartDetailRequest(_requestedMatchId.GetValueOrDefault());
            }
        }

        private void BackToHistoryButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.History;
        }

        private static string FormatLongDate(DateTime playedAt)
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            string datePatternWithoutWeekday = Regex.Replace(
                currentCulture.DateTimeFormat.LongDatePattern,
                LeadingWeekdayPattern,
                string.Empty);

            string longDate = playedAt.ToString(datePatternWithoutWeekday, currentCulture);

            return longDate;
        }
    }
}
