using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class LoginFlowTests
    {
        private const string BlankUsername = "   ";

        private readonly RecordingAccountConnection _accountConnection = new RecordingAccountConnection();
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly LoginFlow _loginFlow;

        public LoginFlowTests()
        {
            _loginFlow = new LoginFlow(_accountConnection, _gameSessionConnection, new PlayerSession());
            _accountConnection.LoginReply = Task.FromResult(new LoginResult { Status = LoginStatus.LoggedIn, Player = SampleClients.Ana, SessionToken = SampleClients.SessionToken });
            _gameSessionConnection.OpenReply = SampleClients.CreateOpenedSessionAsync();
        }

        [Fact]
        public void Test_TakeOutcome_AfterCorrectCredentials_GoesToTheMainMenu()
        {
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            LoginOutcome? outcome = _loginFlow.TakeOutcome();

            Assert.Equal(ScreenId.MainMenu, outcome?.NextScreen);
        }

        [Fact]
        public void Test_StartLogin_WithEmptyFields_ReturnsRequiredField()
        {
            string? errorKey = _loginFlow.StartLogin(BlankUsername, string.Empty);

            Assert.Equal(TextKeys.Common.RequiredField, errorKey);
        }

        [Fact]
        public void Test_StartLogin_WithEmptyFields_DoesNotCallTheServer()
        {
            _loginFlow.StartLogin(BlankUsername, string.Empty);

            Assert.Empty(_accountConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_AfterWrongCredentials_ShowsInvalidCredentials()
        {
            _accountConnection.LoginReply = Task.FromResult(new LoginResult { Status = LoginStatus.InvalidCredentials });
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            LoginOutcome? outcome = _loginFlow.TakeOutcome();

            Assert.Equal(TextKeys.Login.InvalidCredentials, outcome?.ErrorKey);
        }

        [Fact]
        public void Test_TakeOutcome_WithPermanentBan_ReportsTheBan()
        {
            _accountConnection.LoginReply = Task.FromResult(new LoginResult { Status = LoginStatus.AccountBanned });
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            LoginOutcome? outcome = _loginFlow.TakeOutcome();

            Assert.True(outcome?.IsBanned);
        }

        [Fact]
        public void Test_TakeOutcome_WithServerUnreachable_ShowsTheServerError()
        {
            _accountConnection.LoginReply = SampleClients.CreateFailedCallAsync<LoginResult>();
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            LoginOutcome? outcome = _loginFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.ErrorKey);
        }

        [Fact]
        public void Test_TakeOutcome_WithAMatchAwaitingPlayers_GoesToTheMatchInProgress()
        {
            var awaitingMatch = new MatchSeating { RoomCode = SampleClients.RoomCode, IsAwaitingPlayers = true };
            _gameSessionConnection.OpenReply = Task.FromResult(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened, CurrentMatch = awaitingMatch });
            _loginFlow.StartLogin(SampleClients.AnaUsername, SampleClients.Password);

            LoginOutcome? outcome = _loginFlow.TakeOutcome();

            Assert.Equal(ScreenId.MatchInProgress, outcome?.NextScreen);
        }

        [Fact]
        public void Test_FindScreenToCreateAccount_FromTheLogin_ReturnsRegister()
        {
            ScreenId nextScreen = LoginFlow.FindScreenToCreateAccount();

            Assert.Equal(ScreenId.Register, nextScreen);
        }

        [Fact]
        public void Test_FindScreenToRecoverAccess_FromTheLogin_ReturnsPasswordRecovery()
        {
            ScreenId nextScreen = LoginFlow.FindScreenToRecoverAccess();

            Assert.Equal(ScreenId.PasswordRecovery, nextScreen);
        }

        [Fact]
        public void Test_FindScreenToPlayAsGuest_FromTheLogin_ReturnsGuestAccess()
        {
            ScreenId nextScreen = LoginFlow.FindScreenToPlayAsGuest();

            Assert.Equal(ScreenId.GuestAccess, nextScreen);
        }

        [Fact]
        public void Test_FindScreenToGoBack_FromTheLogin_ReturnsMainMenu()
        {
            ScreenId nextScreen = LoginFlow.FindScreenToGoBack();

            Assert.Equal(ScreenId.MainMenu, nextScreen);
        }
    }
}
