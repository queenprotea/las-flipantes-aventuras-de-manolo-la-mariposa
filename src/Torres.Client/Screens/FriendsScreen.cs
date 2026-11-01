using System;
using System.Collections.Generic;
using System.Linq;
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
    internal sealed class FriendsScreen : Screen
    {
        private const double OverviewRefreshSeconds = 5;

        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _session;
        private readonly VerticalStackPanel _tabContentPlaceholder = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
        private readonly LabeledTextBox _searchField = new LabeledTextBox(TextKeys.Friends.SearchPlaceholder, false);
        private readonly LocalizedButton _searchButton = CreatePrimaryButton(TextKeys.Friends.SearchButton);
        private readonly LocalizedLabel _errorLabel = CreateErrorMessage(TextKeys.Common.ServerErrorTitle);

        private FriendsTab _activeTab = FriendsTab.Friends;
        private Task<FriendsOverviewResponse>? _overviewRequest;
        private Task<PlayerSearchResponse>? _searchRequest;
        private Task<FriendRequestResult>? _friendRequest;
        private Task<FriendRequestResponseResult>? _acceptRequest;
        private FriendsOverviewResponse _overview = new FriendsOverviewResponse();
        private PlayerSearchResponse? _searchResult;
        private string _searchedUsername = string.Empty;
        private double _secondsSinceOverview;

        internal FriendsScreen(GameSessionConnection gameSessionConnection, PlayerSession session)
            : base(TextKeys.Friends.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(session);

            _gameSessionConnection = gameSessionConnection;
            _session = session;
        }

        private enum FriendsTab
        {
            Friends,
            Received,
            Sent,
        }

        internal override void Open()
        {
            ForgetRequests();
            _activeTab = FriendsTab.Friends;
            _overview = new FriendsOverviewResponse();
            _searchResult = null;
            _errorLabel.Visible = false;
            RenderTab();

            if (!_session.HasAccount || !_gameSessionConnection.IsOpen)
            {
                RequestedScreen = ScreenId.Login;
                return;
            }

            StartOverviewRequest();
        }

        internal override void Update(GameTime gameTime)
        {
            RefreshOverviewPeriodically(gameTime);
            FollowOverview();
            FollowSearch();
            FollowFriendRequest();
            FollowAccept();
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(BuildSearchRow());
            page.Widgets.Add(_errorLabel);
            page.Widgets.Add(_tabContentPlaceholder);
            StackPanel.SetProportionType(_tabContentPlaceholder, ProportionType.Fill);
            RenderTab();

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(CreateBackBar(backButton));

            return page;
        }

        private void RefreshOverviewPeriodically(GameTime gameTime)
        {
            _secondsSinceOverview += gameTime.ElapsedGameTime.TotalSeconds;
            bool isRefreshDue = (_secondsSinceOverview >= OverviewRefreshSeconds) && (_overviewRequest is null);
            if (isRefreshDue)
            {
                StartOverviewRequest();
            }
        }

        private void FollowOverview()
        {
            if ((_overviewRequest is null) || !_overviewRequest.IsCompleted)
            {
                return;
            }

            Task<FriendsOverviewResponse> completedOverview = _overviewRequest;
            _overviewRequest = null;
            if (completedOverview.IsCompletedSuccessfully)
            {
                FriendsOverviewResponse receivedOverview = completedOverview.Result;
                bool hasChanged = !HasSamePlayers(_overview, receivedOverview);
                _overview = receivedOverview;
                if (hasChanged)
                {
                    RenderTab();
                }
            }
            else
            {
                ShowFailure();
            }
        }

        private void FollowSearch()
        {
            if ((_searchRequest is null) || !_searchRequest.IsCompleted)
            {
                return;
            }

            Task<PlayerSearchResponse> completedSearch = _searchRequest;
            _searchRequest = null;
            if (completedSearch.IsCompletedSuccessfully)
            {
                _searchResult = completedSearch.Result;
                RenderTab();
            }
            else
            {
                ShowFailure();
            }
        }

        private void FollowFriendRequest()
        {
            if ((_friendRequest is null) || !_friendRequest.IsCompleted)
            {
                return;
            }

            Task<FriendRequestResult> completedRequest = _friendRequest;
            _friendRequest = null;
            if (completedRequest.IsCompletedSuccessfully)
            {
                ShowFriendRequestOutcome(completedRequest.Result);
            }
            else
            {
                ShowFailure();
            }
        }

        private void FollowAccept()
        {
            if ((_acceptRequest is null) || !_acceptRequest.IsCompleted)
            {
                return;
            }

            Task<FriendRequestResponseResult> completedAcceptance = _acceptRequest;
            _acceptRequest = null;
            bool isAnswered = completedAcceptance.IsCompletedSuccessfully
                && (completedAcceptance.Result != FriendRequestResponseResult.DatabaseUnavailable);
            if (isAnswered)
            {
                StartOverviewRequest();
            }
            else
            {
                ShowFailure();
            }
        }

        private void ShowFriendRequestOutcome(FriendRequestResult requestResult)
        {
            switch (requestResult)
            {
                case FriendRequestResult.Sent:
                    _searchResult = null;
                    _searchField.Box.Text = string.Empty;
                    _activeTab = FriendsTab.Sent;
                    RenderTab();
                    StartOverviewRequest();
                    break;

                case FriendRequestResult.AlreadyFriends:
                case FriendRequestResult.RequestAlreadyPending:
                    StartSearchRequest(_searchedUsername);
                    StartOverviewRequest();
                    break;

                case FriendRequestResult.PlayerNotFound:
                    _searchResult = new PlayerSearchResponse { Status = PlayerSearchStatus.NotFound };
                    RenderTab();
                    break;

                case FriendRequestResult.DatabaseUnavailable:
                    ShowFailure();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(requestResult), requestResult, null);
            }
        }

        private static bool HasSamePlayers(FriendsOverviewResponse shownOverview, FriendsOverviewResponse receivedOverview)
        {
            bool hasSamePlayers = HasSamePlayers(shownOverview.Friends, receivedOverview.Friends)
                && HasSamePlayers(shownOverview.ReceivedRequests, receivedOverview.ReceivedRequests)
                && HasSamePlayers(shownOverview.SentRequests, receivedOverview.SentRequests);

            return hasSamePlayers;
        }

        private static bool HasSamePlayers(IEnumerable<PlayerSummary> shownPlayers, IEnumerable<PlayerSummary> receivedPlayers)
        {
            bool hasSamePlayers = shownPlayers.Select(player => player.PlayerId)
                .SequenceEqual(receivedPlayers.Select(player => player.PlayerId));

            return hasSamePlayers;
        }

        private void StartOverviewRequest()
        {
            _secondsSinceOverview = 0;
            if (_gameSessionConnection.IsOpen)
            {
                _overviewRequest = _gameSessionConnection.GetFriendsOverviewAsync();
            }
        }

        private void StartSearchRequest(string username)
        {
            if (_gameSessionConnection.IsOpen)
            {
                _searchedUsername = username;
                _searchRequest = _gameSessionConnection.FindPlayerByUsernameAsync(username);
            }
        }

        private void SendFriendRequest(int targetPlayerId)
        {
            bool canSend = _gameSessionConnection.IsOpen && (_friendRequest is null);
            if (canSend)
            {
                _friendRequest = _gameSessionConnection.SendFriendRequestAsync(targetPlayerId);
            }
        }

        private void AcceptFriendRequest(int requesterPlayerId)
        {
            bool canAccept = _gameSessionConnection.IsOpen && (_acceptRequest is null);
            if (canAccept)
            {
                _acceptRequest = _gameSessionConnection.AcceptFriendRequestAsync(requesterPlayerId);
            }
        }

        private void ShowFailure()
        {
            ForgetRequests();
            _errorLabel.TextKey = TextKeys.Common.ServerErrorTitle;
            _errorLabel.Visible = true;
        }

        private void ForgetRequests()
        {
            _overviewRequest = null;
            _searchRequest = null;
            _friendRequest = null;
            _acceptRequest = null;
        }

        private HorizontalStackPanel BuildSearchRow()
        {
            HorizontalStackPanel searchRow = CreateButtonRow();
            searchRow.Widgets.Add(_searchField);
            StackPanel.SetProportionType(_searchField, ProportionType.Fill);
            _searchButton.VerticalAlignment = VerticalAlignment.Bottom;
            _searchButton.Click += SearchButtonOnClick;
            searchRow.Widgets.Add(_searchButton);

            return searchRow;
        }

        private void RenderTab()
        {
            Widget tabContent = _searchResult is null ? BuildActiveTabContent() : BuildSearchResult(_searchResult);
            _tabContentPlaceholder.Widgets.Clear();
            _tabContentPlaceholder.Widgets.Add(BuildTabsRow());
            _tabContentPlaceholder.Widgets.Add(tabContent);
        }

        private HorizontalStackPanel BuildTabsRow()
        {
            LocalizedButton friendsTabButton = BuildTabButton(TextKeys.Friends.FriendsTab, FriendsTab.Friends);
            LocalizedButton receivedTabButton = BuildTabButton(TextKeys.Friends.ReceivedTab, FriendsTab.Received);
            LocalizedButton sentTabButton = BuildTabButton(TextKeys.Friends.SentTab, FriendsTab.Sent);
            friendsTabButton.Click += FriendsTabButtonOnClick;
            receivedTabButton.Click += ReceivedTabButtonOnClick;
            sentTabButton.Click += SentTabButtonOnClick;

            HorizontalStackPanel tabsRow = CreateButtonRow();
            tabsRow.Widgets.Add(friendsTabButton);
            tabsRow.Widgets.Add(receivedTabButton);
            tabsRow.Widgets.Add(sentTabButton);

            return tabsRow;
        }

        private LocalizedButton BuildTabButton(string textKey, FriendsTab tab)
        {
            bool isActiveTab = (_searchResult is null) && (tab == _activeTab);
            LocalizedButton tabButton = isActiveTab ? CreatePrimaryButton(textKey) : CreateSecondaryButton(textKey);

            return tabButton;
        }

        private Widget BuildActiveTabContent()
        {
            Widget activeTabContent = _activeTab switch
            {
                FriendsTab.Received => BuildReceivedList(),
                FriendsTab.Sent => BuildSentList(),
                _ => BuildFriendsList(),
            };

            return activeTabContent;
        }

        private VerticalStackPanel BuildFriendsList()
        {
            if (_overview.Friends.Count == 0)
            {
                VerticalStackPanel noFriendsState = CreateEmptyState(TextKeys.Friends.EmptyStateTitle, TextKeys.Friends.EmptyStateHint);
                return noFriendsState;
            }

            var friendsList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (PlayerSummary friend in _overview.Friends)
            {
                HorizontalStackPanel itemRow = CreateButtonRow();
                itemRow.Widgets.Add(CreateUsernameLabel(friend.Username));

                var spacer = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
                StackPanel.SetProportionType(spacer, ProportionType.Part);
                itemRow.Widgets.Add(spacer);

                LocalizedButton inviteToRoomButton = CreateSecondaryButton(TextKeys.Friends.InviteToRoomButton);
                itemRow.Widgets.Add(inviteToRoomButton);

                LocalizedButton removeButton = CreateDestructiveButton(TextKeys.Friends.RemoveFriendButton);
                itemRow.Widgets.Add(removeButton);

                friendsList.Widgets.Add(CreateItemCard(itemRow));
            }

            return friendsList;
        }

        private Widget BuildReceivedList()
        {
            var receivedSection = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
            receivedSection.Widgets.Add(CreateTitle(TextKeys.Friends.RequestsTitle));
            if (_overview.ReceivedRequests.Count == 0)
            {
                receivedSection.Widgets.Add(CreateEmptyState(TextKeys.Friends.NoReceivedTitle, TextKeys.Friends.NoReceivedHint));
                return receivedSection;
            }

            var requestsList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (PlayerSummary requester in _overview.ReceivedRequests)
            {
                requestsList.Widgets.Add(BuildReceivedRequestCard(requester));
            }

            receivedSection.Widgets.Add(requestsList);

            return receivedSection;
        }

        private Panel BuildReceivedRequestCard(PlayerSummary requester)
        {
            HorizontalStackPanel itemRow = CreateButtonRow();
            itemRow.HorizontalAlignment = HorizontalAlignment.Stretch;

            var nameColumn = new VerticalStackPanel();
            nameColumn.Widgets.Add(CreateUsernameLabel(requester.Username));
            nameColumn.Widgets.Add(CreateHint(TextKeys.Friends.FriendRequestMessage));
            itemRow.Widgets.Add(nameColumn);

            var spacer = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
            StackPanel.SetProportionType(spacer, ProportionType.Part);
            itemRow.Widgets.Add(spacer);

            LocalizedButton acceptButton = CreatePrimaryButton(TextKeys.Friends.AcceptButton);
            acceptButton.Click += (sender, e) => AcceptFriendRequest(requester.PlayerId);
            itemRow.Widgets.Add(acceptButton);

            LocalizedButton rejectButton = CreateSecondaryButton(TextKeys.Friends.RejectButton);
            itemRow.Widgets.Add(rejectButton);

            Panel requestCard = CreateItemCard(itemRow);

            return requestCard;
        }

        private VerticalStackPanel BuildSentList()
        {
            if (_overview.SentRequests.Count == 0)
            {
                VerticalStackPanel noSentState = CreateEmptyState(TextKeys.Friends.NoSentTitle, TextKeys.Friends.NoSentHint);
                return noSentState;
            }

            var sentList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (PlayerSummary addressee in _overview.SentRequests)
            {
                HorizontalStackPanel itemRow = CreateButtonRow();

                var nameColumn = new VerticalStackPanel();
                nameColumn.Widgets.Add(CreateUsernameLabel(addressee.Username));
                nameColumn.Widgets.Add(CreateHint(TextKeys.Friends.AwaitingResponseMessage));
                itemRow.Widgets.Add(nameColumn);
                itemRow.Widgets.Add(CreateStatusBadge(TextKeys.Friends.PendingStatus));

                sentList.Widgets.Add(CreateItemCard(itemRow));
            }

            return sentList;
        }

        private VerticalStackPanel BuildSearchResult(PlayerSearchResponse searchResult)
        {
            if ((searchResult.Status != PlayerSearchStatus.Found) || (searchResult.Player is null))
            {
                VerticalStackPanel noResultsState = CreateEmptyState(TextKeys.Friends.NoResultsTitle, TextKeys.Friends.NoResultsHint);
                return noResultsState;
            }

            HorizontalStackPanel itemRow = CreateButtonRow();
            itemRow.Widgets.Add(CreateUsernameLabel(searchResult.Player.Username));
            itemRow.Widgets.Add(BuildRelationshipWidget(searchResult.Player, searchResult.Relationship));

            var searchResults = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
            searchResults.Widgets.Add(CreateItemCard(itemRow));
            searchResults.Widgets.Add(CreateHint(TextKeys.Friends.SelfExcludedHint));

            return searchResults;
        }

        private Widget BuildRelationshipWidget(PlayerSummary foundPlayer, FriendRelationship relationship)
        {
            Widget relationshipWidget = relationship switch
            {
                FriendRelationship.None => BuildSendRequestButton(foundPlayer),
                FriendRelationship.RequestSent => CreateStatusBadge(TextKeys.Friends.RequestSentStatus),
                FriendRelationship.RequestReceived => CreateStatusBadge(TextKeys.Friends.RequestReceivedStatus),
                FriendRelationship.Friend => CreateStatusBadge(TextKeys.Friends.AlreadyFriendsStatus),
                _ => throw new ArgumentOutOfRangeException(nameof(relationship), relationship, null),
            };

            return relationshipWidget;
        }

        private LocalizedButton BuildSendRequestButton(PlayerSummary foundPlayer)
        {
            LocalizedButton sendRequestButton = CreatePrimaryButton(TextKeys.Friends.SendRequestButton);
            sendRequestButton.Click += (sender, e) => SendFriendRequest(foundPlayer.PlayerId);

            return sendRequestButton;
        }

        private void SearchButtonOnClick(object sender, MyraEventArgs e)
        {
            _errorLabel.Visible = false;
            string username = _searchField.Value.Trim();
            if (username.Length == 0)
            {
                _errorLabel.TextKey = TextKeys.Common.RequiredField;
                _errorLabel.Visible = true;
                return;
            }

            bool isOwnUsername = string.Equals(username, _session.Player?.Username, StringComparison.OrdinalIgnoreCase);
            if (isOwnUsername)
            {
                _searchResult = new PlayerSearchResponse { Status = PlayerSearchStatus.NotFound };
                RenderTab();
            }
            else if (_searchRequest is null)
            {
                StartSearchRequest(username);
            }
        }

        private void SelectTab(FriendsTab tab)
        {
            _activeTab = tab;
            _searchResult = null;
            RenderTab();
            if (_overviewRequest is null)
            {
                StartOverviewRequest();
            }
        }

        private void FriendsTabButtonOnClick(object sender, MyraEventArgs e)
        {
            SelectTab(FriendsTab.Friends);
        }

        private void ReceivedTabButtonOnClick(object sender, MyraEventArgs e)
        {
            SelectTab(FriendsTab.Received);
        }

        private void SentTabButtonOnClick(object sender, MyraEventArgs e)
        {
            SelectTab(FriendsTab.Sent);
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            ForgetRequests();
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
