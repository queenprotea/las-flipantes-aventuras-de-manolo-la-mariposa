using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MainMenuScreen : Screen
    {
        private const string Chevron = "›";

        private readonly LanguageService _languageService;
        private LocalizedButton? _exitButton;
        private LocalizedButton? _languageButton;
        private LocalizedButton? _roomsButton;
        private LocalizedButton? _ranking;

        internal MainMenuScreen(LanguageService languageService)
            : base(TextKeys.MainMenu.HeaderLabel, true)
        {
            _languageService = languageService;
        }

        internal bool WasExitRequested { get; private set; }

        internal void FollowLanguage()
        {
            if (_languageButton is null)
            {
                return;
            }

            _languageButton.TextArgumentKey = _languageService.Current.NameKey;
            _languageButton.RefreshText();
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = Columns();
            columns.Spacing = Theme.MenuRowSpacing;
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
                Padding = Theme.MenuItemPadding,
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
                Padding = new Thickness(Theme.MenuSeparatorInset, Theme.MenuSeparatorSpacing),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            box.Widgets.Add(Divider());
            return box;
        }

        private VerticalStackPanel BuildMenuList()
        {
            var list = new VerticalStackPanel
            {
                Width = Theme.MenuListWidth,
                VerticalAlignment = VerticalAlignment.Top,
            };

            list.Widgets.Add(MenuItem(TextKeys.MainMenu.InvitationsButton, false));
            list.Widgets.Add(MenuItem(TextKeys.MainMenu.FriendsButton, false));
            list.Widgets.Add(MenuItem(TextKeys.MainMenu.HistoryButton, false));
            
            _ranking = MenuItem(TextKeys.MainMenu.RankingButton, true);
            _ranking.Click += OnRankingClick;
            list.Widgets.Add(_ranking);

            list.Widgets.Add(BuildSeparator());

            _exitButton = MenuItem(TextKeys.MainMenu.ExitButton, true);
            _exitButton.Click += OnExitClick;
            list.Widgets.Add(_exitButton);

            _roomsButton = MenuItem(TextKeys.MainMenu.RoomsButton, true);
            _roomsButton.Click += OnRoomsClick;
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
                Padding = Theme.ListItemPadding,
                Background = Theme.SurfaceAltBrush,
                OverBackground = Theme.SurfaceAltBrush,
                PressedBackground = Theme.SurfaceAltBrush,
                Border = Theme.LineBrush,
                BorderThickness = Theme.Border,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, Theme.ListSpacing, 0, 0),
            };
            _languageButton.TextArgumentKey = _languageService.Current.NameKey;
            _languageButton.RefreshText();
            _languageButton.Click += OnLanguageClick;
            return _languageButton;
        }
        
        private void OnRankingClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.GlobalRanking;
        }

        private void OnExitClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            WasExitRequested = true;
        }

        private void OnRoomsClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Rooms;
        }

        private void OnLanguageClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Language;
        }
    }
}
