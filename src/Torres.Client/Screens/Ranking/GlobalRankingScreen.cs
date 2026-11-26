using System.Collections.Generic;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class GlobalRankingScreen : Screen
    {
        private static readonly IReadOnlyList<RankingColumn> _statColumns = new RankingColumn[]
        {
            new RankingColumn(TextKeys.GlobalRanking.WinsColumn, Sizes.RankingStatColumnWidth),
            new RankingColumn(TextKeys.GlobalRanking.PointsColumn, Sizes.RankingStatColumnWidth),
            new RankingColumn(TextKeys.GlobalRanking.MatchesColumn, Sizes.RankingStatColumnWidth),
        };

        private readonly GlobalRankingFlow _rankingFlow;
        private readonly VerticalStackPanel _contentPlaceholder = new VerticalStackPanel { Visible = false };
        private readonly LocalizedLabel _loadingLabel = LabelFactory.CreateHint(TextKeys.GlobalRanking.LoadingLabel);
        private readonly VerticalStackPanel _failureNotice;


        internal GlobalRankingScreen(GlobalRankingFlow rankingFlow)
            : base(TextKeys.GlobalRanking.HeaderLabel, true)
        {
            _rankingFlow = rankingFlow;
            _failureNotice = BuildFailureNotice();
        }

        internal override void Update(GameTime gameTime)
        {
            GlobalRankingOutcome? outcome = _rankingFlow.TakeOutcome();
            if (outcome is null)
            {
                return;
            }

            _loadingLabel.Visible = false;
            if (outcome.Entries is List<RankingEntry> entries)
            {
                RenderEntries(entries);
            }
            else
            {
                _failureNotice.Visible = true;
            }
        }

        protected override Widget Build()
        {
            VerticalStackPanel pagePanel = PanelFactory.CreatePage();
            pagePanel.Widgets.Add(_loadingLabel);
            pagePanel.Widgets.Add(_contentPlaceholder);
            StackPanel.SetProportionType(_contentPlaceholder, ProportionType.Fill);
            pagePanel.Widgets.Add(_failureNotice);

            LocalizedButton backButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            pagePanel.Widgets.Add(PanelFactory.CreateBackBar(backButton));

            return pagePanel;
        }

        private void RenderEntries(List<RankingEntry> entries)
        {
            _contentPlaceholder.Widgets.Clear();

            if (entries.Count == 0)
            {
                _contentPlaceholder.Widgets.Add(PanelFactory.CreateEmptyState(TextKeys.GlobalRanking.EmptyStateTitle, TextKeys.GlobalRanking.EmptyStateHint));
            }
            else
            {
                var rankingTable = new RankingTableView(Sizes.RankingTableWidth, Sizes.RankingRankColumnWidth, _statColumns, ToRankingRows(entries));
                _contentPlaceholder.Widgets.Add(rankingTable.Root);
            }

            _contentPlaceholder.Visible = true;
        }

        private VerticalStackPanel BuildFailureNotice()
        {
            LocalizedButton retryButton = ButtonFactory.CreatePrimaryButton(TextKeys.Common.RetryButton);
            retryButton.Click += RetryButtonOnClick;

            VerticalStackPanel failureNotice = PanelFactory.CreateFailureNotice(retryButton);

            return failureNotice;
        }

        private List<RankingRow> ToRankingRows(List<RankingEntry> entries)
        {
            var rankingRows = new List<RankingRow>();
            for (var i = 0; i < entries.Count; i++)
            {
                rankingRows.Add(ToRankingRow(entries[i], i + 1));
            }

            return rankingRows;
        }

        private RankingRow ToRankingRow(RankingEntry entry, int rank)
        {
            SeatColor? noSeatColor = null;
            var rankingRow = new RankingRow(rank, entry.PlayerName, _rankingFlow.IsCurrentPlayer(entry.PlayerName), noSeatColor, new int[] { entry.Wins, entry.Points, entry.Matches });

            return rankingRow;
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MainMenu;
        }

        private void RetryButtonOnClick(object sender, MyraEventArgs e)
        {
            _rankingFlow.Retry();
            _failureNotice.Visible = false;
            _loadingLabel.Visible = true;
        }
    }
}
