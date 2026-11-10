using System;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MainMenuScreen : Screen
    {
        private const int MostInvitationsCounted = 9;

        private readonly LanguagePreference _languagePreference;
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private LocalizedButton? _exitButton;
        private LocalizedButton? _languageButton;
        private LocalizedButton? _roomsButton;
        private LocalizedButton? _rankingButton;
        private LocalizedButton? _friendsButton;
        private LocalizedButton? _invitationsButton;
        private LocalizedButton? _historyButton;
        private Label? _invitationsMark;
        private NoticeBox? _noticeBox;
        private Task<PendingInvitationsResponse>? _invitationsRequest;
        private int? _shownInvitationsRevision;

        internal MainMenuScreen(LanguagePreference languagePreference, GameSessionConnection gameSessionConnection, PlayerSession playerSession)
            : base(TextKeys.MainMenu.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(languagePreference);
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(playerSession);

            _languagePreference = languagePreference;
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
        }

        internal bool WasExitRequested { get; private set; }

        internal override void Open()
        {
            _noticeBox?.Hide();
            FollowSession();
            ShowInvitationCount();
            bool canReadInvitations = _playerSession.HasAccount && _gameSessionConnection.IsSessionOpen && (_invitationsRequest is null);
            if (canReadInvitations)
            {
                _invitationsRequest = _gameSessionConnection.GetPendingInvitationsAsync();
            }
        }

        internal override void Update(GameTime gameTime)
        {
            FollowInvitationsRequest();
            if (_shownInvitationsRevision != _playerSession.Invitations.Revision)
            {
                ShowInvitationCount();
            }
        }

        internal void ShowNotice(string titleKey, string bodyKey, string[]? bodyArguments)
        {
            _noticeBox?.ShowMessage(titleKey, bodyKey, bodyArguments);
        }

        internal void FollowLanguage()
        {
            if (_languageButton is null)
            {
                return;
            }

            _languageButton.TextArgumentKey = _languagePreference.CurrentLanguage.NameKey;
            _languageButton.RefreshText();
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = CreateColumns();
            columns.Spacing = Metrics.MenuRowSpacing;
            columns.Widgets.Add(BuildMenuList());
            columns.Widgets.Add(CreateAmbiencePanel());
            StackPanel.SetProportionType(columns.Widgets[1], ProportionType.Fill);

            return columns;
        }

        private static LocalizedButton CreateMenuItem(string textKey, bool isAvailable)
        {
            LocalizedButton menuItem = CreateMenuButton(textKey, isAvailable);
            menuItem.SetTrailingMark(DisplaySymbols.MenuChevron, Theme.MutedInk);

            return menuItem;
        }

        private static LocalizedButton CreateMenuButton(string textKey, bool isAvailable)
        {
            var menuButton = new LocalizedButton(textKey)
            {
                LabelColor = isAvailable ? Theme.Ink : Theme.MutedInk,
                LabelFont = Fonts.MenuItem,
                Padding = Metrics.MenuItemPadding,
                Background = null,
                OverBackground = isAvailable ? Theme.MintTintBrush : null,
                PressedBackground = isAvailable ? Theme.MintTintBrush : null,
                Border = null,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            return menuButton;
        }

        private static void ApplyAvailability(LocalizedButton? menuItem, bool isAvailable)
        {
            if (menuItem is null)
            {
                return;
            }

            menuItem.Enabled = isAvailable;
            menuItem.LabelColor = isAvailable ? Theme.Ink : Theme.MutedInk;
            menuItem.OverBackground = isAvailable ? Theme.MintTintBrush : null;
            menuItem.PressedBackground = isAvailable ? Theme.MintTintBrush : null;
        }

        private static Panel BuildSeparator()
        {
            var separatorPanel = new Panel
            {
                Padding = new Thickness(Metrics.MenuSeparatorInset, Metrics.MenuSeparatorSpacing),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            separatorPanel.Widgets.Add(CreateDivider());

            return separatorPanel;
        }

        private static string FormatInvitationCount(int invitationCount)
        {
            string countText = DisplayFormats.FormatInteger(invitationCount);
            if (invitationCount == 0)
            {
                countText = DisplaySymbols.MenuChevron;
            }
            else if (invitationCount > MostInvitationsCounted)
            {
                countText = DisplaySymbols.CountOverflow;
            }

            return countText;
        }

        private void FollowInvitationsRequest()
        {
            if ((_invitationsRequest is null) || !_invitationsRequest.IsCompleted)
            {
                return;
            }

            Task<PendingInvitationsResponse> completedRequest = _invitationsRequest;
            _invitationsRequest = null;
            if (completedRequest.IsCompletedSuccessfully && _playerSession.HasAccount)
            {
                _playerSession.Invitations.Load(completedRequest.Result);
            }
        }

        private void ShowInvitationCount()
        {
            _shownInvitationsRevision = _playerSession.Invitations.Revision;
            if (_invitationsMark is null)
            {
                return;
            }

            int invitationCount = _playerSession.HasAccount ? _playerSession.Invitations.Invitations.Count : 0;
            _invitationsMark.Text = FormatInvitationCount(invitationCount);
            _invitationsMark.TextColor = invitationCount == 0 ? Theme.MutedInk : Theme.MintInk;
        }

        private void FollowSession()
        {
            bool canUseAccountFeatures = !_playerSession.IsGuest;
            ApplyAvailability(_invitationsButton, canUseAccountFeatures);
            ApplyAvailability(_friendsButton, canUseAccountFeatures);
            ApplyAvailability(_historyButton, canUseAccountFeatures);
        }

        private VerticalStackPanel BuildMenuList()
        {
            var menuList = new VerticalStackPanel
            {
                Width = Sizes.MenuListWidth,
                VerticalAlignment = VerticalAlignment.Top,
            };
            _noticeBox = new NoticeBox();
            menuList.Widgets.Add(_noticeBox);

            _roomsButton = CreateMenuItem(TextKeys.MainMenu.RoomsButton, true);
            _roomsButton.Click += RoomsButtonOnClick;
            menuList.Widgets.Add(_roomsButton);
            _invitationsButton = CreateMenuButton(TextKeys.MainMenu.InvitationsButton, true);
            _invitationsMark = _invitationsButton.SetTrailingMark(DisplaySymbols.MenuChevron, Theme.MutedInk);
            _invitationsButton.Click += InvitationsButtonOnClick;
            menuList.Widgets.Add(_invitationsButton);
            _friendsButton = CreateMenuItem(TextKeys.MainMenu.FriendsButton, true);
            _friendsButton.Click += FriendsButtonOnClick;
            menuList.Widgets.Add(_friendsButton);
            _historyButton = CreateMenuItem(TextKeys.MainMenu.HistoryButton, true);
            _historyButton.Click += HistoryButtonOnClick;
            menuList.Widgets.Add(_historyButton);
            _rankingButton = CreateMenuItem(TextKeys.MainMenu.RankingButton, true);
            _rankingButton.Click += RankingButtonOnClick;
            menuList.Widgets.Add(_rankingButton);

            menuList.Widgets.Add(BuildSeparator());

            _exitButton = CreateMenuItem(TextKeys.MainMenu.ExitButton, true);
            _exitButton.Click += ExitButtonOnClick;
            menuList.Widgets.Add(_exitButton);

            menuList.Widgets.Add(BuildLanguageButton());

            return menuList;
        }

        private LocalizedButton BuildLanguageButton()
        {
            _languageButton = new LocalizedButton(TextKeys.MainMenu.LanguageButton)
            {
                LabelColor = Theme.SoftInk,
                LabelFont = Fonts.Control,
                Padding = Metrics.ListItemPadding,
                Background = Theme.SurfaceAltBrush,
                OverBackground = Theme.SurfaceAltBrush,
                PressedBackground = Theme.SurfaceAltBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, Metrics.ListSpacing, 0, 0),
            };
            _languageButton.TextArgumentKey = _languagePreference.CurrentLanguage.NameKey;
            _languageButton.RefreshText();
            _languageButton.Click += LanguageButtonOnClick;

            return _languageButton;
        }

        private void RankingButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.GlobalRanking;
        }

        private void ExitButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            WasExitRequested = true;
        }

        private void RoomsButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Rooms;
        }

        private void LanguageButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Language;
        }

        private void FriendsButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Friends;
        }

        private void InvitationsButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Invitations;
        }

        private void HistoryButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.History;
        }
    }
}
