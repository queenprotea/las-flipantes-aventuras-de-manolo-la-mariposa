using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;
using System.Threading.Tasks;

using FontStashSharp.RichText;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class GlobalRankingScreen : Screen
    {
        private const int NoticeSpacing = 14;

        private const int HeaderRowIndex = 0;
        private const int DividerRowIndex = 1;
        private const int FirstEntryRowIndex = 2;
        private const int ColumnSpanFullTable = 5;

        private const int ColumnIndexRank = 0;
        private const int ColumnIndexPlayer = 1;
        private const int ColumnIndexWins = 2;
        private const int ColumnIndexPoints = 3;
        private const int ColumnIndexMatches = 4;

        private readonly ChannelFactory<IRankingService> _rankingChannelFactory;

        private Widget? _contentPlaceholder;
        private LocalizedLabel? _loadingLabel;
        private VerticalStackPanel? _failureNotice;
        private IRankingService? _rankingChannel;
        private Task<GlobalRankingResponseContract>? _rankingRequest;
        private List<RankingEntryContract>? _entries;

        internal GlobalRankingScreen(ChannelFactory<IRankingService> rankingChannelFactory)
            : base(TextKeys.GlobalRanking.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(rankingChannelFactory);
            _rankingChannelFactory = rankingChannelFactory;
        }

        internal override void Update(GameTime gameTime)
        {
            _rankingRequest ??= StartRankingRequest();
            if (_contentPlaceholder is null || _loadingLabel is null || _failureNotice is null)
            {
                return;
            }

            if (_entries is not null || !_rankingRequest.IsCompleted)
            {
                return;
            }

            _loadingLabel.Visible = false;

            if (!_rankingRequest.IsCompletedSuccessfully)
            {
                if (_rankingChannel is ICommunicationObject failedChannel)
                {
                    failedChannel.Abort();
                }

                _failureNotice.Visible = true;
                return;
            }

            if (_rankingChannel is ICommunicationObject completedChannel)
            {
                completedChannel.Close();
            }

            _entries = _rankingRequest.Result.Entries;
            RenderEntries(_entries);
        }

        protected override Widget Build()
        {
            VerticalStackPanel pagePanel = Page();

            _loadingLabel = Hint(TextKeys.GlobalRanking.LoadingLabel);
            pagePanel.Widgets.Add(_loadingLabel);

            var placeholder = new VerticalStackPanel { Visible = false };
            _contentPlaceholder = placeholder;
            pagePanel.Widgets.Add(placeholder);
            StackPanel.SetProportionType(placeholder, ProportionType.Fill);

            _failureNotice = BuildFailureNotice();
            pagePanel.Widgets.Add(_failureNotice);

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += OnBackClick;
            pagePanel.Widgets.Add(BackBar(backButton));

            return pagePanel;
        }

        private Task<GlobalRankingResponseContract> StartRankingRequest()
        {
            _rankingChannel = _rankingChannelFactory.CreateChannel();
            return _rankingChannel.GetGlobalRankingAsync();
        }

        private void RenderEntries(List<RankingEntryContract> entries)
        {
            var placeholder = (VerticalStackPanel)_contentPlaceholder!;
            placeholder.Widgets.Clear();

            if (entries.Count == 0)
            {
                placeholder.Widgets.Add(EmptyState(TextKeys.GlobalRanking.EmptyStateTitle, TextKeys.GlobalRanking.EmptyStateHint));
            }
            else
            {
                placeholder.Widgets.Add(BuildTable(entries));
            }

            placeholder.Visible = true;
        }

        private VerticalStackPanel BuildFailureNotice()
        {
            var message = new VerticalStackPanel
            {
                Padding = Theme.CardPadding,
                Background = Theme.BlushTintBrush,
                Border = Theme.BlushBrush,
                BorderThickness = Theme.Border,
            };
            message.Widgets.Add(new LocalizedLabel(TextKeys.Common.ServerErrorTitle)
            {
                Font = Fonts.Body,
                TextColor = Theme.BlushInk,
                Wrap = true,
            });
            message.Widgets.Add(Paragraph(TextKeys.Common.ServerErrorDetail));

            LocalizedButton retryButton = PrimaryButton(TextKeys.Common.RetryButton);
            retryButton.Click += OnRetryClick;

            var notice = new VerticalStackPanel
            {
                Spacing = Theme.FieldSpacing,
                Margin = new Thickness(0, NoticeSpacing, 0, 0),
                Visible = false,
            };
            notice.Widgets.Add(message);
            notice.Widgets.Add(retryButton);
            return notice;
        }

        private static Grid BuildTable(List<RankingEntryContract> entries)
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

            for (int entryIndex = 0; entryIndex < entries.Count; entryIndex++)
            {
                int rank = entryIndex + 1;
                AddEntryRow(rankingGrid, FirstEntryRowIndex + entryIndex, rank, entries[entryIndex]);
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
            Panel dividerPanel = Divider();
            Grid.SetRow(dividerPanel, targetRowIndex);
            Grid.SetColumnSpan(dividerPanel, ColumnSpanFullTable);
            rankingGrid.Widgets.Add(dividerPanel);
        }

        private static void AddEntryRow(Grid rankingGrid, int targetRowIndex, int rank, RankingEntryContract entry)
        {
            AddCell(rankingGrid, targetRowIndex, ColumnIndexRank, RowNumber(rank));
            AddCell(rankingGrid, targetRowIndex, ColumnIndexPlayer, RowLabel(entry.PlayerName, Theme.Ink));
            AddCell(rankingGrid, targetRowIndex, ColumnIndexWins, RowNumber(entry.Wins));
            AddCell(rankingGrid, targetRowIndex, ColumnIndexPoints, RowNumber(entry.Points));
            AddCell(rankingGrid, targetRowIndex, ColumnIndexMatches, RowNumber(entry.Matches));
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

        private void OnBackClick(object sender, MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.MainMenu;
        }

        private void OnRetryClick(object sender, MyraEventArgs arguments)
        {
            if (_rankingChannel is ICommunicationObject failedChannel)
            {
                failedChannel.Abort();
            }

            _rankingRequest = null;
            _entries = null;
            _failureNotice!.Visible = false;
            _loadingLabel!.Visible = true;
        }
    }
}