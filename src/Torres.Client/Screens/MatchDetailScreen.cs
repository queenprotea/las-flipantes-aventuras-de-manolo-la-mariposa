using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MatchDetailScreen : Screen
    {
        private const int CardWidth = 420;
        private const int RankColumnWidth = 90;
        private const int PointsColumnWidth = 110;

        private const int NoSpacing = 0;

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
        private const string OrdinalSuffix = ".\u00b0";
        private const string ThousandsSeparatedFormat = "N0";

        private static readonly Thickness CellPadding = new Thickness(NoSpacing, Metrics.ListSpacing, NoSpacing, Metrics.ListSpacing);

        // TODO: replace with the match selected in HistoryScreen once the service exists.
        private readonly MatchDetailInfo _match = SampleMatchDetail();

        internal MatchDetailScreen()
            : base(TextKeys.MatchDetail.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = Page();

            VerticalStackPanel card = Card(CardWidth);
            card.HorizontalAlignment = HorizontalAlignment.Left;
            card.Widgets.Add(BuildMatchHeading());
            card.Widgets.Add(BuildResultsTable());
            card.Widgets.Add(BuildBackButton());

            page.Widgets.Add(card);
            return page;
        }

        private VerticalStackPanel BuildMatchHeading()
        {
            int durationInMinutes = (int)_match.Duration.TotalMinutes;

            var heading = new VerticalStackPanel { Spacing = Metrics.CardHeaderSpacing };
            heading.Widgets.Add(new ComputedLabel(() => FormatLongDate(_match.PlayedAt))
            {
                Font = Fonts.Title,
                TextColor = Theme.Ink,
                Wrap = true,
            });
            heading.Widgets.Add(new ComputedLabel(
                () => LocalizedText.Format(TextKeys.MatchDetail.SummaryLabel, _match.PlayerCount, durationInMinutes))
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                Wrap = true,
            });
            return heading;
        }

        private Grid BuildResultsTable()
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

            for (int participantIndex = 0; participantIndex < _match.Participants.Count; participantIndex++)
            {
                int participantRowIndex = FirstParticipantRowIndex + (participantIndex * RowsPerParticipant);
                AddParticipantRow(resultsGrid, participantRowIndex, _match.Participants[participantIndex]);
                AddDivider(resultsGrid, participantRowIndex + DividerRowOffset);
            }

            return resultsGrid;
        }

        private static void AddHeaderRow(Grid resultsGrid)
        {
            AddCell(resultsGrid, HeaderRowIndex, ColumnIndexRank, Hint(TextKeys.MatchDetail.RankColumn));
            AddCell(resultsGrid, HeaderRowIndex, ColumnIndexPlayer, Hint(TextKeys.MatchDetail.PlayerColumn));
            AddCell(resultsGrid, HeaderRowIndex, ColumnIndexPoints, Hint(TextKeys.MatchDetail.PointsColumn));
        }

        private static void AddParticipantRow(Grid resultsGrid, int rowIndex, MatchParticipantResult participant)
        {
            if (participant.IsCurrentPlayer)
            {
                AddCurrentPlayerHighlight(resultsGrid, rowIndex);
            }

            var playerColor = participant.IsCurrentPlayer ? Theme.MintInk : Theme.Ink;

            AddCell(resultsGrid, rowIndex, ColumnIndexRank,
                ComputedDataLabel(() => FormatRank(participant.Rank), Theme.Ink));
            AddCell(resultsGrid, rowIndex, ColumnIndexPlayer, DataLabel(participant.Username, playerColor));
            AddCell(resultsGrid, rowIndex, ColumnIndexPoints,
                ComputedDataLabel(() => FormatPoints(participant.Points), Theme.Ink));
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
            Panel dividerLine = Divider();
            Grid.SetRow(dividerLine, rowIndex);
            Grid.SetColumnSpan(dividerLine, ColumnCount);
            resultsGrid.Widgets.Add(dividerLine);
        }

        private static void AddCell(Grid resultsGrid, int rowIndex, int columnIndex, Widget cellContent)
        {
            cellContent.Margin = CellPadding;
            Grid.SetRow(cellContent, rowIndex);
            Grid.SetColumn(cellContent, columnIndex);
            resultsGrid.Widgets.Add(cellContent);
        }

        private static Label DataLabel(string labelText, Microsoft.Xna.Framework.Color labelColor)
        {
            return new Label
            {
                Text = labelText,
                Font = Fonts.Body,
                TextColor = labelColor,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }
        
        private static ComputedLabel ComputedDataLabel(Func<string> textProvider, Microsoft.Xna.Framework.Color labelColor)
        {
            return new ComputedLabel(textProvider)
            {
                Font = Fonts.Body,
                TextColor = labelColor,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        private LocalizedButton BuildBackButton()
        {
            LocalizedButton backToHistoryButton = SecondaryButton(TextKeys.MatchDetail.BackToHistoryButton);
            backToHistoryButton.HorizontalAlignment = HorizontalAlignment.Left;
            backToHistoryButton.Click += OnBackToHistoryClick;
            return backToHistoryButton;
        }

        private void OnBackToHistoryClick(object sender, Myra.Events.MyraEventArgs arguments)
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
            return playedAt.ToString(datePatternWithoutWeekday, currentCulture);
        }

        private static string FormatRank(int rank)
        {
            return rank.ToString(CultureInfo.CurrentCulture) + OrdinalSuffix;
        }

        private static string FormatPoints(int points)
        {
            return points.ToString(ThousandsSeparatedFormat, CultureInfo.CurrentCulture);
        }

        private static MatchDetailInfo SampleMatchDetail()
        {
            const int sampleDurationInMinutes = 38;

            var sampleParticipants = new List<MatchParticipantResult>
            {
                new MatchParticipantResult(rank: 1, username: "ana_torres", points: 148, isCurrentPlayer: true),
                new MatchParticipantResult(rank: 2, username: "luis_m", points: 131, isCurrentPlayer: false),
                new MatchParticipantResult(rank: 3, username: "sofia99", points: 96, isCurrentPlayer: false),
                new MatchParticipantResult(rank: 4, username: "Oruga_23", points: 72, isCurrentPlayer: false),
            };

            return new MatchDetailInfo(
                playedAt: new DateTime(2026, 9, 5),
                duration: TimeSpan.FromMinutes(sampleDurationInMinutes),
                participants: sampleParticipants);
        }
    }
    
    internal sealed class MatchDetailInfo
    {
        internal MatchDetailInfo(DateTime playedAt, TimeSpan duration, IReadOnlyList<MatchParticipantResult> participants)
        {
            PlayedAt = playedAt;
            Duration = duration;
            Participants = participants;
        }

        internal DateTime PlayedAt { get; }

        internal TimeSpan Duration { get; }

        internal IReadOnlyList<MatchParticipantResult> Participants { get; }

        internal int PlayerCount => Participants.Count;
    }
    internal sealed class MatchParticipantResult
    {
        internal MatchParticipantResult(int rank, string username, int points, bool isCurrentPlayer)
        {
            Rank = rank;
            Username = username;
            Points = points;
            IsCurrentPlayer = isCurrentPlayer;
        }

        internal int Rank { get; }

        internal string Username { get; }

        internal int Points { get; }

        internal bool IsCurrentPlayer { get; }
    }
}