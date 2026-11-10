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
    internal sealed class InvitePlayersScreen : Screen
    {
        private const double InvitationStateRefreshSeconds = 5;

        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly HorizontalStackPanel _tabsPlaceholder = CreateButtonRow();
        private readonly HorizontalStackPanel _searchRow = CreateButtonRow();
        private readonly VerticalStackPanel _tabContentPlaceholder = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
        private readonly LabeledTextBox _searchField = new LabeledTextBox(TextKeys.Friends.SearchPlaceholder, false);
        private readonly LocalizedButton _searchButton = CreatePrimaryButton(TextKeys.Friends.SearchButton);
        private readonly LocalizedLabel _errorLabel = CreateErrorMessage(TextKeys.Common.ServerErrorTitle);
        private readonly HashSet<int> _invitedPlayerIds = new HashSet<int>();
        private readonly AvatarViewSet _avatarViews;

        private InviteTab _activeTab = InviteTab.MyFriends;
        private Task<FriendshipListResponse>? _friendshipListRequest;
        private Task<RoomInviteesResponse>? _inviteesRequest;
        private Task<PlayerSearchResponse>? _searchRequest;
        private Task<SendInvitationResult>? _invitationRequest;
        private List<PlayerSummary> _friends = new List<PlayerSummary>();
        private PlayerSearchResponse? _searchResult;
        private int _invitedPlayerId;
        private double _secondsSinceRefresh;

        internal InvitePlayersScreen(GameSessionConnection gameSessionConnection, PlayerSession playerSession, AvatarCache avatarCache)
            : base(TextKeys.InvitePlayers.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(playerSession);
            ArgumentNullException.ThrowIfNull(avatarCache);

            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _avatarViews = new AvatarViewSet(avatarCache, Sizes.SmallAvatarSize);
        }

        private enum InviteTab
        {
            MyFriends,
            SearchByUsername,
        }

        internal override void Open()
        {
            ForgetRequests();
            _activeTab = InviteTab.MyFriends;
            _friends = new List<PlayerSummary>();
            _invitedPlayerIds.Clear();
            _searchResult = null;
            _searchField.Box.Text = string.Empty;
            _errorLabel.Visible = false;
            Room? currentRoom = _playerSession.CurrentRoom;
            HeaderArguments = currentRoom is null ? null : new string[] { currentRoom.Code };
            RenderTab();

            if (!_playerSession.HasAccount || !_gameSessionConnection.IsSessionOpen)
            {
                RequestedScreen = ScreenId.Login;
            }
            else if (currentRoom is null)
            {
                RequestedScreen = ScreenId.MainMenu;
            }
            else
            {
                StartRefresh();
            }
        }

        internal override void Update(GameTime gameTime)
        {
            RefreshPeriodically(gameTime);
            FollowFriendshipListRequest();
            FollowInviteesRequest();
            FollowSearch();
            FollowInvitationRequest();
            _avatarViews.Refresh();
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(_tabsPlaceholder);
            page.Widgets.Add(BuildSearchRow());
            page.Widgets.Add(_errorLabel);
            page.Widgets.Add(_tabContentPlaceholder);
            StackPanel.SetProportionType(_tabContentPlaceholder, ProportionType.Fill);
            RenderTab();

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackToRoomButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(CreateBackBar(backButton));

            return page;
        }

        private void RefreshPeriodically(GameTime gameTime)
        {
            _secondsSinceRefresh += gameTime.ElapsedGameTime.TotalSeconds;
            bool isRefreshDue = (_secondsSinceRefresh >= InvitationStateRefreshSeconds)
                && (_friendshipListRequest is null)
                && (_inviteesRequest is null);
            if (isRefreshDue)
            {
                StartRefresh();
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
                List<PlayerSummary> receivedFriends = completedFriendshipList.Result.Friends;
                bool hasChanged = !ShowsSameFriends(_friends, receivedFriends);
                _friends = receivedFriends;
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

        private void FollowInviteesRequest()
        {
            if ((_inviteesRequest is null) || !_inviteesRequest.IsCompleted)
            {
                return;
            }

            Task<RoomInviteesResponse> completedInvitees = _inviteesRequest;
            _inviteesRequest = null;
            if (completedInvitees.IsCompletedSuccessfully)
            {
                bool hasChanged = !_invitedPlayerIds.SetEquals(completedInvitees.Result.InvitedPlayerIds);
                _invitedPlayerIds.Clear();
                _invitedPlayerIds.UnionWith(completedInvitees.Result.InvitedPlayerIds);
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

        private void FollowInvitationRequest()
        {
            if ((_invitationRequest is null) || !_invitationRequest.IsCompleted)
            {
                return;
            }

            Task<SendInvitationResult> completedInvitation = _invitationRequest;
            _invitationRequest = null;
            if (completedInvitation.IsCompletedSuccessfully)
            {
                ShowInvitationOutcome(completedInvitation.Result);
            }
            else
            {
                ShowFailure();
            }
        }

        private void ShowInvitationOutcome(SendInvitationResult invitationResult)
        {
            switch (invitationResult)
            {
                case SendInvitationResult.Sent:
                case SendInvitationResult.InvitationAlreadyPending:
                    _invitedPlayerIds.Add(_invitedPlayerId);
                    RenderTab();
                    break;

                case SendInvitationResult.AlreadyInThisRoom:
                    StartRefresh();
                    RenderTab();
                    break;

                case SendInvitationResult.PlayerNotFound:
                    ShowInviteeMissing();
                    break;

                case SendInvitationResult.NotInRoom:
                    ForgetRequests();
                    RequestedScreen = ScreenId.MainMenu;
                    break;

                case SendInvitationResult.DatabaseUnavailable:
                    ShowFailure();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(invitationResult), invitationResult, null);
            }
        }

        private void ShowInviteeMissing()
        {
            if (_activeTab == InviteTab.SearchByUsername)
            {
                _searchResult = new PlayerSearchResponse { Status = PlayerSearchStatus.NotFound };
            }

            StartRefresh();
            RenderTab();
        }

        private static bool ShowsSameFriends(List<PlayerSummary> shownFriends, List<PlayerSummary> receivedFriends)
        {
            bool hasSamePlayers = shownFriends.Select(friend => friend.PlayerId)
                .SequenceEqual(receivedFriends.Select(friend => friend.PlayerId));
            bool hasSameConnectionStatus = shownFriends.Select(friend => friend.IsOnline)
                .SequenceEqual(receivedFriends.Select(friend => friend.IsOnline));
            bool showsSameFriends = hasSamePlayers && hasSameConnectionStatus;

            return showsSameFriends;
        }

        private void StartRefresh()
        {
            _secondsSinceRefresh = 0;
            if (_gameSessionConnection.IsSessionOpen)
            {
                _friendshipListRequest = _gameSessionConnection.GetFriendshipListAsync();
                _inviteesRequest = _gameSessionConnection.GetRoomInviteesAsync();
            }
        }

        private void SendInvitation(int targetPlayerId)
        {
            bool canSend = _gameSessionConnection.IsSessionOpen && (_invitationRequest is null);
            if (canSend)
            {
                _errorLabel.Visible = false;
                _invitedPlayerId = targetPlayerId;
                _invitationRequest = _gameSessionConnection.SendInvitationAsync(targetPlayerId);
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
            _friendshipListRequest = null;
            _inviteesRequest = null;
            _searchRequest = null;
            _invitationRequest = null;
        }

        private HorizontalStackPanel BuildSearchRow()
        {
            _searchRow.Widgets.Add(_searchField);
            StackPanel.SetProportionType(_searchField, ProportionType.Fill);
            _searchButton.VerticalAlignment = VerticalAlignment.Bottom;
            _searchButton.Click += SearchButtonOnClick;
            _searchRow.Widgets.Add(_searchButton);

            return _searchRow;
        }

        private void RenderTab()
        {
            bool isSearchTab = _activeTab == InviteTab.SearchByUsername;
            _avatarViews.Clear();
            RenderTabButtons();
            _searchRow.Visible = isSearchTab;
            Widget tabContent = isSearchTab ? BuildSearchResult() : BuildFriendsList();
            _tabContentPlaceholder.Widgets.Clear();
            _tabContentPlaceholder.Widgets.Add(tabContent);
        }

        private void RenderTabButtons()
        {
            LocalizedButton myFriendsTabButton = BuildTabButton(TextKeys.InvitePlayers.MyFriendsTab, InviteTab.MyFriends);
            LocalizedButton searchTabButton = BuildTabButton(TextKeys.InvitePlayers.SearchByUsernameTab, InviteTab.SearchByUsername);
            myFriendsTabButton.Click += MyFriendsTabButtonOnClick;
            searchTabButton.Click += SearchTabButtonOnClick;

            _tabsPlaceholder.Widgets.Clear();
            _tabsPlaceholder.Widgets.Add(myFriendsTabButton);
            _tabsPlaceholder.Widgets.Add(searchTabButton);
        }

        private LocalizedButton BuildTabButton(string textKey, InviteTab tab)
        {
            LocalizedButton tabButton = tab == _activeTab ? CreatePrimaryButton(textKey) : CreateSecondaryButton(textKey);

            return tabButton;
        }

        private VerticalStackPanel BuildFriendsList()
        {
            if (_friends.Count == 0)
            {
                VerticalStackPanel noFriendsState = CreateEmptyState(TextKeys.Friends.EmptyStateTitle, TextKeys.Friends.EmptyStateHint);
                return noFriendsState;
            }

            var friendsList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (PlayerSummary friend in _friends)
            {
                HorizontalStackPanel itemRow = CreateButtonRow();
                itemRow.HorizontalAlignment = HorizontalAlignment.Stretch;
                itemRow.Widgets.Add(_avatarViews.Create(friend.AvatarReference).Root);

                var nameColumn = new VerticalStackPanel { VerticalAlignment = VerticalAlignment.Center };
                nameColumn.Widgets.Add(CreateUsernameLabel(friend.Username));
                nameColumn.Widgets.Add(CreateConnectionStatusRow(friend.IsOnline));
                itemRow.Widgets.Add(nameColumn);

                AddInvitationControl(itemRow, friend);
                friendsList.Widgets.Add(CreateItemCard(itemRow));
            }

            return friendsList;
        }

        private Widget BuildSearchResult()
        {
            PlayerSummary? foundPlayer = _searchResult?.Status == PlayerSearchStatus.Found ? _searchResult.Player : null;
            if (foundPlayer is null)
            {
                Widget noResultsContent = _searchResult is null
                    ? new Panel()
                    : CreateEmptyState(TextKeys.Friends.NoResultsTitle, TextKeys.Friends.NoResultsHint);
                return noResultsContent;
            }

            HorizontalStackPanel itemRow = CreateButtonRow();
            itemRow.HorizontalAlignment = HorizontalAlignment.Stretch;
            itemRow.Widgets.Add(_avatarViews.Create(foundPlayer.AvatarReference).Root);
            itemRow.Widgets.Add(CreateUsernameLabel(foundPlayer.Username));
            AddInvitationControl(itemRow, foundPlayer);

            Panel resultCard = CreateItemCard(itemRow);

            return resultCard;
        }

        private void AddInvitationControl(HorizontalStackPanel itemRow, PlayerSummary player)
        {
            var spacer = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
            StackPanel.SetProportionType(spacer, ProportionType.Part);
            itemRow.Widgets.Add(spacer);

            Widget invitationControl;
            if (IsRoomMember(player.Username))
            {
                invitationControl = CreateStatusBadge(TextKeys.InvitePlayers.AlreadyInRoomBadge);
            }
            else if (_invitedPlayerIds.Contains(player.PlayerId))
            {
                invitationControl = CreateStatusBadge(TextKeys.InvitePlayers.InvitedBadge);
            }
            else
            {
                LocalizedButton inviteButton = CreatePrimaryButton(TextKeys.InvitePlayers.InviteButton);
                inviteButton.Click += (sender, e) => SendInvitation(player.PlayerId);
                invitationControl = inviteButton;
            }

            itemRow.Widgets.Add(invitationControl);
        }

        private bool IsRoomMember(string username)
        {
            bool isRoomMember = _playerSession.CurrentRoom?.Members
                .Any(member => !member.IsGuest && string.Equals(member.DisplayName, username, StringComparison.OrdinalIgnoreCase))
                ?? false;

            return isRoomMember;
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
            else if ((_searchRequest is null) && _gameSessionConnection.IsSessionOpen)
            {
                _searchRequest = _gameSessionConnection.FindPlayerByUsernameAsync(username);
            }
        }

        private void SelectTab(InviteTab tab)
        {
            _activeTab = tab;
            _errorLabel.Visible = false;
            RenderTab();
        }

        private void MyFriendsTabButtonOnClick(object sender, MyraEventArgs e)
        {
            SelectTab(InviteTab.MyFriends);
        }

        private void SearchTabButtonOnClick(object sender, MyraEventArgs e)
        {
            SelectTab(InviteTab.SearchByUsername);
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            ForgetRequests();
            RequestedScreen = ScreenId.Room;
        }
    }
}
