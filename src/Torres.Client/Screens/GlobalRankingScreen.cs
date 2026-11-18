using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

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

        private readonly ChannelFactory<IRankingService> _rankingChannelFactory;
        private readonly PlayerSession _playerSession;
        private readonly VerticalStackPanel _contentPlaceholder = new VerticalStackPanel { Visible = false };
        private readonly LocalizedLabel _loadingLabel = CreateHint(TextKeys.GlobalRanking.LoadingLabel);
        private readonly VerticalStackPanel _failureNotice;

        private IRankingService? _rankingChannel;
        private Task<GlobalRankingResponse>? _rankingRequest;
        private List<RankingEntry>? _rankingEntries;

        internal GlobalRankingScreen(ChannelFactory<IRankingService> rankingChannelFactory, PlayerSession playerSession)
            : base(TextKeys.GlobalRanking.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(rankingChannelFactory);
            ArgumentNullException.ThrowIfNull(playerSession);

            _rankingChannelFactory = rankingChannelFactory;
            _playerSession = playerSession;
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
                var rankingTable = new RankingTableView(Sizes.RankingTableWidth, Sizes.RankingRankColumnWidth, _statColumns, ToRankingRows(entries));
                _contentPlaceholder.Widgets.Add(rankingTable.Root);
            }

            _contentPlaceholder.Visible = true;
        }

        private VerticalStackPanel BuildFailureNotice()
        {
            LocalizedButton retryButton = CreatePrimaryButton(TextKeys.Common.RetryButton);
            retryButton.Click += RetryButtonOnClick;

            VerticalStackPanel failureNotice = CreateFailureNotice(retryButton);

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
            var rankingRow = new RankingRow(rank, entry.PlayerName, IsCurrentPlayer(entry), noSeatColor, new int[] { entry.Wins, entry.Points, entry.Matches });

            return rankingRow;
        }

        private bool IsCurrentPlayer(RankingEntry entry)
        {
            bool isCurrentPlayer = (_playerSession.Player is PlayerIdentity player)
                && string.Equals(player.Username, entry.PlayerName, StringComparison.Ordinal);

            return isCurrentPlayer;
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
