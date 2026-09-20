using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    /// <summary>
    /// PT-02 del prototipo: la lista de opciones a la izquierda, con el idioma al pie,
    /// y el recuadro de ambientación ocupando el resto.
    /// </summary>
    internal sealed class MainMenuScreen : Screen
    {
        private const string Chevron = "›";

        private readonly LanguageService _languageService;
        private LocalizedButton? _exitButton;
        private LocalizedButton? _languageButton;

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

        /// <summary>
        /// .mlist button. Las opciones que todavía no tienen pantalla llevan la variante .dis:
        /// tinta apagada y sin realce al pasar el ratón.
        /// </summary>
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

        /// <summary>.mlist hr: la línea que separa Salir del resto, con 8px de aire y 4px de sangría.</summary>
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
            list.Widgets.Add(MenuItem(TextKeys.MainMenu.RoomsButton, false));
            list.Widgets.Add(MenuItem(TextKeys.MainMenu.InvitationsButton, false));
            list.Widgets.Add(MenuItem(TextKeys.MainMenu.FriendsButton, false));
            list.Widgets.Add(MenuItem(TextKeys.MainMenu.HistoryButton, false));
            list.Widgets.Add(MenuItem(TextKeys.MainMenu.RankingButton, false));

            list.Widgets.Add(BuildSeparator());

            _exitButton = MenuItem(TextKeys.MainMenu.ExitButton, true);
            _exitButton.Click += OnExitClick;
            list.Widgets.Add(_exitButton);

            list.Widgets.Add(BuildLanguageButton());
            return list;
        }

        private LocalizedButton BuildLanguageButton()
        {
            // .langbtn: pastilla de fondo --surface-2, alineada a la izquierda y separada 6px.
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

        private void OnExitClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            WasExitRequested = true;
        }

        private void OnLanguageClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Language;
        }
    }
}
