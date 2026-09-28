using System;
using System.Collections.Generic;
using System.Globalization;

using Microsoft.Xna.Framework;

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
        private const string OrdinalSuffix = ".\u00b0";
        private const string MissingValueText = "\u2014";
        private const string WinMark = "\u2713";
        private const string ThousandsSeparatedFormat = "N0";
        private const string OpponentSeparator = ", ";

        private readonly List<MatchRecord> _matches = SampleMatches();

        internal HistoryScreen()
            : base(TextKeys.History.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = Page();

            Widget content = _matches.Count == 0
                ? EmptyState(TextKeys.History.EmptyStateTitle, TextKeys.History.EmptyStateHint)
                : BuildScrollableTable();
            page.Widgets.Add(content);
            StackPanel.SetProportionType(content, ProportionType.Fill);

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += OnBackClick;
            page.Widgets.Add(BackBar(backButton));

            return page;
        }

        private ScrollViewer BuildScrollableTable()
        {
            return new ScrollViewer
            {
                Content = BuildTable(),
                ShowVerticalScrollBar = true,
                ShowHorizontalScrollBar = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            };
        }

        private Grid BuildTable()
        {
            var historyGrid = new Grid
            {
                ColumnSpacing = Theme.ColumnSpacing,
                RowSpacing = Theme.FieldSpacing,
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
            AddCell(historyGrid, HeaderRowIndex, ColumnIndexDate, Hint(TextKeys.History.DateColumn));
            AddCell(historyGrid, HeaderRowIndex, ColumnIndexPlayers, Hint(TextKeys.History.PlayersColumn));
            AddCell(historyGrid, HeaderRowIndex, ColumnIndexOpponents, Hint(TextKeys.History.OpponentsColumn));
            AddCell(historyGrid, HeaderRowIndex, ColumnIndexRank, RightAligned(Hint(TextKeys.History.RankColumn)));
            AddCell(historyGrid, HeaderRowIndex, ColumnIndexPoints, RightAligned(Hint(TextKeys.History.PointsColumn)));
            AddCell(historyGrid, HeaderRowIndex, ColumnIndexResult, Hint(TextKeys.History.ResultColumn));
        }

        private static void AddDivider(Grid historyGrid, int rowIndex)
        {
            Panel dividerLine = Divider();
            Grid.SetRow(dividerLine, rowIndex);
            Grid.SetColumnSpan(dividerLine, ColumnCount);
            historyGrid.Widgets.Add(dividerLine);
        }

        private void AddMatchRow(Grid historyGrid, int rowIndex, MatchRecord match)
        {
            AddCell(historyGrid, rowIndex, ColumnIndexDate, DataLabel(FormatDate(match.PlayedAt)));
            AddCell(historyGrid, rowIndex, ColumnIndexPlayers, DataLabel(match.PlayerCount.ToString(CultureInfo.CurrentCulture)));
            AddCell(historyGrid, rowIndex, ColumnIndexOpponents, DataLabel(FormatOpponents(match)));
            AddCell(historyGrid, rowIndex, ColumnIndexRank, RightAligned(DataLabel(FormatRank(match.Rank))));
            AddCell(historyGrid, rowIndex, ColumnIndexPoints, RightAligned(DataLabel(FormatPoints(match.Points))));
            AddCell(historyGrid, rowIndex, ColumnIndexResult, BuildResultChip(match));

            // Added last so it sits on top of the row and catches the click anywhere in it.
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
            rowClickTarget.Click += OnMatchRowClick;

            Grid.SetRow(rowClickTarget, rowIndex);
            Grid.SetColumnSpan(rowClickTarget, ColumnCount);
            historyGrid.Widgets.Add(rowClickTarget);
        }

        private static string FormatDate(DateTime playedAt)
        {
            return playedAt.ToString(DateFormat, CultureInfo.CurrentCulture).ToLower(CultureInfo.CurrentCulture);
        }

        private static string FormatRank(int? rank)
        {
            return rank.HasValue ? rank.Value.ToString(CultureInfo.CurrentCulture) + OrdinalSuffix : MissingValueText;
        }

        private static string FormatPoints(int? points)
        {
            return points.HasValue ? points.Value.ToString(ThousandsSeparatedFormat, CultureInfo.CurrentCulture) : MissingValueText;
        }

        private static string FormatOpponents(MatchRecord match)
        {
            string namedOpponents = string.Join(OpponentSeparator, match.NamedOpponents);

            if (match.GuestOpponentCount <= 0)
            {
                return namedOpponents;
            }

            string guestPhrase = match.GuestOpponentCount == 1
                ? LocalizedText.Get(TextKeys.History.GuestOpponentMessage)
                : LocalizedText.Format(TextKeys.History.GuestOpponentsMessage, match.GuestOpponentCount);

            return string.IsNullOrEmpty(namedOpponents) ? guestPhrase : $"{namedOpponents} {guestPhrase}";
        }

        private static Widget BuildResultChip(MatchRecord match)
        {
            return match.Status switch
            {
                MatchStatus.Finished when match.Rank == FirstRankPosition => BuildWinChip(),
                MatchStatus.Finished => BuildNeutralChip(TextKeys.History.FinishedStatus),
                MatchStatus.Forfeited => BuildNeutralChip(TextKeys.History.ForfeitedStatus),
                MatchStatus.Interrupted => BuildBlushChip(TextKeys.History.InterruptedStatus),
                _ => BuildNeutralChip(TextKeys.History.FinishedStatus),
            };
        }

        private static Panel BuildWinChip()
        {
            var winChip = new Panel
            {
                Padding = Theme.PillButtonPadding,
                Background = Theme.MintTintBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Theme.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };

            HorizontalStackPanel chipContent = Row();
            chipContent.Widgets.Add(new Label { Text = WinMark, Font = Fonts.Small, TextColor = Theme.MintInk });
            chipContent.Widgets.Add(new LocalizedLabel(TextKeys.History.FinishedStatus) { Font = Fonts.Small, TextColor = Theme.MintInk });
            winChip.Widgets.Add(chipContent);
            return winChip;
        }

        private static Panel BuildNeutralChip(string statusTextKey)
        {
            return BuildChip(statusTextKey, Theme.SurfaceAltBrush, Theme.LineBrush, Theme.MutedInk);
        }

        private static Panel BuildBlushChip(string statusTextKey)
        {
            return BuildChip(statusTextKey, Theme.BlushTintBrush, Theme.BlushBrush, Theme.BlushInk);
        }

        private static Panel BuildChip(string statusTextKey, IBrush chipBackground, IBrush chipBorder, Color chipTextColor)
        {
            var chip = new Panel
            {
                Padding = Theme.PillButtonPadding,
                Background = chipBackground,
                Border = chipBorder,
                BorderThickness = Theme.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };
            chip.Widgets.Add(new LocalizedLabel(statusTextKey) { Font = Fonts.Small, TextColor = chipTextColor });
            return chip;
        }

        private static Label DataLabel(string labelText)
        {
            return new Label
            {
                Text = labelText,
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        private static TLabel RightAligned<TLabel>(TLabel targetLabel)
            where TLabel : Label
        {
            targetLabel.TextAlign = FontStashSharp.RichText.TextHorizontalAlignment.Right;
            return targetLabel;
        }

        private static void AddCell(Grid historyGrid, int rowIndex, int columnIndex, Widget cellContent)
        {
            Grid.SetRow(cellContent, rowIndex);
            Grid.SetColumn(cellContent, columnIndex);
            historyGrid.Widgets.Add(cellContent);
        }

        private void OnMatchRowClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            // TODO: tell MatchDetailScreen which match was clicked once the service exists.
            RequestedScreen = ScreenId.MatchDetail;
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.MainMenu;
        }

        private static List<MatchRecord> SampleMatches() => new()
        {
            new MatchRecord(new DateTime(2026, 9, 5), playerCount: 4, namedOpponents: new[] { "luis_m", "sofia99" }, guestOpponentCount: 1, rank: 1, points: 148, status: MatchStatus.Finished),
            new MatchRecord(new DateTime(2026, 9, 4), playerCount: 3, namedOpponents: new[] { "dgomez", "marco_r" }, guestOpponentCount: 0, rank: 3, points: 61, status: MatchStatus.Finished),
            new MatchRecord(new DateTime(2026, 9, 3), playerCount: 2, namedOpponents: new[] { "luis_m" }, guestOpponentCount: 0, rank: 2, points: 44, status: MatchStatus.Forfeited),
            new MatchRecord(new DateTime(2026, 9, 2), playerCount: 4, namedOpponents: new[] { "sofia99", "marco_r", "dgomez" }, guestOpponentCount: 0, rank: 4, points: 27, status: MatchStatus.Finished),
            new MatchRecord(new DateTime(2026, 9, 1), playerCount: 3, namedOpponents: new[] { "luis_m", "dgomez" }, guestOpponentCount: 0, rank: null, points: null, status: MatchStatus.Interrupted),
        };

        private enum MatchStatus { Finished, Forfeited, Interrupted }

        private sealed class MatchRecord
        {
            internal MatchRecord(DateTime playedAt, int playerCount, IReadOnlyList<string> namedOpponents, int guestOpponentCount, int? rank, int? points, MatchStatus status)
            {
                PlayedAt = playedAt;
                PlayerCount = playerCount;
                NamedOpponents = namedOpponents;
                GuestOpponentCount = guestOpponentCount;
                Rank = rank;
                Points = points;
                Status = status;
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