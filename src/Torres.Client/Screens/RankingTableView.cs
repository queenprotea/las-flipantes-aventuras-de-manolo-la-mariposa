using System.Collections.Generic;

using FontStashSharp.RichText;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class RankingTableView
    {
        private const int HeaderRowIndex = 0;
        private const int DividerRowIndex = 1;
        private const int FirstEntryRowIndex = 2;
        private const int RankColumnIndex = 0;
        private const int PlayerColumnIndex = 1;
        private const int FirstStatColumnIndex = 2;

        private readonly int? _tableWidth;
        private readonly int _rankColumnWidth;
        private readonly IReadOnlyList<RankingColumn> _statColumns;

        internal RankingTableView(int? tableWidth, int rankColumnWidth, IReadOnlyList<RankingColumn> statColumns, IReadOnlyList<RankingRow> rows)
        {
            _tableWidth = tableWidth;
            _rankColumnWidth = rankColumnWidth;
            _statColumns = statColumns;
            Root = BuildTable(rows);
        }

        internal Grid Root { get; }

        private Grid BuildTable(IReadOnlyList<RankingRow> rows)
        {
            var rankingGrid = new Grid
            {
                ColumnSpacing = Metrics.ColumnSpacing,
                RowSpacing = Metrics.FieldSpacing,
                Width = _tableWidth,
                HorizontalAlignment = _tableWidth is null ? HorizontalAlignment.Stretch : HorizontalAlignment.Left,
            };
            AddColumnProportions(rankingGrid);
            AddHeaderRow(rankingGrid);
            AddDivider(rankingGrid, FirstStatColumnIndex + _statColumns.Count);
            for (var i = 0; i < rows.Count; i++)
            {
                AddEntryRow(rankingGrid, FirstEntryRowIndex + i, rows[i]);
            }

            return rankingGrid;
        }

        private void AddColumnProportions(Grid rankingGrid)
        {
            rankingGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, _rankColumnWidth));
            rankingGrid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            foreach (RankingColumn statColumn in _statColumns)
            {
                rankingGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, statColumn.Width));
            }
        }

        private void AddHeaderRow(Grid rankingGrid)
        {
            AddCell(rankingGrid, new CellPosition(HeaderRowIndex, RankColumnIndex), AlignRight(Screen.CreateHint(TextKeys.GlobalRanking.RowNumberColumn)));
            AddCell(rankingGrid, new CellPosition(HeaderRowIndex, PlayerColumnIndex), Screen.CreateHint(TextKeys.GlobalRanking.PlayerColumn));
            for (var i = 0; i < _statColumns.Count; i++)
            {
                AddCell(rankingGrid, new CellPosition(HeaderRowIndex, FirstStatColumnIndex + i), AlignRight(Screen.CreateHint(_statColumns[i].HeaderKey)));
            }
        }

        private static void AddDivider(Grid rankingGrid, int columnCount)
        {
            Panel dividerPanel = Screen.CreateDivider();
            Grid.SetRow(dividerPanel, DividerRowIndex);
            Grid.SetColumnSpan(dividerPanel, columnCount);
            rankingGrid.Widgets.Add(dividerPanel);
        }

        private static void AddEntryRow(Grid rankingGrid, int rowIndex, RankingRow row)
        {
            AddCell(rankingGrid, new CellPosition(rowIndex, RankColumnIndex), CreateNumericCell(row.Rank));
            AddCell(rankingGrid, new CellPosition(rowIndex, PlayerColumnIndex), CreatePlayerCell(row));
            for (var i = 0; i < row.StatValues.Count; i++)
            {
                AddCell(rankingGrid, new CellPosition(rowIndex, FirstStatColumnIndex + i), CreateNumericCell(row.StatValues[i]));
            }
        }

        private static HorizontalStackPanel CreatePlayerCell(RankingRow row)
        {
            var cellPanel = new HorizontalStackPanel { Spacing = Metrics.ButtonSpacing };
            if (row.Color is SeatColor seatColor)
            {
                cellPanel.Widgets.Add(Screen.CreateSeatSwatch(seatColor));
            }

            Color nameColor = row.IsViewer ? Theme.MintInk : Theme.Ink;
            cellPanel.Widgets.Add(CreateRowLabel(row.DisplayName, nameColor));
            if (row.IsViewer)
            {
                cellPanel.Widgets.Add(CreateYouTag());
            }

            return cellPanel;
        }

        private static Panel CreateYouTag()
        {
            var tagPanel = new Panel
            {
                Padding = Metrics.SmallButtonPadding,
                Background = Theme.MintTintBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };
            tagPanel.Widgets.Add(new LocalizedLabel(TextKeys.Common.YouTag)
            {
                Font = Fonts.Label,
                TextColor = Theme.MintInk,
            });

            return tagPanel;
        }

        private static Label CreateNumericCell(int numericValue)
        {
            string formattedValue = DisplayFormats.FormatThousands(numericValue);
            Label numericCell = AlignRight(CreateRowLabel(formattedValue, Theme.SoftInk));

            return numericCell;
        }

        private static Label CreateRowLabel(string labelText, Color labelColor)
        {
            var rowLabel = new Label
            {
                Text = labelText,
                Font = Fonts.Body,
                TextColor = labelColor,
                VerticalAlignment = VerticalAlignment.Center,
                SupportsCommands = false,
            };

            return rowLabel;
        }

        private static TLabel AlignRight<TLabel>(TLabel targetLabel)
            where TLabel : Label
        {
            targetLabel.TextAlign = TextHorizontalAlignment.Right;

            return targetLabel;
        }

        private static void AddCell(Grid rankingGrid, CellPosition position, Widget cellWidget)
        {
            Grid.SetRow(cellWidget, position.Row);
            Grid.SetColumn(cellWidget, position.Column);
            rankingGrid.Widgets.Add(cellWidget);
        }
    }
}
