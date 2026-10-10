using System;
using System.Collections.Generic;
using System.Linq;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class FriendsScreen : Screen
    {
        private readonly List<FriendEntry> _friends = CreateSampleFriends();
        private readonly List<RequestEntry> _receivedRequests = CreateSampleReceivedRequests();
        private readonly List<RequestEntry> _sentRequests = CreateSampleSentRequests();
        private readonly List<SearchResultEntry> _searchablePlayers = CreateSampleSearchResults();
        private readonly VerticalStackPanel _tabContentPlaceholder = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
        private readonly LabeledTextBox _searchField = new LabeledTextBox(TextKeys.Friends.SearchPlaceholder, false);

        private FriendsTab _activeTab = FriendsTab.Friends;
        private string _searchTerm = string.Empty;

        internal FriendsScreen()
            : base(TextKeys.Friends.HeaderLabel, true)
        {
        }

        private enum FriendsTab
        {
            Friends,
            Received,
            Sent,
        }

        private enum SearchStatus
        {
            New,
            Friend,
            Sent,
            Received,
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(BuildSearchRow());
            page.Widgets.Add(_tabContentPlaceholder);
            StackPanel.SetProportionType(_tabContentPlaceholder, ProportionType.Fill);
            RenderTab();

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(CreateBackBar(backButton));

            return page;
        }

        private HorizontalStackPanel BuildSearchRow()
        {
            HorizontalStackPanel searchRow = CreateButtonRow();
            _searchField.Box.TextChanged += SearchBoxOnTextChanged;
            searchRow.Widgets.Add(_searchField);
            StackPanel.SetProportionType(_searchField, ProportionType.Fill);

            return searchRow;
        }

        private void RenderTab()
        {
            Widget tabContent = string.IsNullOrWhiteSpace(_searchTerm) ? BuildActiveTabContent() : BuildSearchResults();
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
            LocalizedButton tabButton = tab == _activeTab ? CreatePrimaryButton(textKey) : CreateSecondaryButton(textKey);

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
            if (_friends.Count == 0)
            {
                VerticalStackPanel noFriendsState = CreateEmptyState(TextKeys.Friends.EmptyStateTitle, TextKeys.Friends.EmptyStateHint);
                return noFriendsState;
            }

            var friendsList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (FriendEntry friend in _friends)
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
            if (_receivedRequests.Count == 0)
            {
                receivedSection.Widgets.Add(CreateEmptyState(TextKeys.Friends.NoReceivedTitle, TextKeys.Friends.NoReceivedHint));
                return receivedSection;
            }

            var requestsList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (RequestEntry request in _receivedRequests)
            {
                HorizontalStackPanel itemRow = CreateButtonRow();
                itemRow.HorizontalAlignment = HorizontalAlignment.Stretch;

                var nameColumn = new VerticalStackPanel();
                nameColumn.Widgets.Add(CreateUsernameLabel(request.Username));
                nameColumn.Widgets.Add(CreateHint(TextKeys.Friends.FriendRequestMessage));
                itemRow.Widgets.Add(nameColumn);

                var spacer = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
                StackPanel.SetProportionType(spacer, ProportionType.Part);
                itemRow.Widgets.Add(spacer);

                LocalizedButton acceptButton = CreatePrimaryButton(TextKeys.Friends.AcceptButton);
                itemRow.Widgets.Add(acceptButton);

                LocalizedButton rejectButton = CreateSecondaryButton(TextKeys.Friends.RejectButton);
                itemRow.Widgets.Add(rejectButton);

                requestsList.Widgets.Add(CreateItemCard(itemRow));
            }

            receivedSection.Widgets.Add(requestsList);

            return receivedSection;
        }

        private VerticalStackPanel BuildSentList()
        {
            if (_sentRequests.Count == 0)
            {
                VerticalStackPanel noSentState = CreateEmptyState(TextKeys.Friends.NoSentTitle, TextKeys.Friends.NoSentHint);
                return noSentState;
            }

            var sentList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (RequestEntry request in _sentRequests)
            {
                HorizontalStackPanel itemRow = CreateButtonRow();

                var nameColumn = new VerticalStackPanel();
                nameColumn.Widgets.Add(CreateUsernameLabel(request.Username));
                nameColumn.Widgets.Add(CreateHint(TextKeys.Friends.AwaitingResponseMessage));
                itemRow.Widgets.Add(nameColumn);
                itemRow.Widgets.Add(CreateStatusBadge(TextKeys.Friends.PendingStatus));

                sentList.Widgets.Add(CreateItemCard(itemRow));
            }

            return sentList;
        }

        private VerticalStackPanel BuildSearchResults()
        {
            List<SearchResultEntry> matchingPlayers = _searchablePlayers
                .Where(searchablePlayer => searchablePlayer.Username.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (matchingPlayers.Count == 0)
            {
                VerticalStackPanel noResultsState = CreateEmptyState(TextKeys.Friends.NoResultsTitle, TextKeys.Friends.NoResultsHint);
                return noResultsState;
            }

            var resultsList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (SearchResultEntry matchingPlayer in matchingPlayers)
            {
                HorizontalStackPanel itemRow = CreateButtonRow();
                itemRow.Widgets.Add(CreateUsernameLabel(matchingPlayer.Username));
                itemRow.Widgets.Add(BuildSearchStatusWidget(matchingPlayer.Status));
                resultsList.Widgets.Add(CreateItemCard(itemRow));
            }

            var searchResults = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
            searchResults.Widgets.Add(resultsList);
            searchResults.Widgets.Add(CreateHint(TextKeys.Friends.SelfExcludedHint));

            return searchResults;
        }

        private static Widget BuildSearchStatusWidget(SearchStatus searchStatus)
        {
            Widget statusWidget = searchStatus switch
            {
                SearchStatus.New => CreatePrimaryButton(TextKeys.Friends.SendRequestButton),
                SearchStatus.Sent => CreateStatusBadge(TextKeys.Friends.RequestSentStatus),
                SearchStatus.Received => CreateStatusBadge(TextKeys.Friends.RequestReceivedStatus),
                _ => CreateStatusBadge(TextKeys.Friends.AlreadyFriendsStatus),
            };

            return statusWidget;
        }

        private void SelectTab(FriendsTab tab)
        {
            _activeTab = tab;
            RenderTab();
        }

        private void SearchBoxOnTextChanged(object sender, ValueChangedEventArgs<string> e)
        {
            _searchTerm = _searchField.Value;
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

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MainMenu;
        }

        private static List<FriendEntry> CreateSampleFriends()
        {
            var sampleFriends = new List<FriendEntry>
            {
                new FriendEntry(PreviewSamples.LuisPlayer),
                new FriendEntry(PreviewSamples.SofiaPlayer),
                new FriendEntry(PreviewSamples.DiegoPlayer),
            };

            return sampleFriends;
        }

        private static List<RequestEntry> CreateSampleReceivedRequests()
        {
            var sampleReceivedRequests = new List<RequestEntry>
            {
                new RequestEntry(PreviewSamples.MarcoPlayer),
                new RequestEntry(PreviewSamples.PauPlayer),
            };

            return sampleReceivedRequests;
        }

        private static List<RequestEntry> CreateSampleSentRequests()
        {
            var sampleSentRequests = new List<RequestEntry>
            {
                new RequestEntry(PreviewSamples.InesPlayer),
                new RequestEntry(PreviewSamples.TomasPlayer),
            };

            return sampleSentRequests;
        }

        private static List<SearchResultEntry> CreateSampleSearchResults()
        {
            var sampleSearchResults = new List<SearchResultEntry>
            {
                new SearchResultEntry(PreviewSamples.NewPlayer, SearchStatus.New),
                new SearchResultEntry(PreviewSamples.LuisPlayer, SearchStatus.Friend),
                new SearchResultEntry(PreviewSamples.InesPlayer, SearchStatus.Sent),
                new SearchResultEntry(PreviewSamples.MarcoPlayer, SearchStatus.Received),
            };

            return sampleSearchResults;
        }

        private sealed class FriendEntry
        {
            internal FriendEntry(string username)
            {
                Username = username;
            }

            internal string Username { get; }
        }

        private sealed class RequestEntry
        {
            internal RequestEntry(string username)
            {
                Username = username;
            }

            internal string Username { get; }
        }

        private sealed class SearchResultEntry
        {
            internal SearchResultEntry(string username, SearchStatus searchStatus)
            {
                Username = username;
                Status = searchStatus;
            }

            internal string Username { get; }

            internal SearchStatus Status { get; }
        }
    }
}
