using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Screens;
using Torres.Client.Ui;

namespace Torres.Client
{
    internal sealed class TorresGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly LanguageService _languageService = new LanguageService();
        private readonly Dictionary<ScreenId, Screen> _screensById = new Dictionary<ScreenId, Screen>();
        private readonly MainMenuScreen _mainMenu;

        private Panel? _content;
        private Desktop? _desktop;
        private TopBarView? _topBar;
        private ScreenId _openScreen = ScreenId.Startup;
        private string _appliedUiCulture = string.Empty;
        private KeyboardState _previousKeyboard;

        internal TorresGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = Theme.WindowWidth;
            _graphics.PreferredBackBufferHeight = Theme.WindowHeight;
            IsMouseVisible = true;

            _mainMenu = new MainMenuScreen(_languageService);
            _screensById.Add(ScreenId.Startup, new StartupScreen());
            _screensById.Add(ScreenId.MainMenu, _mainMenu);
            _screensById.Add(ScreenId.Language, new LanguageScreen(_languageService));
            _screensById.Add(ScreenId.Login, new LoginScreen());
            _screensById.Add(ScreenId.Register, new RegisterScreen());
            _screensById.Add(ScreenId.PasswordRecovery, new PasswordRecoveryScreen());
            _screensById.Add(ScreenId.GuestAccess, new GuestAccessScreen());
            _screensById.Add(ScreenId.Profile, new ProfileScreen());
            _screensById.Add(ScreenId.AccountSettings, new AccountSettingsScreen());
            _screensById.Add(ScreenId.Rooms, new RoomsScreen());
            _screensById.Add(ScreenId.GlobalRanking, new GlobalRankingScreen());
        }

        protected override void Initialize()
        {
            _languageService.LoadSavedPreference();
            _appliedUiCulture = _languageService.Current.UiCultureName;
            MyraEnvironment.Game = this;

            _topBar = new TopBarView();
            _content = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var root = new VerticalStackPanel
            {
                Padding = new Thickness(Theme.ScreenPaddingX, Theme.ScreenPaddingY),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            root.Widgets.Add(_topBar.Panel);
            root.Widgets.Add(_content);
            StackPanel.SetProportionType(_content, ProportionType.Fill);

            _desktop = new Desktop();
            _desktop.HasExternalTextInput = true;
            _desktop.Root = root;
            Window.TextInput += OnTextInput;
            Show(ScreenId.Startup);
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            Screen screen = _screensById[_openScreen];
            screen.Update(gameTime);
            Window.Title = LocalizedText.Get(TextKeys.Common.WindowTitle);

            FollowLanguage();
            FollowNavigation(screen);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Theme.Surface);
            _desktop!.Render();
            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            Window.TextInput -= OnTextInput;
            base.UnloadContent();
        }

        private void OnTextInput(object? sender, TextInputEventArgs eventArguments)
        {
            _desktop!.OnChar(eventArguments.Character);
        }

        private void FollowLanguage()
        {
            if (_languageService.Current.UiCultureName == _appliedUiCulture)
            {
                return;
            }

            _appliedUiCulture = _languageService.Current.UiCultureName;
            _mainMenu.FollowLanguage();
            LanguageRefresher.Refresh(_topBar!.Panel);
            foreach (Screen screen in _screensById.Values)
            {
                RefreshIfBuilt(screen);
            }
        }

        private void RefreshIfBuilt(Screen screen)
        {
            if (!screen.IsBuilt)
            {
                return;
            }

            LanguageRefresher.Refresh(screen.Root);
        }

        private void FollowNavigation(Screen screen)
        {
            KeyboardState keyboard = Keyboard.GetState();
            bool escapePressed = keyboard.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape);
            _previousKeyboard = keyboard;

            if (_mainMenu.WasExitRequested || escapePressed)
            {
                Exit();
                return;
            }

            if (_topBar!.WasLogInRequested)
            {
                _topBar.ClearRequest();
                Show(ScreenId.Login);
            }

            if (screen.RequestedScreen is ScreenId requested)
            {
                screen.RequestedScreen = null;
                Show(requested);
            }
        }

        private void Show(ScreenId screenId)
        {
            _openScreen = screenId;
            Screen screen = _screensById[screenId];

            _content!.Widgets.Clear();
            _content.Widgets.Add(screen.Root);
            LanguageRefresher.Refresh(screen.Root);
            _topBar!.Follow(screen);
        }
    }
}
