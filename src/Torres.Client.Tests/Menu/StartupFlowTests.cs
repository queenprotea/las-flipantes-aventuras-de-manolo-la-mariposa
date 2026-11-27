using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class StartupFlowTests
    {
        private const string GetStatusCall = nameof(RecordingServerStatusConnection.GetStatusAsync);
        private const int TwoChecks = 2;

        private readonly RecordingServerStatusConnection _statusConnection = new RecordingServerStatusConnection();
        private readonly StartupFlow _startupFlow;

        public StartupFlowTests()
        {
            _startupFlow = new StartupFlow(_statusConnection);
        }

        [Fact]
        public void Test_FollowCheck_WithTheServerReady_ReturnsReady()
        {
            _statusConnection.GetStatusReply = Task.FromResult(ServerStatus.Ready);

            ServerCheckState checkState = _startupFlow.FollowCheck();

            Assert.Equal(ServerCheckState.Ready, checkState);
        }

        [Fact]
        public void Test_FollowCheck_WithTheDatabaseUnavailable_ReturnsUnreachable()
        {
            _statusConnection.GetStatusReply = Task.FromResult(ServerStatus.DatabaseUnavailable);

            ServerCheckState checkState = _startupFlow.FollowCheck();

            Assert.Equal(ServerCheckState.Unreachable, checkState);
        }

        [Fact]
        public void Test_FollowCheck_WithTheServerUnreachable_ReturnsUnreachable()
        {
            _statusConnection.GetStatusReply = SampleClients.CreateFailedCallAsync<ServerStatus>();

            ServerCheckState checkState = _startupFlow.FollowCheck();

            Assert.Equal(ServerCheckState.Unreachable, checkState);
        }

        [Fact]
        public void Test_FollowCheck_WhileTheServerAnswers_ReturnsChecking()
        {
            _statusConnection.GetStatusReply = new TaskCompletionSource<ServerStatus>().Task;

            ServerCheckState checkState = _startupFlow.FollowCheck();

            Assert.Equal(ServerCheckState.Checking, checkState);
        }

        [Fact]
        public void Test_Retry_AfterAFailedCheck_ChecksAgain()
        {
            _statusConnection.GetStatusReply = SampleClients.CreateFailedCallAsync<ServerStatus>();
            _startupFlow.FollowCheck();
            _startupFlow.Retry();

            _startupFlow.FollowCheck();

            int statusChecks = _statusConnection.CalledMethods.Count(calledMethod => calledMethod == GetStatusCall);
            Assert.Equal(TwoChecks, statusChecks);
        }
    }
}
