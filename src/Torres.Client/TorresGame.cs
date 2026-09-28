using System;
using System.Collections.Generic;
using System.ServiceModel;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Screens;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client
{
    internal sealed class TorresGame : Microsoft.Xna.Framework.Game
    {
        private const string ServerStatusAddress = "net.tcp://localhost:8000/status";
        private const string AccountAddress = "net.tcp://localhost:8000/account";
        private const string RankingAddress = "net.tcp://localhost:8000/ranking";

        private readonly GraphicsDeviceManager _graphics;
        private readonly LanguagePreference _languagePreference = new LanguagePreference();
        private readonly PlayerSession _session = new PlayerSession();
        private readonly Dictionary<ScreenId, Screen> _screensById = new Dictionary<ScreenId, Screen>();
        private readonly MainMenuScreen _mainMenu;
        private readonly StartupScreen _startup;
        private readonly ChannelFactory<IServerStatusService> _statusChannelFactory;
        private readonly ChannelFactory<IAccountService> _accountChannelFactory;
        private readonly ChannelFactory<IRankingService> _rankingChannelFactory;

        private Panel? _content;
        private Desktop? _desktop;
        private TopBarView? _topBar;
        private ScreenId _openScreen = ScreenId.Startup;
        private string _appliedUiCulture = string.Empty;
        private KeyboardState _previousKeyboard;
        private Point _windowedSize = new Point(Sizes.WindowWidth, Sizes.WindowHeight);

        internal TorresGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = Sizes.WindowWidth;
            _graphics.PreferredBackBufferHeight = Sizes.WindowHeight;
            _graphics.HardwareModeSwitch = false;
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += WindowOnClientSizeChanged;

            _statusChannelFactory = new ChannelFactory<IServerStatusService>(
                new NetTcpBinding(SecurityMode.None),
                new EndpointAddress(ServerStatusAddress));

            _accountChannelFactory = new ChannelFactory<IAccountService>(
                new NetTcpBinding(SecurityMode.None),
                new EndpointAddress(AccountAddress));

            _rankingChannelFactory = new ChannelFactory<IRankingService>(
                new NetTcpBinding(SecurityMode.None),
                new EndpointAddress(RankingAddress));

            _mainMenu = new MainMenuScreen(_languagePreference);
            _startup = new StartupScreen(_statusChannelFactory);
        }

        protected override void Initialize()
        {
            _languagePreference.LoadSavedPreference();
            _appliedUiCulture = _languagePreference.Current.UiCultureName;
            MyraEnvironment.Game = this;
            WindowSizeLimit.ApplyMinimum(Window, Sizes.WindowWidth, Sizes.WindowHeight);

            RegisterScreens();

            _topBar = new TopBarView(_session);
            _content = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var root = new VerticalStackPanel
            {
                Padding = new Thickness(Metrics.ScreenPaddingX, Metrics.ScreenPaddingY),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            root.Widgets.Add(_topBar.Panel);
            root.Widgets.Add(_content);
            StackPanel.SetProportionType(_content, ProportionType.Fill);

            _desktop = new Desktop();
            _desktop.HasExternalTextInput = true;
            _desktop.Root = root;
            Window.TextInput += WindowOnTextInput;
            Show(ScreenId.Startup);
            base.Initialize();
        }

        private void RegisterScreens()
        {
            _screensById.Add(ScreenId.Startup, _startup);
            _screensById.Add(ScreenId.MainMenu, _mainMenu);
            _screensById.Add(ScreenId.Language, new LanguageScreen(_languagePreference));
            _screensById.Add(ScreenId.Login, new LoginScreen(_accountChannelFactory, _session));
            _screensById.Add(ScreenId.Register, new RegisterScreen(_accountChannelFactory));
            _screensById.Add(ScreenId.PasswordRecovery, new PasswordRecoveryScreen());
            _screensById.Add(ScreenId.GuestAccess, new GuestAccessScreen());
            _screensById.Add(ScreenId.Profile, new ProfileScreen(_session));
            _screensById.Add(ScreenId.AccountSettings, new AccountSettingsScreen());
            _screensById.Add(ScreenId.Rooms, new RoomsScreen());
            _screensById.Add(ScreenId.Match, new MatchScreen());
            _screensById.Add(ScreenId.GlobalRanking, new GlobalRankingScreen(_rankingChannelFactory, _session));
            _screensById.Add(ScreenId.CreateRoom, new CreateRoomScreen());
            _screensById.Add(ScreenId.Friends, new FriendsScreen());
            _screensById.Add(ScreenId.ReceivedRequests, new ReceivedRequestsScreen());
            _screensById.Add(ScreenId.Room, new RoomScreen());
            _screensById.Add(ScreenId.InvitePlayers, new InvitePlayersScreen());
            _screensById.Add(ScreenId.History, new HistoryScreen());
            _screensById.Add(ScreenId.MatchDetail, new MatchDetailScreen());
            _screensById.Add(ScreenId.MatchPreparation, new MatchPreparationScreen());
            _screensById.Add(ScreenId.InitialPlacement, new InitialPlacementScreen());
            _screensById.Add(ScreenId.RoundSummary, new RoundSummaryScreen());
            _screensById.Add(ScreenId.Result, new ResultScreen());
            _screensById.Add(ScreenId.Disconnection, new DisconnectionScreen());
            _screensById.Add(ScreenId.Resume, new ResumeScreen());
            _screensById.Add(ScreenId.MatchInProgress, new MatchInProgressScreen());
        }

        protected override void Update(GameTime gameTime)
        {
            Screen screen = _screensById[_openScreen];
            screen.Update(gameTime);
            Window.Title = LocalizedText.Get(TextKeys.Common.WindowTitle);

            KeyboardState keyboard = Keyboard.GetState();
            FollowLanguage();
            FollowFullScreenKey(keyboard);
            FollowNavigation(screen, keyboard);
            _previousKeyboard = keyboard;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Theme.Surface);
            _desktop?.Render();
            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            Window.TextInput -= WindowOnTextInput;
            Window.ClientSizeChanged -= WindowOnClientSizeChanged;
            base.UnloadContent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _statusChannelFactory.Abort();
                _accountChannelFactory.Abort();
                _rankingChannelFactory.Abort();
            }

            base.Dispose(disposing);
        }

        private void WindowOnTextInput(object? sender, TextInputEventArgs e)
        {
            _desktop?.OnChar(e.Character);
        }

        private void WindowOnClientSizeChanged(object? sender, EventArgs e)
        {
            if (_graphics.IsFullScreen)
            {
                return;
            }

            Rectangle client = Window.ClientBounds;
            _graphics.PreferredBackBufferWidth = client.Width;
            _graphics.PreferredBackBufferHeight = client.Height;
        }

        private void FollowFullScreenKey(KeyboardState keyboard)
        {
            if (keyboard.IsKeyDown(Keys.F11) && _previousKeyboard.IsKeyUp(Keys.F11))
            {
                ToggleFullScreen();
            }
        }

        private void ToggleFullScreen()
        {
            if (_graphics.IsFullScreen)
            {
                _graphics.PreferredBackBufferWidth = _windowedSize.X;
                _graphics.PreferredBackBufferHeight = _windowedSize.Y;
            }
            else
            {
                _windowedSize = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);
                DisplayMode display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                _graphics.PreferredBackBufferWidth = display.Width;
                _graphics.PreferredBackBufferHeight = display.Height;
            }

            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            _graphics.ApplyChanges();
        }

        private void FollowLanguage()
        {
            if (_languagePreference.Current.UiCultureName == _appliedUiCulture)
            {
                return;
            }

            _appliedUiCulture = _languagePreference.Current.UiCultureName;
            _mainMenu.FollowLanguage();
            if (_topBar is not null)
            {
                LanguageRefresher.Refresh(_topBar.Panel);
            }

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

        private void FollowNavigation(Screen screen, KeyboardState keyboard)
        {
            bool escapePressed = keyboard.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape);

            if (_mainMenu.WasExitRequested || _startup.WasExitRequested || escapePressed)
            {
                Exit();
                return;
            }

            if ((_topBar is not null) && _topBar.WasLogInRequested)
            {
                _topBar.ClearRequest();
                Show(ScreenId.Login);
            }

            if ((_topBar is not null) && _topBar.WasProfileRequested)
            {
                _topBar.ClearRequest();
                Show(ScreenId.Profile);
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

            _content?.Widgets.Clear();
            _content?.Widgets.Add(screen.Root);
            screen.Open();
            LanguageRefresher.Refresh(screen.Root);
            _topBar?.Follow(screen);
        }
    }
}
