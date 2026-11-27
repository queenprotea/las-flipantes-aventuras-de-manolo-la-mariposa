using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class RoomsFlowJoinTests
    {
        private const string GetRoomsCall = nameof(RecordingGameSessionConnection.GetRoomsAsync);
        private const string FindRoomCall = nameof(RecordingGameSessionConnection.FindRoomAsync);
        private const string BlankRoomCode = "   ";

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly RoomsFlow _roomsFlow;

        public RoomsFlowJoinTests()
        {
            _roomsFlow = new RoomsFlow(_gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_Open_WithoutLoggingIn_RedirectsToTheLogin()
        {
            var roomsFlow = new RoomsFlow(_gameSessionConnection, new PlayerSession());

            ScreenId? redirect = roomsFlow.Open();

            Assert.Equal(ScreenId.Login, redirect);
        }

        [Fact]
        public void Test_Open_InsideARoom_RedirectsToTheRoom()
        {
            _playerSession.EnterRoom(SampleClients.CreateRoom());

            ScreenId? redirect = _roomsFlow.Open();

            Assert.Equal(ScreenId.Room, redirect);
        }

        [Fact]
        public void Test_Open_OutsideARoom_AsksForTheRooms()
        {
            _roomsFlow.Open();

            Assert.Contains(GetRoomsCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_Search_WithACode_AsksTheServerForThatRoom()
        {
            _roomsFlow.Search(SampleClients.RoomCode);

            Assert.Contains(FindRoomCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_Search_WithACode_ShowsTheSearch()
        {
            _roomsFlow.Search(SampleClients.RoomCode);

            bool isShowingSearch = _roomsFlow.IsShowingSearch;
            Assert.True(isShowingSearch);
        }

        [Fact]
        public void Test_Search_WithABlankCode_AsksForTheWholeList()
        {
            _roomsFlow.Search(BlankRoomCode);

            Assert.Contains(GetRoomsCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheListArrives_ShowsTheRooms()
        {
            var rooms = new List<RoomSummary>();
            _gameSessionConnection.GetRoomsReply = Task.FromResult(new RoomListResponse { Rooms = rooms });
            _roomsFlow.RequestRooms();

            RoomsOutcome? outcome = _roomsFlow.TakeOutcome();

            Assert.Same(rooms, outcome?.Rooms);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheListFails_ShowsTheServerError()
        {
            _gameSessionConnection.GetRoomsReply = SampleClients.CreateFailedCallAsync<RoomListResponse>();
            _roomsFlow.RequestRooms();

            RoomsOutcome? outcome = _roomsFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        [Fact]
        public void Test_StartJoin_WhileJoining_ReturnsFalse()
        {
            _gameSessionConnection.JoinRoomReply = new TaskCompletionSource<JoinRoomResult>().Task;
            _roomsFlow.StartJoin(SampleClients.RoomId);

            bool isStarted = _roomsFlow.StartJoin(SampleClients.RoomId);

            Assert.False(isStarted);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheRoomIsNotFound_ShowsTheRoomNotFoundNotice()
        {
            JoinWith(new JoinRoomResult { Status = JoinRoomStatus.RoomNotFound });

            RoomsOutcome? outcome = _roomsFlow.TakeOutcome();

            Assert.Equal(TextKeys.Alerts.RoomNotFoundTitle, outcome?.Notice?.TitleKey);
        }

        [Fact]
        public void Test_TakeOutcome_DuringAMatchOfTheRoom_ShowsRoomInMatch()
        {
            JoinWith(new JoinRoomResult { Status = JoinRoomStatus.RoomInMatch });

            RoomsOutcome? outcome = _roomsFlow.TakeOutcome();

            Assert.Equal(TextKeys.Rooms.RoomInMatchError, outcome?.Notice?.TitleKey);
        }

        [Fact]
        public void Test_TakeOutcome_AlreadyInsideARoom_ShowsAlreadyInRoom()
        {
            JoinWith(new JoinRoomResult { Status = JoinRoomStatus.AlreadyInRoom });

            RoomsOutcome? outcome = _roomsFlow.TakeOutcome();

            Assert.Equal(TextKeys.Rooms.AlreadyInRoomError, outcome?.Notice?.TitleKey);
        }

        [Fact]
        public void Test_TakeOutcome_WithABannedPlayer_ShowsTheBan()
        {
            JoinWith(new JoinRoomResult { Status = JoinRoomStatus.Banned });

            RoomsOutcome? outcome = _roomsFlow.TakeOutcome();

            bool? isBan = outcome?.Notice?.IsBan;
            Assert.True(isBan);
        }

        [Fact]
        public void Test_TakeOutcome_WithoutIdentity_GoesToTheLogin()
        {
            JoinWith(new JoinRoomResult { Status = JoinRoomStatus.NotIdentified });

            RoomsOutcome? outcome = _roomsFlow.TakeOutcome();

            Assert.Equal(ScreenId.Login, outcome?.NextScreen);
        }

        [Fact]
        public void Test_TakeOutcome_WhenJoiningFindsTheDatabaseUnavailable_ShowsTheServerError()
        {
            JoinWith(new JoinRoomResult { Status = JoinRoomStatus.DatabaseUnavailable });

            RoomsOutcome? outcome = _roomsFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheJoinComesWithoutRoom_ShowsTheServerError()
        {
            JoinWith(new JoinRoomResult { Status = JoinRoomStatus.Joined });

            RoomsOutcome? outcome = _roomsFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        private void JoinWith(JoinRoomResult joining)
        {
            _gameSessionConnection.JoinRoomReply = Task.FromResult(joining);
            _roomsFlow.StartJoin(SampleClients.RoomId);
        }
    }
}
