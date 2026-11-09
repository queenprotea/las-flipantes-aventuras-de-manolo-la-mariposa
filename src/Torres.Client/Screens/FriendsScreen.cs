using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Avatars;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class FriendsScreen : Screen
    {
        private const double FriendshipListRefreshSeconds = 5;

        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly VerticalStackPanel _tabContentPlaceholder = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
        private readonly LabeledTextBox _searchField = new LabeledTextBox(TextKeys.Friends.SearchPlaceholder, false);
        private readonly LocalizedButton _searchButton = CreatePrimaryButton(TextKeys.Friends.SearchButton);
        private readonly LocalizedLabel _errorLabel = CreateErrorMessage(TextKeys.Common.ServerErrorTitle);
        private readonly AvatarViewSet _avatarViews;

        private FriendsTab _activeTab = FriendsTab.Friends;
        private Task<FriendshipListResponse>? _friendshipListRequest;
        private Task<PlayerSearchResponse>? _searchRequest;
        private Task<FriendRequestResult>? _friendRequest;
        private Task<FriendRequestResponseResult>? _responseRequest;
        private Task<FriendRemovalResult>? _removalRequest;
        private FriendshipListResponse _friendshipList = new FriendshipListResponse();
        private PlayerSearchResponse? _searchResult;
        private string _searchedUsername = string.Empty;
        private double _secondsSinceFriendshipList;

        internal FriendsScreen(GameSessionConnection gameSessionConnection, PlayerSession playerSession, AvatarCache avatarCache)
            : base(TextKeys.Friends.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(playerSession);
            ArgumentNullException.ThrowIfNull(avatarCache);

            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _avatarViews = new AvatarViewSet(avatarCache, Sizes.SmallAvatarSize);
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
            _friendshipList = new FriendshipListResponse();
            _searchResult = null;
            _errorLabel.Visible = false;
            HideConfirmation();
            RenderTab();

            if (!_playerSession.HasAccount || !_gameSessionConnection.IsSessionOpen)
            {
                RequestedScreen = ScreenId.Login;
                return;
            }

            StartFriendshipListRequest();
        }

        internal override void Update(GameTime gameTime)
        {
            RefreshFriendshipListPeriodically(gameTime);
            FollowFriendshipListRequest();
            FollowSearch();
            FollowFriendRequest();
            FollowResponse();
            FollowRemoval();
            _avatarViews.Refresh();
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(BuildSearchRow());
            page.Widgets.Add(_errorLabel);
            page.Widgets.Add(ConfirmationPlaceholder);
            page.Widgets.Add(_tabContentPlaceholder);
            StackPanel.SetProportionType(_tabContentPlaceholder, ProportionType.Fill);
            RenderTab();

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(CreateBackBar(backButton));

            return page;
        }

        private void RefreshFriendshipListPeriodically(GameTime gameTime)
        {
            _secondsSinceFriendshipList += gameTime.ElapsedGameTime.TotalSeconds;
            bool isRefreshDue = (_secondsSinceFriendshipList >= FriendshipListRefreshSeconds) && (_friendshipListRequest is null);
            if (isRefreshDue)
            {
                StartFriendshipListRequest();
            }
        }

        private void FollowFriendshipListRequest()
        {
            if ((_friendshipListRequest is null) || !_friendshipListRequest.IsCompleted)
            {
                return;
            }

            Task<FriendshipListResponse> completedFriendshipList = _friendshipListRequest;
            _friendshipListRequest = null;
            if (completedFriendshipList.IsCompletedSuccessfully)
            {
                FriendshipListResponse receivedFriendshipList = completedFriendshipList.Result;
                bool hasChanged = !ShowsSameRows(_friendshipList, receivedFriendshipList);
                _friendshipList = receivedFriendshipList;
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

        private void FollowResponse()
        {
            if ((_responseRequest is null) || !_responseRequest.IsCompleted)
            {
                return;
            }

            Task<FriendRequestResponseResult> completedResponse = _responseRequest;
            _responseRequest = null;
            bool isAnswered = completedResponse.IsCompletedSuccessfully
                && (completedResponse.Result != FriendRequestResponseResult.DatabaseUnavailable);
            if (isAnswered)
            {
                StartFriendshipListRequest();
            }
            else
            {
                ShowFailure();
            }
        }

        private void FollowRemoval()
        {
            if ((_removalRequest is null) || !_removalRequest.IsCompleted)
            {
                return;
            }

            Task<FriendRemovalResult> completedRemoval = _removalRequest;
            _removalRequest = null;
            bool isRemoved = completedRemoval.IsCompletedSuccessfully
                && (completedRemoval.Result != FriendRemovalResult.DatabaseUnavailable);

            if (isRemoved)
            {
                StartFriendshipListRequest();
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
                    StartFriendshipListRequest();
                    break;

                case FriendRequestResult.AlreadyFriends:
                case FriendRequestResult.RequestAlreadyPending:
                    StartSearchRequest(_searchedUsername);
                    StartFriendshipListRequest();
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

        private static bool ShowsSameRows(FriendshipListResponse shownFriendshipList, FriendshipListResponse receivedFriendshipList)
        {
            bool showsSameRows = ShowsSameRows(shownFriendshipList.Friends, receivedFriendshipList.Friends)
                && ShowsSameRows(shownFriendshipList.ReceivedRequests, receivedFriendshipList.ReceivedRequests)
                && ShowsSameRows(shownFriendshipList.SentRequests, receivedFriendshipList.SentRequests);

            return showsSameRows;
        }

        private static bool ShowsSameRows(IEnumerable<PlayerSummary> shownPlayers, IEnumerable<PlayerSummary> receivedPlayers)
        {
            bool hasSamePlayers = shownPlayers.Select(player => player.PlayerId)
                .SequenceEqual(receivedPlayers.Select(player => player.PlayerId));
            bool hasSameConnectionStatus = shownPlayers.Select(player => player.IsOnline)
                .SequenceEqual(receivedPlayers.Select(player => player.IsOnline));
            bool showsSameRows = hasSamePlayers && hasSameConnectionStatus;

            return showsSameRows;
        }

        private void StartFriendshipListRequest()
        {
            _secondsSinceFriendshipList = 0;
            if (_gameSessionConnection.IsSessionOpen)
            {
                _friendshipListRequest = _gameSessionConnection.GetFriendshipListAsync();
            }
        }

        private void StartSearchRequest(string username)
        {
            if (_gameSessionConnection.IsSessionOpen)
            {
                _searchedUsername = username;
                _searchRequest = _gameSessionConnection.FindPlayerByUsernameAsync(username);
            }
        }

        private void SendFriendRequest(int targetPlayerId)
        {
            bool canSend = _gameSessionConnection.IsSessionOpen && (_friendRequest is null);
            if (canSend)
            {
                _friendRequest = _gameSessionConnection.SendFriendRequestAsync(targetPlayerId);
            }
        }

        private void AcceptFriendRequest(int requesterPlayerId)
        {
            bool canAccept = _gameSessionConnection.IsSessionOpen && (_responseRequest is null);
            if (canAccept)
            {
                _responseRequest = _gameSessionConnection.AcceptFriendRequestAsync(requesterPlayerId);
            }
        }

        private void RejectFriendRequest(int requesterPlayerId)
        {
            bool canReject = _gameSessionConnection.IsSessionOpen && (_responseRequest is null);
            if (canReject)
            {
                _responseRequest = _gameSessionConnection.RejectFriendRequestAsync(requesterPlayerId);
            }
        }

        private void RemoveFriend(int friendPlayerId)
        {
            HideConfirmation();
            bool canRemove = _gameSessionConnection.IsSessionOpen && (_removalRequest is null);
            if (canRemove)
            {
                _removalRequest = _gameSessionConnection.RemoveFriendAsync(friendPlayerId);
            }
        }

        private void AskRemovalConfirmation(PlayerSummary friend)
        {
            _errorLabel.Visible = false;
            ShowConfirmation(TextKeys.Alerts.RemoveFriendTitle, TextKeys.Alerts.RemoveFriendBody);

            LocalizedButton removeConfirmButton = CreateDestructiveButton(TextKeys.Friends.RemoveFriendButton);
            removeConfirmButton.Click += (sender, e) => RemoveFriend(friend.PlayerId);
            LocalizedButton cancelButton = CreateSecondaryButton(TextKeys.Common.CancelButton);
            cancelButton.Click += CancelButtonOnClick;
            AddConfirmationButtons(removeConfirmButton, cancelButton);
        }

        private void ShowFailure()
        {
            ForgetRequests();
            _errorLabel.TextKey = TextKeys.Common.ServerErrorTitle;
            _errorLabel.Visible = true;
        }

        private void ForgetRequests()
        {
            _friendshipListRequest = null;
            _searchRequest = null;
            _friendRequest = null;
            _responseRequest = null;
            _removalRequest = null;
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
            _avatarViews.Clear();
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
            if (_friendshipList.Friends.Count == 0)
            {
                VerticalStackPanel noFriendsState = CreateEmptyState(TextKeys.Friends.EmptyStateTitle, TextKeys.Friends.EmptyStateHint);
                return noFriendsState;
            }

            var friendsList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (PlayerSummary friend in _friendshipList.Friends)
            {
                HorizontalStackPanel itemRow = CreateButtonRow();
                itemRow.Widgets.Add(_avatarViews.Create(friend.AvatarReference).Root);

                var nameColumn = new VerticalStackPanel { VerticalAlignment = VerticalAlignment.Center };
                nameColumn.Widgets.Add(CreateUsernameLabel(friend.Username));
                nameColumn.Widgets.Add(BuildConnectionStatusRow(friend.IsOnline));
                itemRow.Widgets.Add(nameColumn);

                var spacer = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
                StackPanel.SetProportionType(spacer, ProportionType.Part);
                itemRow.Widgets.Add(spacer);

                LocalizedButton removeButton = CreateDestructiveButton(TextKeys.Friends.RemoveFriendButton);
                removeButton.Click += (sender, e) => AskRemovalConfirmation(friend);
                itemRow.Widgets.Add(removeButton);

                friendsList.Widgets.Add(CreateItemCard(itemRow));
            }

            return friendsList;
        }

        private static HorizontalStackPanel BuildConnectionStatusRow(bool isOnline)
        {
            Color connectionDotColor = isOnline ? Theme.MintLine : Theme.Blush;
            string connectionTextKey = isOnline ? TextKeys.Friends.OnlineStatus : TextKeys.Friends.OfflineStatus;

            var connectionDot = new Label
            {
                Text = DisplaySymbols.ConnectionDot,
                Font = Fonts.Small,
                TextColor = connectionDotColor,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var connectionLabel = new LocalizedLabel(connectionTextKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var connectionStatusRow = new HorizontalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            connectionStatusRow.Widgets.Add(connectionDot);
            connectionStatusRow.Widgets.Add(connectionLabel);

            return connectionStatusRow;
        }

        private Widget BuildReceivedList()
        {
            var receivedSection = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
            receivedSection.Widgets.Add(CreateTitle(TextKeys.Friends.RequestsTitle));
            if (_friendshipList.ReceivedRequests.Count == 0)
            {
                receivedSection.Widgets.Add(CreateEmptyState(TextKeys.Friends.NoReceivedTitle, TextKeys.Friends.NoReceivedHint));
                return receivedSection;
            }

            var requestsList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (PlayerSummary requester in _friendshipList.ReceivedRequests)
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
            itemRow.Widgets.Add(_avatarViews.Create(requester.AvatarReference).Root);

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
            rejectButton.Click += (sender, e) => RejectFriendRequest(requester.PlayerId);
            itemRow.Widgets.Add(rejectButton);

            Panel requestCard = CreateItemCard(itemRow);

            return requestCard;
        }

        private VerticalStackPanel BuildSentList()
        {
            if (_friendshipList.SentRequests.Count == 0)
            {
                VerticalStackPanel noSentState = CreateEmptyState(TextKeys.Friends.NoSentTitle, TextKeys.Friends.NoSentHint);
                return noSentState;
            }

            var sentList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (PlayerSummary addressee in _friendshipList.SentRequests)
            {
                HorizontalStackPanel itemRow = CreateButtonRow();
                itemRow.Widgets.Add(_avatarViews.Create(addressee.AvatarReference).Root);

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
            itemRow.Widgets.Add(_avatarViews.Create(searchResult.Player.AvatarReference).Root);
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

            bool isOwnUsername = string.Equals(username, _playerSession.Player?.Username, StringComparison.OrdinalIgnoreCase);
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
            HideConfirmation();
            _activeTab = tab;
            _searchResult = null;
            RenderTab();
            if (_friendshipListRequest is null)
            {
                StartFriendshipListRequest();
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

        private void CancelButtonOnClick(object sender, MyraEventArgs e)
        {
            HideConfirmation();
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            HideConfirmation();
            ForgetRequests();
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
