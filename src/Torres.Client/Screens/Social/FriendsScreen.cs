using System;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Avatars;
using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class FriendsScreen : Screen
    {
        private readonly FriendsFlow _friendsFlow;
        private readonly VerticalStackPanel _tabContentPlaceholder = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
        private readonly LabeledTextBox _searchField = new LabeledTextBox(TextKeys.Friends.SearchPlaceholder, false, AccountLimits.LongestUsername);
        private readonly LocalizedButton _searchButton = ButtonFactory.CreatePrimaryButton(TextKeys.Friends.SearchButton);
        private readonly LocalizedLabel _errorLabel = LabelFactory.CreateErrorMessage(TextKeys.Common.ServerErrorTitle);
        private readonly AvatarViewSet _avatarViews;

        private FriendsTab _activeTab = FriendsTab.Friends;

        internal FriendsScreen(FriendsFlow friendsFlow, AvatarCache avatarCache)
            : base(TextKeys.Friends.HeaderLabel, true)
        {
            _friendsFlow = friendsFlow;
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
            _activeTab = FriendsTab.Friends;
            _errorLabel.Visible = false;
            HideConfirmation();
            bool canOpen = _friendsFlow.Open();
            RenderTab();
            if (!canOpen)
            {
                RequestedScreen = ScreenId.Login;
            }
        }

        internal override void Update(GameTime gameTime)
        {
            _friendsFlow.Advance(gameTime.ElapsedGameTime);
            FriendsOutcome? outcome = _friendsFlow.TakeOutcome();
            if (outcome is not null)
            {
                Show(outcome);
            }

            _avatarViews.Refresh();
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = PanelFactory.CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(BuildSearchRow());
            page.Widgets.Add(_errorLabel);
            page.Widgets.Add(ConfirmationPlaceholder);
            page.Widgets.Add(_tabContentPlaceholder);
            StackPanel.SetProportionType(_tabContentPlaceholder, ProportionType.Fill);
            RenderTab();

            LocalizedButton backButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(PanelFactory.CreateBackBar(backButton));

            return page;
        }

        private void Show(FriendsOutcome outcome)
        {
            if (outcome.HasFailed)
            {
                _errorLabel.TextKey = TextKeys.Common.ServerErrorTitle;
                _errorLabel.Visible = true;
            }

            if (outcome.ShowsSentRequests)
            {
                _searchField.Box.Text = string.Empty;
                _activeTab = FriendsTab.Sent;
            }

            if (outcome.NeedsRender || outcome.ShowsSentRequests)
            {
                RenderTab();
            }
        }

        private void RemoveFriend(int friendPlayerId)
        {
            HideConfirmation();
            _friendsFlow.RemoveFriend(friendPlayerId);
        }

        private void AskRemovalConfirmation(PlayerSummary friend)
        {
            _errorLabel.Visible = false;
            ShowConfirmation(TextKeys.Alerts.RemoveFriendTitle, TextKeys.Alerts.RemoveFriendBody);

            LocalizedButton removeConfirmButton = ButtonFactory.CreateDestructiveButton(TextKeys.Friends.RemoveFriendButton);
            removeConfirmButton.Click += (sender, e) => RemoveFriend(friend.PlayerId);
            LocalizedButton cancelButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.CancelButton);
            cancelButton.Click += CancelButtonOnClick;
            AddConfirmationButtons(removeConfirmButton, cancelButton);
        }

        private HorizontalStackPanel BuildSearchRow()
        {
            HorizontalStackPanel searchRow = PanelFactory.CreateButtonRow();
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
            Widget tabContent = _friendsFlow.SearchResult is null ? BuildActiveTabContent() : BuildSearchResult(_friendsFlow.SearchResult);
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

            HorizontalStackPanel tabsRow = PanelFactory.CreateButtonRow();
            tabsRow.Widgets.Add(friendsTabButton);
            tabsRow.Widgets.Add(receivedTabButton);
            tabsRow.Widgets.Add(sentTabButton);

            return tabsRow;
        }

        private LocalizedButton BuildTabButton(string textKey, FriendsTab tab)
        {
            bool isActiveTab = (_friendsFlow.SearchResult is null) && (tab == _activeTab);
            LocalizedButton tabButton = isActiveTab ? ButtonFactory.CreatePrimaryButton(textKey) : ButtonFactory.CreateSecondaryButton(textKey);

            return tabButton;
        }

        private Widget BuildActiveTabContent()
        {
            Widget activeTabContent = _activeTab switch
            {
                FriendsTab.Received => BuildReceivedList(),
                FriendsTab.Sent => BuildSentList(),
                FriendsTab.Friends => BuildFriendsList(),
                _ => throw new ArgumentOutOfRangeException(nameof(_activeTab)),
            };

            return activeTabContent;
        }

        private VerticalStackPanel BuildFriendsList()
        {
            if (_friendsFlow.FriendshipList.Friends.Count == 0)
            {
                VerticalStackPanel noFriendsState = PanelFactory.CreateEmptyState(TextKeys.Friends.EmptyStateTitle, TextKeys.Friends.EmptyStateHint);
                return noFriendsState;
            }

            var friendsList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (PlayerSummary friend in _friendsFlow.FriendshipList.Friends)
            {
                HorizontalStackPanel itemRow = PanelFactory.CreateButtonRow();
                itemRow.Widgets.Add(_avatarViews.Create(friend.AvatarReference).Root);

                var nameColumn = new VerticalStackPanel { VerticalAlignment = VerticalAlignment.Center };
                nameColumn.Widgets.Add(LabelFactory.CreateUsernameLabel(friend.Username));
                nameColumn.Widgets.Add(PanelFactory.CreateConnectionStatusRow(friend.IsOnline));
                itemRow.Widgets.Add(nameColumn);

                var spacer = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
                StackPanel.SetProportionType(spacer, ProportionType.Part);
                itemRow.Widgets.Add(spacer);

                LocalizedButton removeButton = ButtonFactory.CreateDestructiveButton(TextKeys.Friends.RemoveFriendButton);
                removeButton.Click += (sender, e) => AskRemovalConfirmation(friend);
                itemRow.Widgets.Add(removeButton);

                friendsList.Widgets.Add(PanelFactory.CreateItemCard(itemRow));
            }

            return friendsList;
        }

        private VerticalStackPanel BuildReceivedList()
        {
            var receivedSection = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
            receivedSection.Widgets.Add(LabelFactory.CreateTitle(TextKeys.Friends.RequestsTitle));
            if (_friendsFlow.FriendshipList.ReceivedRequests.Count == 0)
            {
                receivedSection.Widgets.Add(PanelFactory.CreateEmptyState(TextKeys.Friends.NoReceivedTitle, TextKeys.Friends.NoReceivedHint));
                return receivedSection;
            }

            var requestsList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (PlayerSummary requester in _friendsFlow.FriendshipList.ReceivedRequests)
            {
                requestsList.Widgets.Add(BuildReceivedRequestCard(requester));
            }

            receivedSection.Widgets.Add(requestsList);

            return receivedSection;
        }

        private Panel BuildReceivedRequestCard(PlayerSummary requester)
        {
            HorizontalStackPanel itemRow = PanelFactory.CreateButtonRow();
            itemRow.HorizontalAlignment = HorizontalAlignment.Stretch;
            itemRow.Widgets.Add(_avatarViews.Create(requester.AvatarReference).Root);

            var nameColumn = new VerticalStackPanel();
            nameColumn.Widgets.Add(LabelFactory.CreateUsernameLabel(requester.Username));
            nameColumn.Widgets.Add(LabelFactory.CreateHint(TextKeys.Friends.FriendRequestMessage));
            itemRow.Widgets.Add(nameColumn);

            var spacer = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
            StackPanel.SetProportionType(spacer, ProportionType.Part);
            itemRow.Widgets.Add(spacer);

            LocalizedButton acceptButton = ButtonFactory.CreatePrimaryButton(TextKeys.Friends.AcceptButton);
            acceptButton.Click += (sender, e) => _friendsFlow.AcceptFriendRequest(requester.PlayerId);
            itemRow.Widgets.Add(acceptButton);

            LocalizedButton rejectButton = ButtonFactory.CreateSecondaryButton(TextKeys.Friends.RejectButton);
            rejectButton.Click += (sender, e) => _friendsFlow.RejectFriendRequest(requester.PlayerId);
            itemRow.Widgets.Add(rejectButton);

            Panel requestCard = PanelFactory.CreateItemCard(itemRow);

            return requestCard;
        }

        private VerticalStackPanel BuildSentList()
        {
            if (_friendsFlow.FriendshipList.SentRequests.Count == 0)
            {
                VerticalStackPanel noSentState = PanelFactory.CreateEmptyState(TextKeys.Friends.NoSentTitle, TextKeys.Friends.NoSentHint);
                return noSentState;
            }

            var sentList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (PlayerSummary addressee in _friendsFlow.FriendshipList.SentRequests)
            {
                HorizontalStackPanel itemRow = PanelFactory.CreateButtonRow();
                itemRow.Widgets.Add(_avatarViews.Create(addressee.AvatarReference).Root);

                var nameColumn = new VerticalStackPanel();
                nameColumn.Widgets.Add(LabelFactory.CreateUsernameLabel(addressee.Username));
                nameColumn.Widgets.Add(LabelFactory.CreateHint(TextKeys.Friends.AwaitingResponseMessage));
                itemRow.Widgets.Add(nameColumn);
                itemRow.Widgets.Add(PanelFactory.CreateStatusBadge(TextKeys.Friends.PendingStatus));

                sentList.Widgets.Add(PanelFactory.CreateItemCard(itemRow));
            }

            return sentList;
        }

        private VerticalStackPanel BuildSearchResult(PlayerSearchResponse searchResult)
        {
            if ((searchResult.Status != PlayerSearchStatus.Found) || (searchResult.Player is null))
            {
                VerticalStackPanel noResultsState = PanelFactory.CreateEmptyState(TextKeys.Friends.NoResultsTitle, TextKeys.Friends.NoResultsHint);
                return noResultsState;
            }

            HorizontalStackPanel itemRow = PanelFactory.CreateButtonRow();
            itemRow.Widgets.Add(_avatarViews.Create(searchResult.Player.AvatarReference).Root);
            itemRow.Widgets.Add(LabelFactory.CreateUsernameLabel(searchResult.Player.Username));
            itemRow.Widgets.Add(BuildRelationshipWidget(searchResult.Player, searchResult.Relationship));

            var searchResults = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
            searchResults.Widgets.Add(PanelFactory.CreateItemCard(itemRow));
            searchResults.Widgets.Add(LabelFactory.CreateHint(TextKeys.Friends.SelfExcludedHint));

            return searchResults;
        }

        private Widget BuildRelationshipWidget(PlayerSummary foundPlayer, FriendRelationship relationship)
        {
            Widget relationshipWidget = relationship switch
            {
                FriendRelationship.None => BuildSendRequestButton(foundPlayer),
                FriendRelationship.RequestSent => PanelFactory.CreateStatusBadge(TextKeys.Friends.RequestSentStatus),
                FriendRelationship.RequestReceived => PanelFactory.CreateStatusBadge(TextKeys.Friends.RequestReceivedStatus),
                FriendRelationship.Friend => PanelFactory.CreateStatusBadge(TextKeys.Friends.AlreadyFriendsStatus),
                _ => throw new ArgumentOutOfRangeException(nameof(relationship), relationship, null),
            };

            return relationshipWidget;
        }

        private LocalizedButton BuildSendRequestButton(PlayerSummary foundPlayer)
        {
            LocalizedButton sendRequestButton = ButtonFactory.CreatePrimaryButton(TextKeys.Friends.SendRequestButton);
            sendRequestButton.Click += (sender, e) => _friendsFlow.SendFriendRequest(foundPlayer.PlayerId);

            return sendRequestButton;
        }

        private void SearchButtonOnClick(object sender, MyraEventArgs e)
        {
            _errorLabel.Visible = false;
            string? errorKey = _friendsFlow.Search(_searchField.Value);
            if (errorKey is not null)
            {
                _errorLabel.TextKey = errorKey;
                _errorLabel.Visible = true;
            }
        }

        private void SelectTab(FriendsTab tab)
        {
            HideConfirmation();
            _activeTab = tab;
            _friendsFlow.ClearSearch();
            RenderTab();
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
            _friendsFlow.ForgetRequests();
            RequestedScreen = FriendsFlow.FindScreenToGoBack();
        }
    }
}
