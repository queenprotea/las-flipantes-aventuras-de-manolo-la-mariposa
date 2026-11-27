using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class GuestAccessFlowTests
    {
        private const string BlankAlias = "   ";

        private readonly RecordingAccountConnection _accountConnection = new RecordingAccountConnection();
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly GuestAccessFlow _guestAccessFlow;

        public GuestAccessFlowTests()
        {
            _guestAccessFlow = new GuestAccessFlow(_accountConnection, _gameSessionConnection, new PlayerSession());
            _accountConnection.EnterAsGuestReply = Task.FromResult(new GuestAccessResult { Status = GuestAccessStatus.Entered, SessionToken = SampleClients.SessionToken });
            _gameSessionConnection.OpenReply = SampleClients.CreateOpenedSessionAsync();
        }

        [Fact]
        public void Test_TakeOutcome_AfterEntering_GoesToTheMainMenu()
        {
            _guestAccessFlow.StartGuestAccess(SampleClients.GuestAlias);

            GuestAccessOutcome? outcome = _guestAccessFlow.TakeOutcome();

            Assert.Equal(ScreenId.MainMenu, outcome?.NextScreen);
        }

        [Fact]
        public void Test_StartGuestAccess_WithBlankAlias_ReturnsRequiredField()
        {
            string? errorKey = _guestAccessFlow.StartGuestAccess(BlankAlias);

            Assert.Equal(TextKeys.Common.RequiredField, errorKey);
        }

        [Fact]
        public void Test_TakeOutcome_WithServerUnreachable_ReportsIt()
        {
            _accountConnection.EnterAsGuestReply = SampleClients.CreateFailedCallAsync<GuestAccessResult>();
            _guestAccessFlow.StartGuestAccess(SampleClients.GuestAlias);

            GuestAccessOutcome? outcome = _guestAccessFlow.TakeOutcome();

            Assert.True(outcome?.IsServerUnreachable);
        }

        [Fact]
        public void Test_FindScreenToCancel_FromTheGuestAccess_ReturnsLogin()
        {
            ScreenId nextScreen = GuestAccessFlow.FindScreenToCancel();

            Assert.Equal(ScreenId.Login, nextScreen);
        }
    }
}
