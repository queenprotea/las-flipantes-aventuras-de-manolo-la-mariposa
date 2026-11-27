using System.Collections.Generic;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class RoomFlowMatchStartTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly RoomFlow _roomFlow;

        public RoomFlowMatchStartTests()
        {
            _roomFlow = new RoomFlow(_gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_TakeOutcome_AfterStartingTheMatch_ReturnsNull()
        {
            StartMatchWith(StartMatchStatus.Started);

            RoomOutcome? outcome = _roomFlow.TakeOutcome();

            Assert.Null(outcome);
        }

        [Fact]
        public void Test_TakeOutcome_WithABannedPlayer_ReportsTheBannedPlayer()
        {
            var bannedStart = new StartMatchResult { Status = StartMatchStatus.PlayerBanned };
            _gameSessionConnection.StartMatchReply = Task.FromResult(bannedStart);
            _roomFlow.StartMatch();

            RoomOutcome? outcome = _roomFlow.TakeOutcome();

            Assert.Same(bannedStart, outcome?.BannedPlayer);
        }

        [Fact]
        public void Test_TakeOutcome_WithNotEnoughPlayers_ShowsItsNotice()
        {
            StartMatchWith(StartMatchStatus.NotEnoughPlayers);

            RoomOutcome? outcome = _roomFlow.TakeOutcome();

            Assert.Equal(TextKeys.Room.NotEnoughPlayersNotice, outcome?.Notice?.TitleKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenAMemberTriesToStart_ShowsTheHostOnlyHint()
        {
            StartMatchWith(StartMatchStatus.NotHost);

            RoomOutcome? outcome = _roomFlow.TakeOutcome();

            Assert.Equal(TextKeys.Room.HostOnlyHint, outcome?.Notice?.TitleKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenStartingOutsideTheRoom_ShowsTheServerError()
        {
            StartMatchWith(StartMatchStatus.NotInRoom);

            RoomOutcome? outcome = _roomFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        [Fact]
        public void Test_TakeOutcome_WhenLeavingDuringAMatch_ShowsTheLeaveNotice()
        {
            _gameSessionConnection.LeaveRoomReply = Task.FromResult(LeaveRoomResult.RoomInMatch);
            _roomFlow.Leave();

            RoomOutcome? outcome = _roomFlow.TakeOutcome();

            Assert.Equal(TextKeys.Room.LeaveDuringMatchNotice, outcome?.Notice?.TitleKey);
        }

        [Fact]
        public void Test_TakeOutcome_AfterKickingAMember_ReturnsNull()
        {
            _gameSessionConnection.KickMemberReply = Task.FromResult(KickMemberResult.Kicked);
            _roomFlow.Kick(SampleClients.BetoId);

            RoomOutcome? outcome = _roomFlow.TakeOutcome();

            Assert.Null(outcome);
        }

        [Fact]
        public void Test_FindRedirect_OutsideARoom_GoesToTheMainMenu()
        {
            ScreenId? redirect = _roomFlow.FindRedirect();

            Assert.Equal(ScreenId.MainMenu, redirect);
        }

        [Fact]
        public void Test_FindRedirect_InsideARoom_StaysInTheRoom()
        {
            _playerSession.EnterRoom(SampleClients.CreateRoom());

            ScreenId? redirect = _roomFlow.FindRedirect();

            Assert.Null(redirect);
        }

        [Fact]
        public void Test_NeedsLeaveConfirmation_AsTheOnlyMember_ReturnsTrue()
        {
            Room room = SampleClients.CreateRoom();
            room.Members = new List<RoomMember> { new RoomMember { MemberId = SampleClients.AnaId } };
            _playerSession.EnterRoom(room);

            bool needsConfirmation = _roomFlow.NeedsLeaveConfirmation;

            Assert.True(needsConfirmation);
        }

        [Fact]
        public void Test_HasAccount_WithAnAccount_ReturnsTrue()
        {
            bool hasAccount = _roomFlow.HasAccount;

            Assert.True(hasAccount);
        }

        [Fact]
        public void Test_CurrentRoom_InsideARoom_ReturnsTheRoom()
        {
            Room room = SampleClients.CreateRoom();
            _playerSession.EnterRoom(room);

            Room? currentRoom = _roomFlow.CurrentRoom;

            Assert.Same(room, currentRoom);
        }

        private void StartMatchWith(StartMatchStatus startStatus)
        {
            _gameSessionConnection.StartMatchReply = Task.FromResult(new StartMatchResult { Status = startStatus });
            _roomFlow.StartMatch();
        }
    }
}
