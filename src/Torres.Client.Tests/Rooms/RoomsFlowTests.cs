using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class RoomsFlowTests
    {
        private const string GetRoomsCall = nameof(RecordingGameSessionConnection.GetRoomsAsync);

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly RoomsFlow _roomsFlow;

        public RoomsFlowTests()
        {
            _roomsFlow = new RoomsFlow(_gameSessionConnection, SampleClients.CreateSessionOfAna());
        }

        [Fact]
        public void Test_TakeOutcome_AfterJoiningFromTheList_GoesToTheRoom()
        {
            _gameSessionConnection.JoinRoomReply = Task.FromResult(new JoinRoomResult { Status = JoinRoomStatus.Joined, Room = SampleClients.CreateRoom() });
            _roomsFlow.StartJoin(SampleClients.RoomId);

            RoomsOutcome? outcome = _roomsFlow.TakeOutcome();

            Assert.Equal(ScreenId.Room, outcome?.NextScreen);
        }

        [Fact]
        public void Test_TakeOutcome_ToAFullRoom_ShowsTheRoomFullNotice()
        {
            _gameSessionConnection.JoinRoomReply = Task.FromResult(new JoinRoomResult { Status = JoinRoomStatus.RoomFull });
            _roomsFlow.StartJoin(SampleClients.RoomId);

            RoomsOutcome? outcome = _roomsFlow.TakeOutcome();

            Assert.Equal(TextKeys.Alerts.RoomFullTitle, outcome?.Notice?.TitleKey);
        }

        [Fact]
        public void Test_RequestRooms_FromTheList_AsksTheServerAgain()
        {
            _roomsFlow.RequestRooms();

            Assert.Contains(GetRoomsCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_WhenJoiningFails_ShowsTheServerError()
        {
            _gameSessionConnection.JoinRoomReply = SampleClients.CreateFailedCallAsync<JoinRoomResult>();
            _roomsFlow.StartJoin(SampleClients.RoomId);

            RoomsOutcome? outcome = _roomsFlow.TakeOutcome();

            Assert.Equal(Notice.CreateServerError(), outcome?.Notice);
        }

        [Fact]
        public void Test_FindScreenToCreateRoom_FromTheRooms_ReturnsCreateRoom()
        {
            ScreenId nextScreen = RoomsFlow.FindScreenToCreateRoom();

            Assert.Equal(ScreenId.CreateRoom, nextScreen);
        }

        [Fact]
        public void Test_FindScreenToGoBack_FromTheRooms_ReturnsMainMenu()
        {
            ScreenId nextScreen = RoomsFlow.FindScreenToGoBack();

            Assert.Equal(ScreenId.MainMenu, nextScreen);
        }
    }
}
