using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class MatchInProgressFlowAbandonTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly MatchInProgressFlow _inProgressFlow;

        public MatchInProgressFlowAbandonTests()
        {
            _inProgressFlow = new MatchInProgressFlow(_gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_MatchRoomCode_WithoutMatch_ReturnsAnEmptyCode()
        {
            string matchRoomCode = _inProgressFlow.MatchRoomCode;

            Assert.Equal(string.Empty, matchRoomCode);
        }

        [Fact]
        public void Test_MatchRoomCode_OfAMatchWithoutRoom_ReturnsItsCode()
        {
            _playerSession.EnterMatchWithoutRoom(SampleClients.RoomCode);

            string matchRoomCode = _inProgressFlow.MatchRoomCode;

            Assert.Equal(SampleClients.RoomCode, matchRoomCode);
        }

        [Fact]
        public void Test_IsAbandoning_AfterAbandoning_ReturnsTrue()
        {
            _inProgressFlow.Abandon();

            bool isAbandoning = _inProgressFlow.IsAbandoning;

            Assert.True(isAbandoning);
        }

        [Fact]
        public void Test_Open_WhileAbandoning_StopsWaiting()
        {
            _inProgressFlow.Abandon();

            _inProgressFlow.Open();

            Assert.False(_inProgressFlow.IsAbandoning);
        }

        [Fact]
        public void Test_TakeOutcome_WhenAbandoningFails_StaysOnTheScreen()
        {
            _gameSessionConnection.ForfeitReply = SampleClients.CreateFailedCallAsync<ForfeitResult>();
            _inProgressFlow.Abandon();

            ScreenOutcome? outcome = _inProgressFlow.TakeOutcome();

            Assert.Null(outcome?.NextScreen);
        }
    }
}
