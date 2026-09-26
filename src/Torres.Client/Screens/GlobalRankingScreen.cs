using System.Collections.Generic;
using System.Globalization;

using FontStashSharp.RichText;

using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class GlobalRankingScreen : Screen
    {
        private const int DividerRowIndex = 1;
        private const int FirstEntryRowIndex = 2;
        private const int RankOffset = 1;
        private const int HeaderRowIndex = 0;
        private const int ColumnSpanFullTable = 5;

        private const int ColumnIndexRank = 0;
        private const int ColumnIndexPlayer = 1;
        private const int ColumnIndexWins = 2;
        private const int ColumnIndexPoints = 3;
        private const int ColumnIndexMatches = 4;

        private readonly List<RankingEntry> _entries = SampleEntries();

        internal GlobalRankingScreen()
            : base(TextKeys.GlobalRanking.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            var pagePanel = Page();

            if (_entries.Count == 0)
            {
                var emptyStateWidget = EmptyState(TextKeys.GlobalRanking.EmptyStateTitle, TextKeys.GlobalRanking.EmptyStateHint);
                pagePanel.Widgets.Add(emptyStateWidget);
                StackPanel.SetProportionType(emptyStateWidget, ProportionType.Fill);
            }
            else
            {
                pagePanel.Widgets.Add(BuildTable());
            }

            var backButton = SecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += OnBackClick;
            pagePanel.Widgets.Add(BackBar(backButton));
            return pagePanel;
        }

        private Grid BuildTable()
        {
            var rankingGrid = new Grid
            {
                ColumnSpacing = Theme.ColumnSpacing,
                RowSpacing = Theme.FieldSpacing,
                Width = Theme.RankingTableWidth,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            rankingGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, Theme.RankingRankColumnWidth));
            rankingGrid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            rankingGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, Theme.RankingStatColumnWidth));
            rankingGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, Theme.RankingStatColumnWidth));
            rankingGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, Theme.RankingStatColumnWidth));

            AddHeaderRow(rankingGrid);
            AddDivider(rankingGrid, DividerRowIndex);

            for (var entryIndex = 0; entryIndex < _entries.Count; entryIndex++)
            {
                var tableRowIndex = entryIndex + FirstEntryRowIndex;
                var displayRank = entryIndex + RankOffset;
                AddEntryRow(rankingGrid, tableRowIndex, displayRank, _entries[entryIndex]);
            }

            return rankingGrid;
        }

        private static void AddHeaderRow(Grid rankingGrid)
        {
            AddCell(rankingGrid, HeaderRowIndex, ColumnIndexRank, RightAligned(Hint(TextKeys.GlobalRanking.RowNumberColumn)));
            AddCell(rankingGrid, HeaderRowIndex, ColumnIndexPlayer, Hint(TextKeys.GlobalRanking.PlayerColumn));
            AddCell(rankingGrid, HeaderRowIndex, ColumnIndexWins, RightAligned(Hint(TextKeys.GlobalRanking.WinsColumn)));
            AddCell(rankingGrid, HeaderRowIndex, ColumnIndexPoints, RightAligned(Hint(TextKeys.GlobalRanking.PointsColumn)));
            AddCell(rankingGrid, HeaderRowIndex, ColumnIndexMatches, RightAligned(Hint(TextKeys.GlobalRanking.MatchesColumn)));
        }

        private static void AddDivider(Grid rankingGrid, int targetRowIndex)
        {
            var dividerPanel = Divider();
            Grid.SetRow(dividerPanel, targetRowIndex);
            Grid.SetColumnSpan(dividerPanel, ColumnSpanFullTable);
            rankingGrid.Widgets.Add(dividerPanel);
        }

        private static void AddEntryRow(Grid rankingGrid, int targetRowIndex, int displayRank, RankingEntry rankingEntry)
        {
            AddCell(rankingGrid, targetRowIndex, ColumnIndexRank, RowNumber(displayRank));
            AddCell(rankingGrid, targetRowIndex, ColumnIndexPlayer, PlayerCell(rankingEntry));
            AddCell(rankingGrid, targetRowIndex, ColumnIndexWins, RowNumber(rankingEntry.Wins));
            AddCell(rankingGrid, targetRowIndex, ColumnIndexPoints, RowNumber(rankingEntry.Points));
            AddCell(rankingGrid, targetRowIndex, ColumnIndexMatches, RowNumber(rankingEntry.Matches));
        }

        private static Widget PlayerCell(RankingEntry rankingEntry)
        {
            if (!rankingEntry.IsCurrentPlayer)
            {
                return RowLabel(rankingEntry.PlayerName, Theme.Ink);
            }

            var cellPanel = Row();
            cellPanel.Widgets.Add(RowLabel(rankingEntry.PlayerName, Theme.MintInk));
            cellPanel.Widgets.Add(YouTag());
            return cellPanel;
        }

        private static Panel YouTag()
        {
            var tagPanel = new Panel
            {
                Padding = Theme.SmallButtonPadding,
                Background = Theme.MintTintBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Theme.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };
            tagPanel.Widgets.Add(new LocalizedLabel(TextKeys.Common.YouTag)
            {
                Font = Fonts.Label,
                TextColor = Theme.MintInk,
            });
            return tagPanel;
        }

        private static Label RowNumber(int numericValue)
        {
            return RightAligned(RowLabel(numericValue.ToString("N0", CultureInfo.CurrentCulture), Theme.SoftInk));
        }

        private static Label RowLabel(string labelText, Color labelColor)
        {
            return new Label
            {
                Text = labelText,
                Font = Fonts.Body,
                TextColor = labelColor,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        private static TLabel RightAligned<TLabel>(TLabel targetLabel)
            where TLabel : Label
        {
            targetLabel.TextAlign = TextHorizontalAlignment.Right;
            return targetLabel;
        }

        private static void AddCell(Grid rankingGrid, int targetRowIndex, int targetColumnIndex, Widget cellWidget)
        {
            Grid.SetRow(cellWidget, targetRowIndex);
            Grid.SetColumn(cellWidget, targetColumnIndex);
            rankingGrid.Widgets.Add(cellWidget);
        }

        private static List<RankingEntry> SampleEntries()
        {
            return new List<RankingEntry>();
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.MainMenu;
        }

        private sealed class RankingEntry
        {
            internal RankingEntry(string playerName, int wins, int points, int matches, bool isCurrentPlayer)
            {
                PlayerName = playerName;
                Wins = wins;
                Points = points;
                Matches = matches;
                IsCurrentPlayer = isCurrentPlayer;
            }

            internal string PlayerName { get; }
            internal int Wins { get; }
            internal int Points { get; }
            internal int Matches { get; }
            internal bool IsCurrentPlayer { get; }
        }
    }
}