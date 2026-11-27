using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class LoginFlowSessionTests
    {
        private const string AbandonSessionCall = nameof(RecordingGameSessionConnection.AbandonSession);
        private const string CloseSessionCall = nameof(RecordingGameSessionConnection.CloseSession);
        private const string ReleaseChannelCall = nameof(RecordingAccountConnection.ReleaseChannel);

        private readonly RecordingAccountConnection _accountConnection = new RecordingAccountConnection();
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = new PlayerSession();
        private readonly LoginFlow _loginFlow;

        public LoginFlowSessionTests()
        {
            _accountConnection.LoginReply = Task.FromResult(new LoginResult { Status = LoginStatus.LoggedIn, Player = SampleClients.Ana, SessionToken = SampleClients.SessionToken });
            _gameSessionConnection.OpenReply = SampleClients.CreateOpenedSessionAsync();
            _loginFlow = new LoginFlow(_accountConnection, _gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheLoginComesWithoutToken_ShowsTheServerError()
        {
            _accountConnection.LoginReply = Task.FromResult(new LoginResult { Status = LoginStatus.LoggedIn, Player = SampleClients.Ana });
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            LoginOutcome? outcome = _loginFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.ErrorKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheDatabaseIsUnavailable_ShowsTheServerError()
        {
            _accountConnection.LoginReply = Task.FromResult(new LoginResult { Status = LoginStatus.DatabaseUnavailable });
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            LoginOutcome? outcome = _loginFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.ErrorKey);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheLoginAnswers_ReleasesTheAccountChannel()
        {
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            _loginFlow.TakeOutcome();

            Assert.Contains(ReleaseChannelCall, _accountConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheSessionIsRefused_ShowsTheServerError()
        {
            _gameSessionConnection.OpenReply = Task.FromResult(new OpenSessionResult { Status = OpenSessionStatus.InvalidSessionToken });
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            LoginOutcome? outcome = _loginFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.ErrorKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheSessionIsRefused_AbandonsTheSession()
        {
            _gameSessionConnection.OpenReply = Task.FromResult(new OpenSessionResult { Status = OpenSessionStatus.InvalidSessionToken });
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            _loginFlow.TakeOutcome();

            Assert.Contains(AbandonSessionCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_WhenOpeningTheSessionFails_ShowsTheServerError()
        {
            _gameSessionConnection.OpenReply = SampleClients.CreateFailedCallAsync<OpenSessionResult>();
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            LoginOutcome? outcome = _loginFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.ErrorKey);
        }

        [Fact]
        public void Test_TakeOutcome_WithAnOpenRoom_GoesToTheRoom()
        {
            _gameSessionConnection.OpenReply = Task.FromResult(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened, CurrentRoom = SampleClients.CreateRoom() });
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            LoginOutcome? outcome = _loginFlow.TakeOutcome();

            Assert.Equal(ScreenId.Room, outcome?.NextScreen);
        }

        [Fact]
        public void Test_TakeOutcome_WithAnOpenRoom_EntersTheRoom()
        {
            Room room = SampleClients.CreateRoom();
            _gameSessionConnection.OpenReply = Task.FromResult(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened, CurrentRoom = room });
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            _loginFlow.TakeOutcome();

            Assert.Same(room, _playerSession.CurrentRoom);
        }

        [Fact]
        public void Test_TakeOutcome_WithAMatchWithoutRoom_RemembersTheRoomCode()
        {
            _gameSessionConnection.OpenReply = Task.FromResult(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened, CurrentMatch = new MatchSeating { RoomCode = SampleClients.RoomCode } });
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            _loginFlow.TakeOutcome();

            Assert.Equal(SampleClients.RoomCode, _playerSession.MatchRoomCode);
        }

        [Fact]
        public void Test_TakeOutcome_LoggingInAfterPlayingAsGuest_ClosesTheGuestSession()
        {
            _playerSession.StartAsGuest(SampleClients.GuestAlias);
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            _loginFlow.TakeOutcome();

            Assert.Contains(CloseSessionCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_IsWaiting_AfterStartingTheLogin_ReturnsTrue()
        {
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            bool isWaiting = _loginFlow.IsWaiting;

            Assert.True(isWaiting);
        }

        [Fact]
        public void Test_CanEnterAsGuest_AsAGuest_ReturnsFalse()
        {
            _playerSession.StartAsGuest(SampleClients.GuestAlias);

            bool canEnterAsGuest = _loginFlow.CanEnterAsGuest;

            Assert.False(canEnterAsGuest);
        }
    }
}
