using System.Collections.Generic;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Avatars;
using Torres.Client.Contracts;
using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class InvitePlayersScreen : Screen
    {
        private readonly InvitePlayersFlow _invitePlayersFlow;
        private readonly HorizontalStackPanel _tabsPlaceholder = PanelFactory.CreateButtonRow();
        private readonly HorizontalStackPanel _searchRow = PanelFactory.CreateButtonRow();
        private readonly VerticalStackPanel _tabContentPlaceholder = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
        private readonly LabeledTextBox _searchField = new LabeledTextBox(TextKeys.Friends.SearchPlaceholder, false, AccountLimits.LongestUsername);
        private readonly LocalizedButton _searchButton = ButtonFactory.CreatePrimaryButton(TextKeys.Friends.SearchButton);
        private readonly LocalizedLabel _errorLabel = LabelFactory.CreateErrorMessage(TextKeys.Common.ServerErrorTitle);
        private readonly AvatarViewSet _avatarViews;

        private InviteTab _activeTab = InviteTab.MyFriends;

        internal InvitePlayersScreen(InvitePlayersFlow invitePlayersFlow, AvatarCache avatarCache)
            : base(TextKeys.InvitePlayers.HeaderLabel, true)
        {
            _invitePlayersFlow = invitePlayersFlow;
            _avatarViews = new AvatarViewSet(avatarCache, Sizes.SmallAvatarSize);
        }

        private enum InviteTab
        {
            MyFriends,
            SearchByUsername,
        }

        internal override void Open()
        {
            _activeTab = InviteTab.MyFriends;
            _searchField.Box.Text = string.Empty;
            _errorLabel.Visible = false;
            RequestedScreen = _invitePlayersFlow.Open();
            string? roomCode = _invitePlayersFlow.RoomCode;
            HeaderArguments = roomCode is null ? null : new string[] { roomCode };
            RenderTab();
        }

        internal override void Update(GameTime gameTime)
        {
            _invitePlayersFlow.Advance(gameTime.ElapsedGameTime);
            InvitePlayersOutcome? outcome = _invitePlayersFlow.TakeOutcome();
            if (outcome is not null)
            {
                ShowOutcome(outcome);
            }

            _avatarViews.Refresh();
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = PanelFactory.CreatePage();
            page.Spacing = Metrics.FieldSpacing;
            page.Widgets.Add(_tabsPlaceholder);
            page.Widgets.Add(BuildSearchRow());
            page.Widgets.Add(_errorLabel);
            page.Widgets.Add(_tabContentPlaceholder);
            StackPanel.SetProportionType(_tabContentPlaceholder, ProportionType.Fill);
            RenderTab();

            LocalizedButton backButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.BackToRoomButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(PanelFactory.CreateBackBar(backButton));

            return page;
        }

        private void ShowOutcome(InvitePlayersOutcome outcome)
        {
            if (outcome.NeedsRender)
            {
                RenderTab();
            }

            if (outcome.ErrorKey is string errorKey)
            {
                ShowError(errorKey);
            }

            if (outcome.NextScreen is ScreenId nextScreen)
            {
                RequestedScreen = nextScreen;
            }
        }

        private void ShowError(string errorKey)
        {
            _errorLabel.TextKey = errorKey;
            _errorLabel.Visible = true;
        }

        private void SendInvitation(int targetPlayerId, bool isByEmail)
        {
            if (_invitePlayersFlow.Invite(targetPlayerId, isByEmail))
            {
                _errorLabel.Visible = false;
            }
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
            LocalizedButton tabButton = tab == _activeTab ? ButtonFactory.CreatePrimaryButton(textKey) : ButtonFactory.CreateSecondaryButton(textKey);

            return tabButton;
        }

        private VerticalStackPanel BuildFriendsList()
        {
            IReadOnlyList<PlayerSummary> friends = _invitePlayersFlow.Friends;
            if (friends.Count == 0)
            {
                VerticalStackPanel noFriendsState = PanelFactory.CreateEmptyState(TextKeys.Friends.EmptyStateTitle, TextKeys.Friends.EmptyStateHint);
                return noFriendsState;
            }

            var friendsList = new VerticalStackPanel { Spacing = Metrics.ListSpacing };
            foreach (PlayerSummary friend in friends)
            {
                HorizontalStackPanel itemRow = PanelFactory.CreateButtonRow();
                itemRow.HorizontalAlignment = HorizontalAlignment.Stretch;
                itemRow.Widgets.Add(_avatarViews.Create(friend.AvatarReference).Root);

                var nameColumn = new VerticalStackPanel { VerticalAlignment = VerticalAlignment.Center };
                nameColumn.Widgets.Add(LabelFactory.CreateUsernameLabel(friend.Username));
                nameColumn.Widgets.Add(PanelFactory.CreateConnectionStatusRow(friend.IsOnline));
                itemRow.Widgets.Add(nameColumn);

                AddInvitationControl(itemRow, friend);
                friendsList.Widgets.Add(PanelFactory.CreateItemCard(itemRow));
            }

            return friendsList;
        }

        private Widget BuildSearchResult()
        {
            PlayerSearchResponse? searchResult = _invitePlayersFlow.SearchResult;
            PlayerSummary? foundPlayer = searchResult?.Status == PlayerSearchStatus.Found ? searchResult.Player : null;
            if (foundPlayer is null)
            {
                Widget noResultsContent = searchResult is null
                    ? new Panel()
                    : PanelFactory.CreateEmptyState(TextKeys.Friends.NoResultsTitle, TextKeys.Friends.NoResultsHint);
                return noResultsContent;
            }

            HorizontalStackPanel itemRow = PanelFactory.CreateButtonRow();
            itemRow.HorizontalAlignment = HorizontalAlignment.Stretch;
            itemRow.Widgets.Add(_avatarViews.Create(foundPlayer.AvatarReference).Root);
            itemRow.Widgets.Add(LabelFactory.CreateUsernameLabel(foundPlayer.Username));
            AddInvitationControl(itemRow, foundPlayer);

            Panel resultCard = PanelFactory.CreateItemCard(itemRow);

            return resultCard;
        }

        private void AddInvitationControl(HorizontalStackPanel itemRow, PlayerSummary player)
        {
            var spacer = new Panel { HorizontalAlignment = HorizontalAlignment.Stretch };
            StackPanel.SetProportionType(spacer, ProportionType.Part);
            itemRow.Widgets.Add(spacer);

            if (_invitePlayersFlow.IsRoomMember(player.Username))
            {
                itemRow.Widgets.Add(PanelFactory.CreateStatusBadge(TextKeys.InvitePlayers.AlreadyInRoomBadge));
                return;
            }

            if (_invitePlayersFlow.IsInvited(player.PlayerId))
            {
                itemRow.Widgets.Add(PanelFactory.CreateStatusBadge(TextKeys.InvitePlayers.InvitedBadge));
            }
            else
            {
                LocalizedButton inviteButton = ButtonFactory.CreatePrimaryButton(TextKeys.InvitePlayers.InviteButton);
                inviteButton.Click += (sender, e) => SendInvitation(player.PlayerId, false);
                itemRow.Widgets.Add(inviteButton);
            }

            LocalizedButton emailButton = ButtonFactory.CreateSecondaryButton(TextKeys.InvitePlayers.InviteByEmailButton);
            emailButton.Click += (sender, e) => SendInvitation(player.PlayerId, true);
            itemRow.Widgets.Add(emailButton);
        }

        private void SearchButtonOnClick(object sender, MyraEventArgs e)
        {
            _errorLabel.Visible = false;
            string? errorKey = _invitePlayersFlow.Search(_searchField.Value);
            if (errorKey is not null)
            {
                ShowError(errorKey);
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
            _invitePlayersFlow.ForgetRequests();
            RequestedScreen = ScreenId.Room;
        }
    }
}
