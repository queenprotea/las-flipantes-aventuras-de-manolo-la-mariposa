using System;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MainMenuScreen : Screen
    {
        private readonly LanguagePreference _languagePreference;
        private LocalizedButton? _exitButton;
        private LocalizedButton? _languageButton;
        private LocalizedButton? _roomsButton;
        private LocalizedButton? _rankingButton;
        private LocalizedButton? _friendsButton;
        private LocalizedButton? _invitationsButton;
        private LocalizedButton? _historyButton;
        private NoticeBox? _noticeBox;

        internal MainMenuScreen(LanguagePreference languagePreference)
            : base(TextKeys.MainMenu.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(languagePreference);

            _languagePreference = languagePreference;
        }

        internal bool WasExitRequested { get; private set; }

        internal override void Open()
        {
            _noticeBox?.Hide();
        }

        internal void ShowNotice(string titleKey, string bodyKey, object[]? bodyArguments)
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
            var menuItem = new LocalizedButton(textKey)
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
            menuItem.SetTrailingMark(DisplaySymbols.MenuChevron, Theme.MutedInk);

            return menuItem;
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

        private VerticalStackPanel BuildMenuList()
        {
            var menuList = new VerticalStackPanel
            {
                Width = Sizes.MenuListWidth,
                VerticalAlignment = VerticalAlignment.Top,
            };
            _noticeBox = new NoticeBox();
            menuList.Widgets.Add(_noticeBox);

            _invitationsButton = CreateMenuItem(TextKeys.MainMenu.InvitationsButton, true);
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

            _roomsButton = CreateMenuItem(TextKeys.MainMenu.RoomsButton, true);
            _roomsButton.Click += RoomsButtonOnClick;
            menuList.Widgets.Add(_roomsButton);

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
            RequestedScreen = ScreenId.ReceivedRequests;
        }

        private void HistoryButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.History;
        }
    }
}
