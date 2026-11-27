using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class ForfeitFlowResultTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly ForfeitFlow _forfeitFlow;

        public ForfeitFlowResultTests()
        {
            _forfeitFlow = new ForfeitFlow(_gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_TakeOutcome_AfterForfeitingInsideARoom_GoesBackToTheRoom()
        {
            _gameSessionConnection.ForfeitReply = Task.FromResult(ForfeitResult.Forfeited);
            _playerSession.EnterRoom(SampleClients.CreateRoom());
            _forfeitFlow.Forfeit();

            ScreenOutcome? outcome = _forfeitFlow.TakeOutcome();

            Assert.Equal(ScreenId.Room, outcome?.NextScreen);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheServerDoesNotKnowThePlayer_ShowsTheServerError()
        {
            _gameSessionConnection.ForfeitReply = Task.FromResult(ForfeitResult.NotIdentified);
            _forfeitFlow.Forfeit();

            ScreenOutcome? outcome = _forfeitFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        [Fact]
        public void Test_IsForfeiting_AfterAskingToForfeit_ReturnsTrue()
        {
            _forfeitFlow.Forfeit();

            bool isForfeiting = _forfeitFlow.IsForfeiting;

            Assert.True(isForfeiting);
        }

        [Fact]
        public void Test_Forget_WhileForfeiting_StopsWaiting()
        {
            _forfeitFlow.Forfeit();

            _forfeitFlow.Forget();

            Assert.False(_forfeitFlow.IsForfeiting);
        }
    }
}
