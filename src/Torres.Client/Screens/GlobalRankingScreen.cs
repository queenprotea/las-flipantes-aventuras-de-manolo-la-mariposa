using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

using FontStashSharp.RichText;

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
        private readonly PlayerSession _session;
        private readonly VerticalStackPanel _contentPlaceholder = new VerticalStackPanel { Visible = false };
        private readonly LocalizedLabel _loadingLabel = CreateHint(TextKeys.GlobalRanking.LoadingLabel);
        private readonly VerticalStackPanel _failureNotice;

        private IRankingService? _rankingChannel;
        private Task<GlobalRankingResponse>? _rankingRequest;
        private List<RankingEntry>? _rankingEntries;

        internal GlobalRankingScreen(ChannelFactory<IRankingService> rankingChannelFactory, PlayerSession session)
            : base(TextKeys.GlobalRanking.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(rankingChannelFactory);
            ArgumentNullException.ThrowIfNull(session);

            _rankingChannelFactory = rankingChannelFactory;
            _session = session;
            _failureNotice = BuildFailureNotice();
        }

        internal override void Update(GameTime gameTime)
        {
            _rankingRequest ??= StartRankingRequestAsync();
            if ((_rankingEntries is not null) || !_rankingRequest.IsCompleted)
            {
                return;
            }

            _loadingLabel.Visible = false;
            if (_rankingRequest.IsCompletedSuccessfully)
            {
                ShowRanking(_rankingRequest.Result);
            }
            else
            {
                ShowFailure();
            }
        }

        protected override Widget Build()
        {
            VerticalStackPanel pagePanel = CreatePage();
            pagePanel.Widgets.Add(_loadingLabel);
            pagePanel.Widgets.Add(_contentPlaceholder);
            StackPanel.SetProportionType(_contentPlaceholder, ProportionType.Fill);
            pagePanel.Widgets.Add(_failureNotice);

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            pagePanel.Widgets.Add(CreateBackBar(backButton));

            return pagePanel;
        }

        private Task<GlobalRankingResponse> StartRankingRequestAsync()
        {
            _rankingChannel = _rankingChannelFactory.CreateChannel();

            Task<GlobalRankingResponse> rankingRequest = _rankingChannel.GetGlobalRankingAsync();

            return rankingRequest;
        }

        private void ShowRanking(GlobalRankingResponse response)
        {
            if (_rankingChannel is ICommunicationObject completedChannel)
            {
                completedChannel.Close();
            }

            _rankingEntries = response.Entries;
            RenderEntries(_rankingEntries);
        }

        private void ShowFailure()
        {
            if (_rankingChannel is ICommunicationObject failedChannel)
            {
                failedChannel.Abort();
            }

            _failureNotice.Visible = true;
        }

        private void RenderEntries(List<RankingEntry> entries)
        {
            _contentPlaceholder.Widgets.Clear();

            if (entries.Count == 0)
            {
                _contentPlaceholder.Widgets.Add(CreateEmptyState(TextKeys.GlobalRanking.EmptyStateTitle, TextKeys.GlobalRanking.EmptyStateHint));
            }
            else
            {
                _contentPlaceholder.Widgets.Add(BuildTable(entries));
            }

            _contentPlaceholder.Visible = true;
        }

        private VerticalStackPanel BuildFailureNotice()
        {
            var failureMessage = new VerticalStackPanel
            {
                Padding = Metrics.CardPadding,
                Background = Theme.BlushTintBrush,
                Border = Theme.BlushBrush,
                BorderThickness = Sizes.Border,
            };
            failureMessage.Widgets.Add(new LocalizedLabel(TextKeys.Common.ServerErrorTitle)
            {
                Font = Fonts.Body,
                TextColor = Theme.BlushInk,
                Wrap = true,
            });
            failureMessage.Widgets.Add(CreateParagraph(TextKeys.Common.ServerErrorDetail));

            LocalizedButton retryButton = CreatePrimaryButton(TextKeys.Common.RetryButton);
            retryButton.Click += RetryButtonOnClick;

            var failureNotice = new VerticalStackPanel
            {
                Spacing = Metrics.FieldSpacing,
                Margin = new Thickness(0, NoticeSpacing, 0, 0),
                Visible = false,
            };
            failureNotice.Widgets.Add(failureMessage);
            failureNotice.Widgets.Add(retryButton);

            return failureNotice;
        }

        private Grid BuildTable(List<RankingEntry> entries)
        {
            var rankingGrid = new Grid
            {
                ColumnSpacing = Metrics.ColumnSpacing,
                RowSpacing = Metrics.FieldSpacing,
                Width = Sizes.RankingTableWidth,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            rankingGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, Sizes.RankingRankColumnWidth));
            rankingGrid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            rankingGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, Sizes.RankingStatColumnWidth));
            rankingGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, Sizes.RankingStatColumnWidth));
            rankingGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, Sizes.RankingStatColumnWidth));

            AddHeaderRow(rankingGrid);
            AddDivider(rankingGrid, DividerRowIndex);

            for (var entryIndex = 0; entryIndex < entries.Count; entryIndex++)
            {
                AddEntryRow(rankingGrid, entryIndex, entries[entryIndex]);
            }

            return rankingGrid;
        }

        private static void AddHeaderRow(Grid rankingGrid)
        {
            AddCell(rankingGrid, new CellPosition(HeaderRowIndex, ColumnIndexRank), AlignRight(CreateHint(TextKeys.GlobalRanking.RowNumberColumn)));
            AddCell(rankingGrid, new CellPosition(HeaderRowIndex, ColumnIndexPlayer), CreateHint(TextKeys.GlobalRanking.PlayerColumn));
            AddCell(rankingGrid, new CellPosition(HeaderRowIndex, ColumnIndexWins), AlignRight(CreateHint(TextKeys.GlobalRanking.WinsColumn)));
            AddCell(rankingGrid, new CellPosition(HeaderRowIndex, ColumnIndexPoints), AlignRight(CreateHint(TextKeys.GlobalRanking.PointsColumn)));
            AddCell(rankingGrid, new CellPosition(HeaderRowIndex, ColumnIndexMatches), AlignRight(CreateHint(TextKeys.GlobalRanking.MatchesColumn)));
        }

        private static void AddDivider(Grid rankingGrid, int targetRowIndex)
        {
            Panel dividerPanel = CreateDivider();
            Grid.SetRow(dividerPanel, targetRowIndex);
            Grid.SetColumnSpan(dividerPanel, ColumnSpanFullTable);
            rankingGrid.Widgets.Add(dividerPanel);
        }

        private void AddEntryRow(Grid rankingGrid, int entryIndex, RankingEntry entry)
        {
            int rowIndex = FirstEntryRowIndex + entryIndex;
            int rank = entryIndex + 1;
            AddCell(rankingGrid, new CellPosition(rowIndex, ColumnIndexRank), CreateNumericCell(rank));
            AddCell(rankingGrid, new CellPosition(rowIndex, ColumnIndexPlayer), CreatePlayerCell(entry));
            AddCell(rankingGrid, new CellPosition(rowIndex, ColumnIndexWins), CreateNumericCell(entry.Wins));
            AddCell(rankingGrid, new CellPosition(rowIndex, ColumnIndexPoints), CreateNumericCell(entry.Points));
            AddCell(rankingGrid, new CellPosition(rowIndex, ColumnIndexMatches), CreateNumericCell(entry.Matches));
        }

        private Widget CreatePlayerCell(RankingEntry entry)
        {
            if (!IsCurrentPlayer(entry))
            {
                Label otherPlayerLabel = CreateRowLabel(entry.PlayerName, Theme.Ink);
                return otherPlayerLabel;
            }

            HorizontalStackPanel cellPanel = CreateButtonRow();
            cellPanel.Widgets.Add(CreateRowLabel(entry.PlayerName, Theme.MintInk));
            cellPanel.Widgets.Add(CreateYouTag());

            return cellPanel;
        }

        private bool IsCurrentPlayer(RankingEntry entry)
        {
            bool isCurrentPlayer = (_session.Player is PlayerIdentity player)
                && string.Equals(player.Username, entry.PlayerName, StringComparison.Ordinal);

            return isCurrentPlayer;
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

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MainMenu;
        }

        private void RetryButtonOnClick(object sender, MyraEventArgs e)
        {
            if (_rankingChannel is ICommunicationObject failedChannel)
            {
                failedChannel.Abort();
            }

            _rankingRequest = null;
            _rankingEntries = null;
            _failureNotice.Visible = false;
            _loadingLabel.Visible = true;
        }
    }
}
