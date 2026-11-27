using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class OwnProfileFlowTests
    {
        private const string GetProfileCall = nameof(RecordingGameSessionConnection.GetProfileAsync);
        private const string ServerUsername = "ana_del_servidor";
        private const int OneRequest = 1;

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = new PlayerSession();
        private readonly OwnProfileFlow _ownProfileFlow;

        public OwnProfileFlowTests()
        {
            _playerSession.Start(new PlayerIdentity { PlayerId = SampleClients.AnaId, Username = SampleClients.AnaUsername });
            _gameSessionConnection.GetProfileReply = Task.FromResult(new ProfileResponse { Username = ServerUsername });
            _ownProfileFlow = new OwnProfileFlow(_gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_Follow_WithAnAccount_AppliesTheProfileOfTheServer()
        {
            _ownProfileFlow.Follow();

            Assert.Equal(ServerUsername, _playerSession.DisplayName);
        }

        [Fact]
        public void Test_Follow_AfterReadingTheProfile_DoesNotAskAgain()
        {
            _ownProfileFlow.Follow();

            _ownProfileFlow.Follow();

            int profileRequests = _gameSessionConnection.CalledMethods.Count(calledMethod => calledMethod == GetProfileCall);
            Assert.Equal(OneRequest, profileRequests);
        }

        [Fact]
        public void Test_Follow_AsAGuest_DoesNotAskForTheProfile()
        {
            _playerSession.StartAsGuest(SampleClients.GuestAlias);

            _ownProfileFlow.Follow();

            Assert.DoesNotContain(GetProfileCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_Follow_WithTheSessionClosed_DoesNotAskForTheProfile()
        {
            _gameSessionConnection.IsSessionOpen = false;

            _ownProfileFlow.Follow();

            Assert.DoesNotContain(GetProfileCall, _gameSessionConnection.CalledMethods);
        }
    }
}
