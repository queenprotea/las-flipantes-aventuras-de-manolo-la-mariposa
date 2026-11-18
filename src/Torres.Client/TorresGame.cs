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

using Torres.Client.Avatars;
using Torres.Client.Localization;
using Torres.Client.Screens;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client
{
    internal sealed class TorresGame : Microsoft.Xna.Framework.Game
    {
        private const int FirstRoundNumber = 1;

        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private readonly LanguagePreference _languagePreference = new LanguagePreference();
        private readonly PlayerSession _playerSession = new PlayerSession();
        private readonly Dictionary<ScreenId, Screen> _screensById = new Dictionary<ScreenId, Screen>();
        private readonly MainMenuScreen _mainMenuScreen;
        private readonly StartupScreen _startupScreen;
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly ChannelFactory<IServerStatusService> _statusChannelFactory;
        private readonly ChannelFactory<IAccountService> _accountChannelFactory;
        private readonly ChannelFactory<IRankingService> _rankingChannelFactory;
        private readonly DoomLauncher _doomLauncher;

        private MatchScreen? _matchScreen;
        private DisconnectionScreen? _disconnectionScreen;
        private ResumeScreen? _resumeScreen;
        private RoomScreen? _roomScreen;
        private InitialPlacementScreen? _initialPlacementScreen;
        private Panel? _screenContainer;
        private Desktop? _desktop;
        private TopBarView? _topBarView;
        private LoginScreen? _loginScreen;
        private GuestAccessScreen? _guestAccessScreen;
        private ProfileScreen? _profileScreen;
        private AvatarCache? _avatarCache;
        private OwnProfileReader? _ownProfileReader;
        private ScreenId _openScreen = ScreenId.Startup;
        private bool _isMatchReturnPending;
        private string _appliedUiCultureName = string.Empty;
        private KeyboardState _previousKeyboardState;
        private Point _windowedClientSize = new Point(Sizes.WindowWidth, Sizes.WindowHeight);

        internal TorresGame()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            _graphicsDeviceManager.PreferredBackBufferWidth = Sizes.WindowWidth;
            _graphicsDeviceManager.PreferredBackBufferHeight = Sizes.WindowHeight;
            _graphicsDeviceManager.HardwareModeSwitch = false;

            _graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += WindowOnClientSizeChanged;

            ClientSettings clientSettings = ClientSettings.Load();
            _statusChannelFactory = new ChannelFactory<IServerStatusService>(
                new NetTcpBinding(SecurityMode.None),
                clientSettings.CreateEndpointAddress(clientSettings.ServicePaths.ServerStatus));
            _accountChannelFactory = new ChannelFactory<IAccountService>(
                new NetTcpBinding(SecurityMode.None),
                clientSettings.CreateEndpointAddress(clientSettings.ServicePaths.Account));
            _rankingChannelFactory = new ChannelFactory<IRankingService>(
                new NetTcpBinding(SecurityMode.None),
                clientSettings.CreateEndpointAddress(clientSettings.ServicePaths.Ranking));

            _gameSessionConnection = new GameSessionConnection(clientSettings);
            _doomLauncher = new DoomLauncher(clientSettings);
            _mainMenuScreen = new MainMenuScreen(_languagePreference, _gameSessionConnection, _playerSession);
            _startupScreen = new StartupScreen(_statusChannelFactory);
        }

        protected override void Initialize()
        {
            _languagePreference.LoadSavedPreference();
            _appliedUiCultureName = _languagePreference.CurrentLanguage.UiCultureName;
            MyraEnvironment.Game = this;
            WindowSizeLimit.ApplyMinimum(Window, Sizes.WindowWidth, Sizes.WindowHeight);

            var avatarCache = new AvatarCache(GraphicsDevice, _gameSessionConnection);
            _avatarCache = avatarCache;
            _ownProfileReader = new OwnProfileReader(_gameSessionConnection, _playerSession);
            RegisterScreens(avatarCache);

            _topBarView = new TopBarView(_playerSession, avatarCache);
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

        private void RegisterScreens(AvatarCache avatarCache)
        {
            _screensById.Add(ScreenId.Startup, _startupScreen);
            _screensById.Add(ScreenId.MainMenu, _mainMenuScreen);
            _screensById.Add(ScreenId.Language, new LanguageScreen(_languagePreference));
            _loginScreen = new LoginScreen(_accountChannelFactory, _gameSessionConnection, _playerSession);
            _screensById.Add(ScreenId.Login, _loginScreen);
            _screensById.Add(ScreenId.Register, new RegisterScreen(_accountChannelFactory));
            _screensById.Add(ScreenId.PasswordRecovery, new PasswordRecoveryScreen());
            _guestAccessScreen = new GuestAccessScreen(_accountChannelFactory, _gameSessionConnection, _playerSession);
            _screensById.Add(ScreenId.GuestAccess, _guestAccessScreen);
            _profileScreen = new ProfileScreen(_gameSessionConnection, _playerSession, avatarCache);
            _screensById.Add(ScreenId.Profile, _profileScreen);
            _screensById.Add(ScreenId.AccountSettings, new AccountSettingsScreen());
            _screensById.Add(ScreenId.Rooms, new RoomsScreen(_gameSessionConnection, _playerSession));
            _matchScreen = new MatchScreen(GraphicsDevice, _gameSessionConnection, _playerSession);
            _screensById.Add(ScreenId.Match, _matchScreen);
            _screensById.Add(ScreenId.GlobalRanking, new GlobalRankingScreen(_rankingChannelFactory, _playerSession));
            _screensById.Add(ScreenId.CreateRoom, new CreateRoomScreen(_gameSessionConnection, _playerSession));
            _screensById.Add(ScreenId.Friends, new FriendsScreen(_gameSessionConnection, _playerSession, avatarCache));
            _screensById.Add(ScreenId.Invitations, new InvitationsScreen(_gameSessionConnection, _playerSession, avatarCache));
            _roomScreen = new RoomScreen(_gameSessionConnection, _playerSession, avatarCache);
            _screensById.Add(ScreenId.Room, _roomScreen);
            _screensById.Add(ScreenId.InvitePlayers, new InvitePlayersScreen(_gameSessionConnection, _playerSession, avatarCache));
            _screensById.Add(ScreenId.History, new HistoryScreen(_gameSessionConnection, _playerSession));
            _screensById.Add(ScreenId.MatchDetail, new MatchDetailScreen(_gameSessionConnection, _playerSession));
            _screensById.Add(ScreenId.MatchPreparation, new MatchPreparationScreen(_playerSession));
            _initialPlacementScreen = new InitialPlacementScreen(GraphicsDevice, _gameSessionConnection, _playerSession, _doomLauncher);
            _screensById.Add(ScreenId.InitialPlacement, _initialPlacementScreen);
            _screensById.Add(ScreenId.RoundSummary, new RoundSummaryScreen(_playerSession));
            _screensById.Add(ScreenId.Result, new ResultScreen(_playerSession));
            _disconnectionScreen = new DisconnectionScreen(_gameSessionConnection, _playerSession);
            _screensById.Add(ScreenId.Disconnection, _disconnectionScreen);
            _resumeScreen = new ResumeScreen(_gameSessionConnection, _playerSession);
            _screensById.Add(ScreenId.Resume, _resumeScreen);
            _screensById.Add(ScreenId.MatchInProgress, new MatchInProgressScreen(_gameSessionConnection, _playerSession));
        }

        protected override void Update(GameTime gameTime)
        {
            _gameSessionConnection.CheckConnection(gameTime);
            FollowGameSession();
            _ownProfileReader?.Update();
            _avatarCache?.Update();
            _topBarView?.Update();
            Screen openScreen = _screensById[_openScreen];
            openScreen.Update(gameTime);
            Window.Title = LocalizedText.Get(TextKeys.Common.WindowTitle);

            KeyboardState keyboardState = Keyboard.GetState();
            FollowLanguageChange();
            FollowFullScreenKey(keyboardState);
            FollowNavigation(openScreen, keyboardState);
            FollowMatchReturn();
            _previousKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _screensById[_openScreen].RenderScene();
            GraphicsDevice.Clear(Theme.Surface);
            _desktop?.Render();
            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            Window.TextInput -= WindowOnTextInput;
            Window.ClientSizeChanged -= WindowOnClientSizeChanged;
            _matchScreen?.Dispose();
            _initialPlacementScreen?.Dispose();
            _avatarCache?.Dispose();
            base.UnloadContent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gameSessionConnection.CloseSessionAndFactory();
                _statusChannelFactory.Abort();
                _accountChannelFactory.Abort();
                _rankingChannelFactory.Abort();
            }

            base.Dispose(disposing);
        }

        private void FollowGameSession()
        {
            if (_gameSessionConnection.TakeTerminationReason() is not null)
            {
                EndGameSession(TextKeys.Login.SignedInElsewhere);
            }
            else if (_gameSessionConnection.WasConnectionLost && _playerSession.IsPlayingMatch)
            {
                WaitForReconnection();
            }
            else if (_gameSessionConnection.WasConnectionLost)
            {
                EndGameSession(TextKeys.Common.ServerErrorTitle);
            }
            else
            {
                TakeSessionNotices();
            }
        }

        private void TakeSessionNotices()
        {
            RoomChange? roomChange = _gameSessionConnection.TakeRoomChange();
            while (roomChange is not null)
            {
                ApplyRoomChange(roomChange);
                roomChange = _gameSessionConnection.TakeRoomChange();
            }

            MatchChange? matchChange = _gameSessionConnection.TakeMatchChange();
            while (matchChange is not null)
            {
                ApplyMatchChange(matchChange);
                matchChange = _gameSessionConnection.TakeMatchChange();
            }

            ChatMessage? chatMessage = _gameSessionConnection.TakeChatMessage();
            while (chatMessage is not null)
            {
                _playerSession.RoomChat.Add(chatMessage);
                chatMessage = _gameSessionConnection.TakeChatMessage();
            }

            RoomInvitation? roomInvitation = _gameSessionConnection.TakeRoomInvitation();
            while (roomInvitation is not null)
            {
                _playerSession.Invitations.Add(roomInvitation);
                roomInvitation = _gameSessionConnection.TakeRoomInvitation();
            }
        }

        private void WaitForReconnection()
        {
            _gameSessionConnection.AbortSessionChannel();
            _playerSession.LoseConnection(DateTime.UtcNow);
            _isMatchReturnPending = false;
            ShowScreen(ScreenId.Disconnection);
        }

        private void EndGameSession(string noticeKey)
        {
            _gameSessionConnection.AbandonSession();
            _playerSession.End();
            ShowScreen(ScreenId.Login);
            _loginScreen?.ShowNotice(noticeKey);
        }

        private void LogOut()
        {
            _gameSessionConnection.CloseSession();
            _playerSession.End();
            ShowScreen(ScreenId.Login);
        }

        private void ApplyRoomChange(RoomChange roomChange)
        {
            if (roomChange.Reason == RoomChangeReason.YouWereKicked)
            {
                _playerSession.LeaveRoom();
                ShowScreen(ScreenId.MainMenu);
                _mainMenuScreen.ShowNotice(TextKeys.Alerts.RemovedFromRoomTitle, TextKeys.Alerts.RemovedFromRoomBody, null);
            }
            else if (_playerSession.CurrentRoom is not null)
            {
                _playerSession.EnterRoom(roomChange.Room);
            }
        }

        private void ApplyMatchChange(MatchChange matchChange)
        {
            if (_playerSession.HasForfeitedMatch && (matchChange.Kind != MatchChangeKind.Started))
            {
                return;
            }

            _playerSession.ApplyMatchChange(matchChange);
            bool isFollowedByResumeScreen = (_openScreen == ScreenId.Resume) && (matchChange.MatchView.Phase == MatchPhase.AwaitingPlayers);
            if (matchChange.Kind == MatchChangeKind.Started)
            {
                ShowScreen(ScreenId.MatchPreparation);
            }
            else if (!IsMatchScreen(_openScreen) && !isFollowedByResumeScreen)
            {
                _isMatchReturnPending = true;
            }
        }

        private static bool IsMatchScreen(ScreenId screenId)
        {
            bool isMatchScreen = screenId is ScreenId.MatchPreparation or ScreenId.InitialPlacement or ScreenId.Match or ScreenId.RoundSummary or ScreenId.Result;

            return isMatchScreen;
        }

        private static ScreenId FindMatchScreen(MatchView matchView)
        {
            ScreenId matchScreen = matchView.Phase switch
            {
                MatchPhase.CaterpillarPlacement => ScreenId.InitialPlacement,
                MatchPhase.ButterflyPlacement when matchView.RoundNumber == FirstRoundNumber => ScreenId.InitialPlacement,
                MatchPhase.ButterflyPlacement => ScreenId.RoundSummary,
                MatchPhase.PlayerTurn => ScreenId.Match,
                MatchPhase.Finished => ScreenId.Result,
                MatchPhase.AwaitingPlayers => ScreenId.Resume,
                _ => throw new ArgumentOutOfRangeException(nameof(matchView)),
            };

            return matchScreen;
        }

        private void FollowMatchReturn()
        {
            bool canReturn = _isMatchReturnPending && _playerSession.IsLoggedIn && !IsMatchScreen(_openScreen) && (_openScreen is not ScreenId.Login and not ScreenId.GuestAccess);
            if (canReturn)
            {
                _isMatchReturnPending = false;
                ReturnToMatch();
            }
        }

        private void ReturnToMatch()
        {
            MatchView? matchView = _playerSession.CurrentMatchView;
            if ((matchView is not null) && PlayerSession.IsViewerRetired(matchView))
            {
                _playerSession.LeaveMatch();
                ShowRoomWithNotice(TextKeys.Room.RetiredFromMatchNotice);
            }
            else if (matchView is not null)
            {
                ShowScreen(FindMatchScreen(matchView));
            }
        }

        private void ShowRoomWithNotice(string noticeKey)
        {
            if (_playerSession.CurrentRoom is null)
            {
                ShowScreen(ScreenId.MainMenu);
                _mainMenuScreen.ShowTitleNotice(noticeKey);
            }
            else
            {
                ShowScreen(ScreenId.Room);
                _roomScreen?.ShowNotice(noticeKey);
            }
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

            FollowSessionRequests();

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

        private void FollowSessionRequests()
        {
            if ((_profileScreen is not null) && _profileScreen.WasLogOutRequested)
            {
                _profileScreen.ClearRequests();
                LogOut();
            }

            FollowDisconnectionRequests();
            FollowResumeRequests();

            if ((_guestAccessScreen is not null) && _guestAccessScreen.WasServerUnreachable)
            {
                _guestAccessScreen.ClearRequests();
                ShowScreen(ScreenId.Login);
                _loginScreen?.ShowNotice(TextKeys.Common.ServerErrorTitle);
            }
        }

        private void FollowDisconnectionRequests()
        {
            if (_disconnectionScreen is null)
            {
                return;
            }

            if (_disconnectionScreen.WasExitRequested)
            {
                _disconnectionScreen.ClearRequests();
                Exit();
            }
            else if (_disconnectionScreen.WasSessionRefused)
            {
                _disconnectionScreen.ClearRequests();
                EndGameSession(TextKeys.Common.ServerErrorTitle);
            }
            else if (_disconnectionScreen.HasMatchEndedWhileAway)
            {
                _disconnectionScreen.ClearRequests();
                ShowRoomWithNotice(TextKeys.Room.MatchEndedWhileAwayNotice);
            }
            else if (_disconnectionScreen.IsMatchAwaitingPlayers)
            {
                _disconnectionScreen.ClearRequests();
                ShowScreen(ScreenId.Resume);
            }
        }

        private void FollowResumeRequests()
        {
            if ((_resumeScreen is not null) && _resumeScreen.WasMatchLost)
            {
                _resumeScreen.ClearRequests();
                _playerSession.LeaveRoom();
                ShowRoomWithNotice(TextKeys.Room.MatchEndedWhileAwayNotice);
            }
        }

        private void ShowScreen(ScreenId screenId)
        {
            _screensById[_openScreen].Close();
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
