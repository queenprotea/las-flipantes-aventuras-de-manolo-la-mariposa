using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class InitialPlacementFlowTests
    {
        private static readonly MatchCell _flowerCell = new MatchCell { Row = 1, Column = 1 };

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly InitialPlacementFlow _initialPlacementFlow;

        public InitialPlacementFlowTests()
        {
            PlayerSession playerSession = SampleClients.CreateSessionOfAna();
            playerSession.ApplyMatchChange(SampleClients.CreatePlacementChange());
            _initialPlacementFlow = new InitialPlacementFlow(_gameSessionConnection, playerSession);
        }

        [Fact]
        public void Test_TakeNotice_WhenPlacingFails_ShowsTheServerError()
        {
            _gameSessionConnection.PlacePieceReply = SampleClients.CreateFailedCallAsync<MatchActionResult>();
            _initialPlacementFlow.Place(_flowerCell);

            Notice? notice = _initialPlacementFlow.TakeNotice();

            Assert.Equal(Notice.CreateServerError(), notice);
        }
    }
}
