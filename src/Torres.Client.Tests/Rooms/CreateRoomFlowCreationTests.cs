using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class CreateRoomFlowCreationTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = SampleClients.CreateSessionOfAna();
        private readonly CreateRoomFlow _createRoomFlow;

        public CreateRoomFlowCreationTests()
        {
            _createRoomFlow = new CreateRoomFlow(_gameSessionConnection, _playerSession);
        }

        [Fact]
        public void Test_FindRedirect_WithoutLoggingIn_GoesToTheLogin()
        {
            var createRoomFlow = new CreateRoomFlow(_gameSessionConnection, new PlayerSession());

            ScreenId? redirect = createRoomFlow.FindRedirect();

            Assert.Equal(ScreenId.Login, redirect);
        }

        [Fact]
        public void Test_FindRedirect_InsideARoom_GoesToTheRoom()
        {
            _playerSession.EnterRoom(SampleClients.CreateRoom());

            ScreenId? redirect = _createRoomFlow.FindRedirect();

            Assert.Equal(ScreenId.Room, redirect);
        }

        [Fact]
        public void Test_FindRedirect_OutsideARoom_StaysOnTheCreation()
        {
            ScreenId? redirect = _createRoomFlow.FindRedirect();

            Assert.Null(redirect);
        }

        [Fact]
        public void Test_IsCreating_AfterStartingAPrivateRoom_ReturnsTrue()
        {
            _createRoomFlow.StartCreation(false);

            bool isCreating = _createRoomFlow.IsCreating;

            Assert.True(isCreating);
        }

        [Fact]
        public void Test_TakeOutcome_AlreadyInsideARoom_ShowsAlreadyInRoom()
        {
            CreateWith(CreateRoomStatus.AlreadyInRoom);

            ScreenOutcome? outcome = _createRoomFlow.TakeOutcome();

            Assert.Equal(TextKeys.Rooms.AlreadyInRoomError, outcome?.Notice?.TitleKey);
        }

        [Fact]
        public void Test_TakeOutcome_WithABannedPlayer_ShowsTheBan()
        {
            CreateWith(CreateRoomStatus.Banned);

            ScreenOutcome? outcome = _createRoomFlow.TakeOutcome();

            bool? isBan = outcome?.Notice?.IsBan;
            Assert.True(isBan);
        }

        [Fact]
        public void Test_TakeOutcome_WithoutIdentity_GoesToTheLogin()
        {
            CreateWith(CreateRoomStatus.NotIdentified);

            ScreenOutcome? outcome = _createRoomFlow.TakeOutcome();

            Assert.Equal(ScreenId.Login, outcome?.NextScreen);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheCreationFindsTheDatabaseUnavailable_ShowsTheServerError()
        {
            CreateWith(CreateRoomStatus.DatabaseUnavailable);

            ScreenOutcome? outcome = _createRoomFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheRoomComesMissing_ShowsTheServerError()
        {
            CreateWith(CreateRoomStatus.Created);

            ScreenOutcome? outcome = _createRoomFlow.TakeOutcome();

            Notice serverError = Notice.CreateServerError();
            Assert.Equal(serverError, outcome?.Notice);
        }

        private void CreateWith(CreateRoomStatus creationStatus)
        {
            _gameSessionConnection.CreateRoomReply = Task.FromResult(new CreateRoomResult { Status = creationStatus });
            _createRoomFlow.StartCreation(true);
        }
    }
}
