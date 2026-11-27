using Game.Contracts;

using Torres.Client.Flows;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class DisconnectionFlowTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly DisconnectionFlow _disconnectionFlow;

        public DisconnectionFlowTests()
        {
            _disconnectionFlow = new DisconnectionFlow(_gameSessionConnection, SampleClients.CreateSessionOfAna());
        }

        [Fact]
        public void Test_TakeOutcome_WhenReconnectingFails_ShowsTheServerError()
        {
            _gameSessionConnection.ReopenReply = SampleClients.CreateFailedCallAsync<OpenSessionResult>();
            _disconnectionFlow.Retry();

            DisconnectionOutcome? outcome = _disconnectionFlow.TakeOutcome();

            Assert.Equal(Notice.CreateServerError(), outcome?.Notice);
        }
    }
}
