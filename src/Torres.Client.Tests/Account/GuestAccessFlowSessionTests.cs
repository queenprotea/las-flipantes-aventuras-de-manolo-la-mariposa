using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class GuestAccessFlowSessionTests
    {
        private const string AbandonSessionCall = nameof(RecordingGameSessionConnection.AbandonSession);
        private const string GivenAlias = "Lupe";

        private readonly RecordingAccountConnection _accountConnection = new RecordingAccountConnection();
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = new PlayerSession();
        private readonly GuestAccessFlow _guestAccessFlow;

        public GuestAccessFlowSessionTests()
        {
            _accountConnection.EnterAsGuestReply = Task.FromResult(new GuestAccessResult { Status = GuestAccessStatus.Entered, SessionToken = SampleClients.SessionToken });
            _gameSessionConnection.OpenReply = SampleClients.CreateOpenedSessionAsync();
            _guestAccessFlow = new GuestAccessFlow(_accountConnection, _gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheAliasIsTooLong_ReturnsRequiredField()
        {
            _accountConnection.EnterAsGuestReply = Task.FromResult(new GuestAccessResult { Status = GuestAccessStatus.AliasTooLong });
            _guestAccessFlow.StartGuestAccess(SampleClients.GuestAlias);

            GuestAccessOutcome? outcome = _guestAccessFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.RequiredField, outcome?.ErrorKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheEntryComesWithoutToken_ReportsTheServerUnreachable()
        {
            _accountConnection.EnterAsGuestReply = Task.FromResult(new GuestAccessResult { Status = GuestAccessStatus.Entered });
            _guestAccessFlow.StartGuestAccess(SampleClients.GuestAlias);

            GuestAccessOutcome? outcome = _guestAccessFlow.TakeOutcome();

            bool? isServerUnreachable = outcome?.IsServerUnreachable;
            Assert.True(isServerUnreachable);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheSessionIsRefused_AbandonsTheSession()
        {
            _gameSessionConnection.OpenReply = Task.FromResult(new OpenSessionResult { Status = OpenSessionStatus.InvalidSessionToken });
            _guestAccessFlow.StartGuestAccess(SampleClients.GuestAlias);

            _guestAccessFlow.TakeOutcome();

            Assert.Contains(AbandonSessionCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_WithAnOpenRoom_GoesToTheRoom()
        {
            _gameSessionConnection.OpenReply = Task.FromResult(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened, CurrentRoom = SampleClients.CreateRoom() });
            _guestAccessFlow.StartGuestAccess(SampleClients.GuestAlias);

            GuestAccessOutcome? outcome = _guestAccessFlow.TakeOutcome();

            Assert.Equal(ScreenId.Room, outcome?.NextScreen);
        }

        [Fact]
        public void Test_TakeOutcome_WithTheAliasGivenByTheServer_UsesThatAlias()
        {
            _gameSessionConnection.OpenReply = Task.FromResult(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened, Identity = new SessionIdentity { DisplayName = GivenAlias, IsGuest = true } });
            _guestAccessFlow.StartGuestAccess(SampleClients.GuestAlias);

            _guestAccessFlow.TakeOutcome();

            Assert.Equal(GivenAlias, _playerSession.DisplayName);
        }

        [Fact]
        public void Test_IsWaiting_AfterStartingTheEntry_ReturnsTrue()
        {
            _guestAccessFlow.StartGuestAccess(SampleClients.GuestAlias);

            bool isWaiting = _guestAccessFlow.IsWaiting;

            Assert.True(isWaiting);
        }
    }
}
