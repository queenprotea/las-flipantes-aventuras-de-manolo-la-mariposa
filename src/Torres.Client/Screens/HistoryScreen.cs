using System;
using System.Collections.Generic;
using System.Globalization;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class HistoryScreen : Screen
    {
        private const int DateColumnWidth = 140;
        private const int PlayersColumnWidth = 90;
        private const int StatColumnWidth = 90;
        private const int ResultColumnWidth = 170;

        private const int NoThickness = 0;
        private const int FirstRankPosition = 1;

        private const int ColumnCount = 6;
        private const int ColumnIndexDate = 0;
        private const int ColumnIndexPlayers = 1;
        private const int ColumnIndexOpponents = 2;
        private const int ColumnIndexRank = 3;
        private const int ColumnIndexPoints = 4;
        private const int ColumnIndexResult = 5;

        private const int HeaderRowIndex = 0;
        private const int DividerRowIndex = 1;
        private const int FirstMatchRowIndex = 2;

        private const string DateFormat = "dd MMM yyyy";

        private static readonly ChipStyle _neutralChipStyle = new ChipStyle(Theme.SurfaceAltBrush, Theme.LineBrush, Theme.MutedInk);
        private static readonly ChipStyle _blushChipStyle = new ChipStyle(Theme.BlushTintBrush, Theme.BlushBrush, Theme.BlushInk);

        private readonly List<MatchRecord> _matches = CreateSampleMatches();

        internal HistoryScreen()
            : base(TextKeys.History.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();

            Widget historyContent = _matches.Count == 0
                ? CreateEmptyState(TextKeys.History.EmptyStateTitle, TextKeys.History.EmptyStateHint)
                : BuildScrollableTable();
            page.Widgets.Add(historyContent);
            StackPanel.SetProportionType(historyContent, ProportionType.Fill);

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(CreateBackBar(backButton));

            return page;
        }

        private ScrollViewer BuildScrollableTable()
        {
            var scrollableTable = new ScrollViewer
            {
                Content = BuildTable(),
                ShowVerticalScrollBar = true,
                ShowHorizontalScrollBar = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            };

            return scrollableTable;
        }

        private Grid BuildTable()
        {
            var historyGrid = new Grid
            {
                ColumnSpacing = Metrics.ColumnSpacing,
                RowSpacing = Metrics.FieldSpacing,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            historyGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, DateColumnWidth));
            historyGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, PlayersColumnWidth));
            historyGrid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            historyGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, StatColumnWidth));
            historyGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, StatColumnWidth));
            historyGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, ResultColumnWidth));

            AddHeaderRow(historyGrid);
            AddDivider(historyGrid, DividerRowIndex);

            for (int matchIndex = 0; matchIndex < _matches.Count; matchIndex++)
            {
                AddMatchRow(historyGrid, FirstMatchRowIndex + matchIndex, _matches[matchIndex]);
            }

            return historyGrid;
        }

        private static void AddHeaderRow(Grid historyGrid)
        {
            AddCell(historyGrid, new CellPosition(HeaderRowIndex, ColumnIndexDate), CreateHint(TextKeys.History.DateColumn));
            AddCell(historyGrid, new CellPosition(HeaderRowIndex, ColumnIndexPlayers), CreateHint(TextKeys.History.PlayersColumn));
            AddCell(historyGrid, new CellPosition(HeaderRowIndex, ColumnIndexOpponents), CreateHint(TextKeys.History.OpponentsColumn));
            AddCell(historyGrid, new CellPosition(HeaderRowIndex, ColumnIndexRank), AlignRight(CreateHint(TextKeys.History.RankColumn)));
            AddCell(historyGrid, new CellPosition(HeaderRowIndex, ColumnIndexPoints), AlignRight(CreateHint(TextKeys.History.PointsColumn)));
            AddCell(historyGrid, new CellPosition(HeaderRowIndex, ColumnIndexResult), CreateHint(TextKeys.History.ResultColumn));
        }

        private static void AddDivider(Grid historyGrid, int rowIndex)
        {
            Panel dividerLine = CreateDivider();
            Grid.SetRow(dividerLine, rowIndex);
            Grid.SetColumnSpan(dividerLine, ColumnCount);
            historyGrid.Widgets.Add(dividerLine);
        }

        private void AddMatchRow(Grid historyGrid, int rowIndex, MatchRecord match)
        {
            AddCell(historyGrid, new CellPosition(rowIndex, ColumnIndexDate), CreateDataLabel(FormatDate(match.PlayedAt)));
            AddCell(historyGrid, new CellPosition(rowIndex, ColumnIndexPlayers), CreateDataLabel(DisplayFormats.FormatInteger(match.PlayerCount)));
            AddCell(historyGrid, new CellPosition(rowIndex, ColumnIndexOpponents), CreateDataLabel(FormatOpponents(match)));
            AddCell(historyGrid, new CellPosition(rowIndex, ColumnIndexRank), AlignRight(CreateDataLabel(FormatRank(match.Rank))));
            AddCell(historyGrid, new CellPosition(rowIndex, ColumnIndexPoints), AlignRight(CreateDataLabel(FormatPoints(match.Points))));
            AddCell(historyGrid, new CellPosition(rowIndex, ColumnIndexResult), BuildResultChip(match));

            AddRowClickTarget(historyGrid, rowIndex);
        }

        private void AddRowClickTarget(Grid historyGrid, int rowIndex)
        {
            var rowClickTarget = new Button
            {
                Background = null,
                OverBackground = null,
                PressedBackground = null,
                Border = null,
                BorderThickness = new Thickness(NoThickness),
                Padding = new Thickness(NoThickness),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            rowClickTarget.Click += MatchRowButtonOnClick;

            Grid.SetRow(rowClickTarget, rowIndex);
            Grid.SetColumnSpan(rowClickTarget, ColumnCount);
            historyGrid.Widgets.Add(rowClickTarget);
        }

        private static string FormatDate(DateTime playedAt)
        {
            string formattedDate = DisplayFormats.FormatDate(playedAt, DateFormat);
            string lowerCaseDate = formattedDate.ToLower(CultureInfo.CurrentCulture);

            return lowerCaseDate;
        }

        private static string FormatRank(int? rank)
        {
            string rankText = rank.HasValue ? DisplayFormats.FormatRankPosition(rank.Value) : DisplaySymbols.MissingValue;

            return rankText;
        }

        private static string FormatPoints(int? points)
        {
            string pointsText = points.HasValue ? DisplayFormats.FormatThousands(points.Value) : DisplaySymbols.MissingValue;

            return pointsText;
        }

        private static string FormatOpponents(MatchRecord match)
        {
            string namedOpponents = string.Join(DisplaySymbols.ListSeparator, match.NamedOpponents);

            if (match.GuestOpponentCount <= 0)
            {
                return namedOpponents;
            }

            string guestPhrase = match.GuestOpponentCount == 1
                ? LocalizedText.Get(TextKeys.History.GuestOpponentMessage)
                : LocalizedText.Format(TextKeys.History.GuestOpponentsMessage, match.GuestOpponentCount);

            string opponentsText = string.IsNullOrEmpty(namedOpponents) ? guestPhrase : $"{namedOpponents} {guestPhrase}";

            return opponentsText;
        }

        private static Widget BuildResultChip(MatchRecord match)
        {
            Widget resultChip = match.Status switch
            {
                MatchStatus.Finished when match.Rank == FirstRankPosition => BuildWinChip(),
                MatchStatus.Finished => BuildNeutralChip(TextKeys.History.FinishedStatus),
                MatchStatus.Forfeited => BuildNeutralChip(TextKeys.History.ForfeitedStatus),
                MatchStatus.Interrupted => BuildBlushChip(TextKeys.History.InterruptedStatus),
                _ => BuildNeutralChip(TextKeys.History.FinishedStatus),
            };

            return resultChip;
        }

        private static Panel BuildWinChip()
        {
            var winChip = new Panel
            {
                Padding = Metrics.PillButtonPadding,
                Background = Theme.MintTintBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };

            HorizontalStackPanel chipContent = CreateButtonRow();
            chipContent.Widgets.Add(new Label { Text = DisplaySymbols.CheckMark, Font = Fonts.Small, TextColor = Theme.MintInk });
            chipContent.Widgets.Add(new LocalizedLabel(TextKeys.History.FinishedStatus) { Font = Fonts.Small, TextColor = Theme.MintInk });
            winChip.Widgets.Add(chipContent);

            return winChip;
        }

        private static Panel BuildNeutralChip(string statusTextKey)
        {
            Panel neutralChip = BuildChip(statusTextKey, _neutralChipStyle);

            return neutralChip;
        }

        private static Panel BuildBlushChip(string statusTextKey)
        {
            Panel blushChip = BuildChip(statusTextKey, _blushChipStyle);

            return blushChip;
        }

        private static Panel BuildChip(string statusTextKey, ChipStyle chipStyle)
        {
            var statusChip = new Panel
            {
                Padding = Metrics.PillButtonPadding,
                Background = chipStyle.Background,
                Border = chipStyle.Border,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };
            statusChip.Widgets.Add(new LocalizedLabel(statusTextKey) { Font = Fonts.Small, TextColor = chipStyle.TextColor });

            return statusChip;
        }

        private static Label CreateDataLabel(string labelText)
        {
            var dataLabel = new Label
            {
                Text = labelText,
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return dataLabel;
        }

        private static TLabel AlignRight<TLabel>(TLabel targetLabel)
            where TLabel : Label
        {
            targetLabel.TextAlign = FontStashSharp.RichText.TextHorizontalAlignment.Right;

            return targetLabel;
        }

        private static void AddCell(Grid historyGrid, CellPosition cellPosition, Widget cellContent)
        {
            Grid.SetRow(cellContent, cellPosition.Row);
            Grid.SetColumn(cellContent, cellPosition.Column);
            historyGrid.Widgets.Add(cellContent);
        }

        private void MatchRowButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MatchDetail;
        }

        private void BackButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MainMenu;
        }

        private static List<MatchRecord> CreateSampleMatches()
        {
            var sampleMatches = new List<MatchRecord>
            {
                new MatchRecord(new DateTime(2026, 9, 5), playerCount: 4, namedOpponents: new[] { PreviewSamples.LuisPlayer, PreviewSamples.SofiaPlayer }, guestOpponentCount: 1, rank: 1, points: 148, matchStatus: MatchStatus.Finished),
                new MatchRecord(new DateTime(2026, 9, 4), playerCount: 3, namedOpponents: new[] { PreviewSamples.DiegoPlayer, PreviewSamples.MarcoPlayer }, guestOpponentCount: 0, rank: 3, points: 61, matchStatus: MatchStatus.Finished),
                new MatchRecord(new DateTime(2026, 9, 3), playerCount: 2, namedOpponents: new[] { PreviewSamples.LuisPlayer }, guestOpponentCount: 0, rank: 2, points: 44, matchStatus: MatchStatus.Forfeited),
                new MatchRecord(new DateTime(2026, 9, 2), playerCount: 4, namedOpponents: new[] { PreviewSamples.SofiaPlayer, PreviewSamples.MarcoPlayer, PreviewSamples.DiegoPlayer }, guestOpponentCount: 0, rank: 4, points: 27, matchStatus: MatchStatus.Finished),
                new MatchRecord(new DateTime(2026, 9, 1), playerCount: 3, namedOpponents: new[] { PreviewSamples.LuisPlayer, PreviewSamples.DiegoPlayer }, guestOpponentCount: 0, rank: null, points: null, matchStatus: MatchStatus.Interrupted),
            };

            return sampleMatches;
        }

        private enum MatchStatus
        {
            Finished,
            Forfeited,
            Interrupted,
        }

        private sealed class MatchRecord
        {
            internal MatchRecord(DateTime playedAt, int playerCount, IReadOnlyList<string> namedOpponents, int guestOpponentCount, int? rank, int? points, MatchStatus matchStatus)
            {
                PlayedAt = playedAt;
                PlayerCount = playerCount;
                NamedOpponents = namedOpponents;
                GuestOpponentCount = guestOpponentCount;
                Rank = rank;
                Points = points;
                Status = matchStatus;
            }

            internal DateTime PlayedAt { get; }

            internal int PlayerCount { get; }

            internal IReadOnlyList<string> NamedOpponents { get; }

            internal int GuestOpponentCount { get; }

            internal int? Rank { get; }

            internal int? Points { get; }

            internal MatchStatus Status { get; }
        }
    }
}
