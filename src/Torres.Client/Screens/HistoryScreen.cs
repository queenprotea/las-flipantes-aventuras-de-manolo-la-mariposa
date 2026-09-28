using System;
using System.Collections.Generic;
using System.Globalization;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Microsoft.Xna.Framework;

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

        private Widget BuildTable()
        {
            var grid = new Grid
            {
                ColumnSpacing = Metrics.ColumnSpacing,
                RowSpacing = Metrics.FieldSpacing,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, DateColumnWidth));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, PlayersColumnWidth));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, StatColumnWidth));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, StatColumnWidth));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, ResultColumnWidth));

            AddHeaderRow(grid);
            AddDivider(grid, 1);

            for (int index = 0; index < _matches.Count; index++)
            {
                AddMatchRow(grid, index + 2, _matches[index]);
            }

            return grid;
        }

        private static void AddHeaderRow(Grid grid)
        {
            AddCell(grid, 0, 0, Hint(TextKeys.History.DateColumn));
            AddCell(grid, 0, 1, Hint(TextKeys.History.PlayersColumn));
            AddCell(grid, 0, 2, Hint(TextKeys.History.OpponentsColumn));
            AddCell(grid, 0, 3, RightAligned(Hint(TextKeys.History.RankColumn)));
            AddCell(grid, 0, 4, RightAligned(Hint(TextKeys.History.PointsColumn)));
            AddCell(grid, 0, 5, Hint(TextKeys.History.ResultColumn));
        }

        private static void AddDivider(Grid grid, int row)
        {
            Panel divider = Divider();
            Grid.SetRow(divider, row);
            Grid.SetColumnSpan(divider, 6);
            grid.Widgets.Add(divider);
        }

        private static void AddMatchRow(Grid grid, int row, MatchRecord match)
        {
            AddCell(grid, row, 0, DataLabel(FormatDate(match.PlayedAt)));
            AddCell(grid, row, 1, DataLabel(match.PlayerCount.ToString(CultureInfo.CurrentCulture)));
            AddCell(grid, row, 2, DataLabel(FormatOpponents(match)));
            AddCell(grid, row, 3, RightAligned(DataLabel(FormatRank(match.Rank))));
            AddCell(grid, row, 4, RightAligned(DataLabel(FormatPoints(match.Points))));
            AddCell(grid, row, 5, BuildResultChip(match));
        }

        private static string FormatDate(DateTime playedAt)
        {
            return playedAt.ToString("dd MMM yyyy", CultureInfo.CurrentCulture).ToLower(CultureInfo.CurrentCulture);
        }

        private static string FormatRank(int? rank)
        {
            return rank.HasValue ? $"{rank.Value.ToString(CultureInfo.CurrentCulture)}.°" : "—";
        }

        private static string FormatPoints(int? points)
        {
            return points.HasValue ? points.Value.ToString("N0", CultureInfo.CurrentCulture) : "—";
        }

        private static string FormatOpponents(MatchRecord match)
        {
            string named = string.Join(", ", match.NamedOpponents);

            if (match.GuestOpponentCount <= 0)
            {
                return named;
            }

            string guestPhrase = match.GuestOpponentCount == 1
                ? LocalizedText.Get(TextKeys.History.GuestOpponentMessage)
                : LocalizedText.Format(TextKeys.History.GuestOpponentsMessage, match.GuestOpponentCount);

            return string.IsNullOrEmpty(named) ? guestPhrase : $"{named} {guestPhrase}";
        }

        private static Widget BuildResultChip(MatchRecord match)
        {
            return match.Status switch
            {
                MatchStatus.Finished when match.Rank == 1 => BuildWinChip(),
                MatchStatus.Finished => BuildNeutralChip(TextKeys.History.FinishedStatus),
                MatchStatus.Forfeited => BuildNeutralChip(TextKeys.History.ForfeitedStatus),
                MatchStatus.Interrupted => BuildBlushChip(TextKeys.History.InterruptedStatus),
                _ => BuildNeutralChip(TextKeys.History.FinishedStatus),
            };
        }

        private static Panel BuildWinChip()
        {
            var chip = new Panel
            {
                Padding = Metrics.PillButtonPadding,
                Background = Theme.MintTintBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };

            HorizontalStackPanel content = Row();
            content.Widgets.Add(new Label { Text = "✓", Font = Fonts.Small, TextColor = Theme.MintInk });
            content.Widgets.Add(new LocalizedLabel(TextKeys.History.FinishedStatus) { Font = Fonts.Small, TextColor = Theme.MintInk });
            chip.Widgets.Add(content);
            return chip;
        }

        private static Panel BuildNeutralChip(string textKey)
        {
            return BuildChip(textKey, Theme.SurfaceAltBrush, Theme.LineBrush, Theme.MutedInk);
        }

        private static Panel BuildBlushChip(string textKey)
        {
            return BuildChip(textKey, Theme.BlushTintBrush, Theme.BlushBrush, Theme.BlushInk);
        }

        private static Panel BuildChip(string textKey, IBrush background, IBrush border, Color textColor)
        {
            var chip = new Panel
            {
                Padding = Metrics.PillButtonPadding,
                Background = background,
                Border = border,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };
            chip.Widgets.Add(new LocalizedLabel(textKey) { Font = Fonts.Small, TextColor = textColor });
            return chip;
        }

        private static Label DataLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        private static T RightAligned<T>(T label)
            where T : Label
        {
            label.TextAlign = FontStashSharp.RichText.TextHorizontalAlignment.Right;
            return label;
        }

        private static void AddCell(Grid grid, int row, int column, Widget widget)
        {
            Grid.SetRow(widget, row);
            Grid.SetColumn(widget, column);
            grid.Widgets.Add(widget);
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
