using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class CreateRoomFlowTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly CreateRoomFlow _createRoomFlow;

        public CreateRoomFlowTests()
        {
            _createRoomFlow = new CreateRoomFlow(_gameSessionConnection, SampleClients.CreateSessionOfAna());
        }

        [Fact]
        public void Test_TakeOutcome_AfterCreatingTheRoom_GoesToTheRoom()
        {
            _gameSessionConnection.CreateRoomReply = Task.FromResult(new CreateRoomResult { Status = CreateRoomStatus.Created, Room = SampleClients.CreateRoom() });
            _createRoomFlow.StartCreation(true);

            ScreenOutcome? outcome = _createRoomFlow.TakeOutcome();

            Assert.Equal(ScreenId.Room, outcome?.NextScreen);
        }

        [Fact]
        public void Test_TakeOutcome_WhenCreatingFails_ShowsTheServerError()
        {
            _gameSessionConnection.CreateRoomReply = SampleClients.CreateFailedCallAsync<CreateRoomResult>();
            _createRoomFlow.StartCreation(true);

            ScreenOutcome? outcome = _createRoomFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        [Fact]
        public void Test_FindScreenToCancel_FromTheRoomCreation_ReturnsRooms()
        {
            ScreenId nextScreen = CreateRoomFlow.FindScreenToCancel();

            Assert.Equal(ScreenId.Rooms, nextScreen);
        }
    }
}
