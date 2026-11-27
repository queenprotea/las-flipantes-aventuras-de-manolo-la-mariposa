using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class RoomFlowTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly RoomFlow _roomFlow;

        public RoomFlowTests()
        {
            PlayerSession playerSession = SampleClients.CreateSessionOfAna();
            playerSession.EnterRoom(SampleClients.CreateRoom());
            _roomFlow = new RoomFlow(_gameSessionConnection, playerSession);
        }

        [Fact]
        public void Test_TakeOutcome_AfterLeaving_GoesToTheMainMenu()
        {
            _gameSessionConnection.LeaveRoomReply = Task.FromResult(LeaveRoomResult.Left);
            _roomFlow.Leave();

            RoomOutcome? outcome = _roomFlow.TakeOutcome();

            Assert.Equal(ScreenId.MainMenu, outcome?.NextScreen);
        }

        [Fact]
        public void Test_TakeOutcome_WhenLeavingFails_ShowsTheServerError()
        {
            _gameSessionConnection.LeaveRoomReply = SampleClients.CreateFailedCallAsync<LeaveRoomResult>();
            _roomFlow.Leave();

            RoomOutcome? outcome = _roomFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        [Fact]
        public void Test_TakeOutcome_WhenStartingTheMatchFails_ShowsTheServerError()
        {
            _gameSessionConnection.StartMatchReply = SampleClients.CreateFailedCallAsync<StartMatchResult>();
            _roomFlow.StartMatch();

            RoomOutcome? outcome = _roomFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        [Fact]
        public void Test_TakeOutcome_WhenKickingFails_ShowsTheServerError()
        {
            _gameSessionConnection.KickMemberReply = SampleClients.CreateFailedCallAsync<KickMemberResult>();
            _roomFlow.Kick(SampleClients.BetoId);

            RoomOutcome? outcome = _roomFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        [Fact]
        public void Test_TakeOutcome_AfterLosingTheHost_ShowsTheHostOnlyHint()
        {
            _gameSessionConnection.KickMemberReply = Task.FromResult(KickMemberResult.NotHost);
            _roomFlow.Kick(SampleClients.BetoId);

            RoomOutcome? outcome = _roomFlow.TakeOutcome();

            Assert.Equal(TextKeys.Room.HostOnlyHint, outcome?.Notice?.TitleKey);
        }
    }
}
