using System;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MainMenuScreen : Screen
    {
        private const string Chevron = "›";

        private readonly LanguagePreference _languagePreference;
        private LocalizedButton? _exitButton;
        private LocalizedButton? _languageButton;
        private LocalizedButton? _roomsButton;
        private LocalizedButton? _rankingButton;
        private LocalizedButton? _friendsButton;
        private LocalizedButton? _invitationsButton;
        private LocalizedButton? _historyButton;

        internal MainMenuScreen(LanguagePreference languagePreference)
            : base(TextKeys.MainMenu.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(languagePreference);

            _languagePreference = languagePreference;
        }

        internal bool WasExitRequested { get; private set; }

        internal void FollowLanguage()
        {
            if (_languageButton is null)
            {
                return;
            }

            _languageButton.TextArgumentKey = _languagePreference.Current.NameKey;
            _languageButton.RefreshText();
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = Columns();
            columns.Spacing = Metrics.MenuRowSpacing;
            columns.Widgets.Add(BuildMenuList());
            columns.Widgets.Add(Ambience());
            StackPanel.SetProportionType(columns.Widgets[1], ProportionType.Fill);

            return columns;
        }

        private static LocalizedButton MenuItem(string textKey, bool isAvailable)
        {
            var item = new LocalizedButton(textKey)
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
            item.SetTrailingMark(Chevron, Theme.MutedInk);

            return item;
        }

        private static Panel BuildSeparator()
        {
            var box = new Panel
            {
                Padding = new Thickness(Metrics.MenuSeparatorInset, Metrics.MenuSeparatorSpacing),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            box.Widgets.Add(Divider());

            return box;
        }

        private VerticalStackPanel BuildMenuList()
        {
            var list = new VerticalStackPanel
            {
                Width = Sizes.MenuListWidth,
                VerticalAlignment = VerticalAlignment.Top,
            };

            _invitationsButton = MenuItem(TextKeys.MainMenu.InvitationsButton, true);
            _invitationsButton.Click += InvitationsButtonOnClick;
            list.Widgets.Add(_invitationsButton);
            _friendsButton = MenuItem(TextKeys.MainMenu.FriendsButton, true);
            _friendsButton.Click += FriendsButtonOnClick;
            list.Widgets.Add(_friendsButton);
            _historyButton = MenuItem(TextKeys.MainMenu.HistoryButton, true);
            _historyButton.Click += HistoryButtonOnClick;
            list.Widgets.Add(_historyButton);

            _rankingButton = MenuItem(TextKeys.MainMenu.RankingButton, true);
            _rankingButton.Click += RankingButtonOnClick;
            list.Widgets.Add(_rankingButton);

            list.Widgets.Add(BuildSeparator());

            _exitButton = MenuItem(TextKeys.MainMenu.ExitButton, true);
            _exitButton.Click += ExitButtonOnClick;
            list.Widgets.Add(_exitButton);

            _roomsButton = MenuItem(TextKeys.MainMenu.RoomsButton, true);
            _roomsButton.Click += RoomsButtonOnClick;
            list.Widgets.Add(_roomsButton);

            list.Widgets.Add(BuildLanguageButton());

            return list;
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
            _languageButton.TextArgumentKey = _languagePreference.Current.NameKey;
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
