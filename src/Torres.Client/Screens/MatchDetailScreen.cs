using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

using Microsoft.Xna.Framework;

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

        private static readonly Thickness _cellPadding = new Thickness(NoSpacing, Metrics.ListSpacing, NoSpacing, Metrics.ListSpacing);

        private readonly PlayedMatch _playedMatch = CreateSamplePlayedMatch();

        internal MatchDetailScreen()
            : base(TextKeys.MatchDetail.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();

            VerticalStackPanel card = CreateCard(CardWidth);
            card.HorizontalAlignment = HorizontalAlignment.Left;
            card.Widgets.Add(BuildMatchHeading());
            card.Widgets.Add(BuildResultsTable());
            card.Widgets.Add(BuildBackButton());

            page.Widgets.Add(card);

            return page;
        }

        private VerticalStackPanel BuildMatchHeading()
        {
            int durationInMinutes = (int)_playedMatch.Duration.TotalMinutes;

            var heading = new VerticalStackPanel { Spacing = Metrics.CardHeaderSpacing };
            heading.Widgets.Add(new ComputedLabel(() => FormatLongDate(_playedMatch.PlayedAt))
            {
                Font = Fonts.Title,
                TextColor = Theme.Ink,
                Wrap = true,
            });
            heading.Widgets.Add(new ComputedLabel(
                () => LocalizedText.Format(TextKeys.MatchDetail.SummaryLabel, _playedMatch.PlayerCount, durationInMinutes))
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

            for (int participantIndex = 0; participantIndex < _playedMatch.Participants.Count; participantIndex++)
            {
                int participantRowIndex = FirstParticipantRowIndex + (participantIndex * RowsPerParticipant);
                AddParticipantRow(resultsGrid, participantRowIndex, _playedMatch.Participants[participantIndex]);
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

        private static void AddParticipantRow(Grid resultsGrid, int rowIndex, MatchParticipantResult participant)
        {
            if (participant.IsCurrentPlayer)
            {
                AddCurrentPlayerHighlight(resultsGrid, rowIndex);
            }

            Color playerColor = participant.IsCurrentPlayer ? Theme.MintInk : Theme.Ink;

            AddCell(resultsGrid, new CellPosition(rowIndex, ColumnIndexRank), CreateComputedDataLabel(() => DisplayFormats.FormatRankPosition(participant.Rank), Theme.Ink));
            AddCell(resultsGrid, new CellPosition(rowIndex, ColumnIndexPlayer), CreateDataLabel(participant.Username, playerColor));
            AddCell(resultsGrid, new CellPosition(rowIndex, ColumnIndexPoints), CreateComputedDataLabel(() => DisplayFormats.FormatThousands(participant.Points), Theme.Ink));
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
            };

            return dataLabel;
        }

        private static ComputedLabel CreateComputedDataLabel(Func<string> textProvider, Color labelColor)
        {
            var computedDataLabel = new ComputedLabel(textProvider)
            {
                Font = Fonts.Body,
                TextColor = labelColor,
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

        private void BackToHistoryButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
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

        private static PlayedMatch CreateSamplePlayedMatch()
        {
            const int SampleDurationInMinutes = 38;

            var sampleParticipants = new List<MatchParticipantResult>
            {
                new MatchParticipantResult(rank: 1, username: PreviewSamples.AnaPlayer, points: 148, isCurrentPlayer: true),
                new MatchParticipantResult(rank: 2, username: PreviewSamples.LuisPlayer, points: 131, isCurrentPlayer: false),
                new MatchParticipantResult(rank: 3, username: PreviewSamples.SofiaPlayer, points: 96, isCurrentPlayer: false),
                new MatchParticipantResult(rank: 4, username: PreviewSamples.OrugaPlayer, points: 72, isCurrentPlayer: false),
            };

            var samplePlayedMatch = new PlayedMatch(
                playedAt: new DateTime(2026, 9, 5),
                duration: TimeSpan.FromMinutes(SampleDurationInMinutes),
                participants: sampleParticipants);

            return samplePlayedMatch;
        }
    }

    internal sealed class PlayedMatch
    {
        internal PlayedMatch(DateTime playedAt, TimeSpan duration, IReadOnlyList<MatchParticipantResult> participants)
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