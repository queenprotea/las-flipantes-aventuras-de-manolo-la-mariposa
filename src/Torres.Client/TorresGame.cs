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
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private readonly LanguagePreference _languagePreference = new LanguagePreference();
        private readonly PlayerSession _playerSession = new PlayerSession();
        private readonly Dictionary<ScreenId, Screen> _screensById = new Dictionary<ScreenId, Screen>();
        private readonly MainMenuScreen _mainMenuScreen;
        private readonly StartupScreen _startupScreen;
        private readonly ChannelFactory<IServerStatusService> _statusChannelFactory;
        private readonly ChannelFactory<IAccountService> _accountChannelFactory;
        private readonly ChannelFactory<IRankingService> _rankingChannelFactory;

        private Panel? _screenContainer;
        private Desktop? _desktop;
        private TopBarView? _topBarView;
        private ScreenId _openScreen = ScreenId.Startup;
        private string _appliedUiCultureName = string.Empty;
        private KeyboardState _previousKeyboardState;
        private Point _windowedClientSize = new Point(Sizes.WindowWidth, Sizes.WindowHeight);

        internal TorresGame()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            _graphicsDeviceManager.PreferredBackBufferWidth = Sizes.WindowWidth;
            _graphicsDeviceManager.PreferredBackBufferHeight = Sizes.WindowHeight;
            _graphicsDeviceManager.HardwareModeSwitch = false;
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += WindowOnClientSizeChanged;

            ClientSettings clientSettings = ClientSettings.Load();
            _statusChannelFactory = new ChannelFactory<IServerStatusService>(
                new NetTcpBinding(SecurityMode.None),
                clientSettings.CreateEndpointAddress(ServiceEndpointPaths.ServerStatus));
            _accountChannelFactory = new ChannelFactory<IAccountService>(
                new NetTcpBinding(SecurityMode.None),
                clientSettings.CreateEndpointAddress(ServiceEndpointPaths.Account));
            _rankingChannelFactory = new ChannelFactory<IRankingService>(
                new NetTcpBinding(SecurityMode.None),
                clientSettings.CreateEndpointAddress(ServiceEndpointPaths.Ranking));

            _mainMenuScreen = new MainMenuScreen(_languagePreference);
            _startupScreen = new StartupScreen(_statusChannelFactory);
        }

        protected override void Initialize()
        {
            _languagePreference.LoadSavedPreference();
            _appliedUiCultureName = _languagePreference.CurrentLanguage.UiCultureName;
            MyraEnvironment.Game = this;
            WindowSizeLimit.ApplyMinimum(Window, Sizes.WindowWidth, Sizes.WindowHeight);

            RegisterScreens();

            _topBarView = new TopBarView(_playerSession);
            _screenContainer = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var layoutRoot = new VerticalStackPanel
            {
                Padding = new Thickness(Metrics.ScreenPaddingX, Metrics.ScreenPaddingY),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            layoutRoot.Widgets.Add(_topBarView.Root);
            layoutRoot.Widgets.Add(_screenContainer);
            StackPanel.SetProportionType(_screenContainer, ProportionType.Fill);

            _desktop = new Desktop();
            _desktop.HasExternalTextInput = true;
            _desktop.Root = layoutRoot;
            Window.TextInput += WindowOnTextInput;
            ShowScreen(ScreenId.Startup);
            base.Initialize();
        }

        private void RegisterScreens()
        {
            _screensById.Add(ScreenId.Startup, _startupScreen);
            _screensById.Add(ScreenId.MainMenu, _mainMenuScreen);
            _screensById.Add(ScreenId.Language, new LanguageScreen(_languagePreference));
            _screensById.Add(ScreenId.Login, new LoginScreen(_accountChannelFactory, _playerSession));
            _screensById.Add(ScreenId.Register, new RegisterScreen(_accountChannelFactory));
            _screensById.Add(ScreenId.PasswordRecovery, new PasswordRecoveryScreen());
            _screensById.Add(ScreenId.GuestAccess, new GuestAccessScreen());
            _screensById.Add(ScreenId.Profile, new ProfileScreen(_playerSession));
            _screensById.Add(ScreenId.AccountSettings, new AccountSettingsScreen());
            _screensById.Add(ScreenId.Rooms, new RoomsScreen());
            _screensById.Add(ScreenId.Match, new MatchScreen());
            _screensById.Add(ScreenId.GlobalRanking, new GlobalRankingScreen(_rankingChannelFactory, _playerSession));
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
            Screen openScreen = _screensById[_openScreen];
            openScreen.Update(gameTime);
            Window.Title = LocalizedText.Get(TextKeys.Common.WindowTitle);

            KeyboardState keyboardState = Keyboard.GetState();
            FollowLanguageChange();
            FollowFullScreenKey(keyboardState);
            FollowNavigation(openScreen, keyboardState);
            _previousKeyboardState = keyboardState;
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
            if (_graphicsDeviceManager.IsFullScreen)
            {
                return;
            }

            Rectangle clientBounds = Window.ClientBounds;
            _graphicsDeviceManager.PreferredBackBufferWidth = clientBounds.Width;
            _graphicsDeviceManager.PreferredBackBufferHeight = clientBounds.Height;
        }

        private void FollowFullScreenKey(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.F11) && _previousKeyboardState.IsKeyUp(Keys.F11))
            {
                ToggleFullScreen();
            }
        }

        private void ToggleFullScreen()
        {
            if (_graphicsDeviceManager.IsFullScreen)
            {
                _graphicsDeviceManager.PreferredBackBufferWidth = _windowedClientSize.X;
                _graphicsDeviceManager.PreferredBackBufferHeight = _windowedClientSize.Y;
            }
            else
            {
                _windowedClientSize = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);
                DisplayMode displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                _graphicsDeviceManager.PreferredBackBufferWidth = displayMode.Width;
                _graphicsDeviceManager.PreferredBackBufferHeight = displayMode.Height;
            }

            _graphicsDeviceManager.IsFullScreen = !_graphicsDeviceManager.IsFullScreen;
            _graphicsDeviceManager.ApplyChanges();
        }

        private void FollowLanguageChange()
        {
            if (_languagePreference.CurrentLanguage.UiCultureName == _appliedUiCultureName)
            {
                return;
            }

            _appliedUiCultureName = _languagePreference.CurrentLanguage.UiCultureName;
            _mainMenuScreen.FollowLanguage();
            if (_topBarView is not null)
            {
                LanguageRefresher.Refresh(_topBarView.Root);
            }

            foreach (Screen registeredScreen in _screensById.Values)
            {
                RefreshIfBuilt(registeredScreen);
            }
        }

        private static void RefreshIfBuilt(Screen registeredScreen)
        {
            if (!registeredScreen.IsBuilt)
            {
                return;
            }

            LanguageRefresher.Refresh(registeredScreen.Root);
        }

        private void FollowNavigation(Screen openScreen, KeyboardState keyboardState)
        {
            bool isEscapePressed = keyboardState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape);

            if (_mainMenuScreen.WasExitRequested || _startupScreen.WasExitRequested || isEscapePressed)
            {
                Exit();
                return;
            }

            if ((_topBarView is not null) && _topBarView.WasLogInRequested)
            {
                _topBarView.ClearRequests();
                ShowScreen(ScreenId.Login);
            }

            if ((_topBarView is not null) && _topBarView.WasProfileRequested)
            {
                _topBarView.ClearRequests();
                ShowScreen(ScreenId.Profile);
            }

            if (openScreen.RequestedScreen is ScreenId requestedScreen)
            {
                openScreen.RequestedScreen = null;
                ShowScreen(requestedScreen);
            }
        }

        private void ShowScreen(ScreenId screenId)
        {
            _openScreen = screenId;
            Screen selectedScreen = _screensById[screenId];

            _screenContainer?.Widgets.Clear();
            _screenContainer?.Widgets.Add(selectedScreen.Root);
            selectedScreen.Open();
            LanguageRefresher.Refresh(selectedScreen.Root);
            _topBarView?.FollowScreen(selectedScreen);
        }
    }
}
