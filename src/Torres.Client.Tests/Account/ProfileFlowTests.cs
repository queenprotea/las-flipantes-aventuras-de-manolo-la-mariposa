using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class ProfileFlowTests
    {
        private const string UpdateProfileCall = nameof(RecordingGameSessionConnection.UpdateProfileAsync);
        private const string NewUsername = "ana_flores";
        private const string PaddedNewUsername = "  ana_flores  ";
        private const string ServerUsername = "ana_del_servidor";

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PlayerSession _playerSession = new PlayerSession();
        private readonly ProfileFlow _profileFlow;

        public ProfileFlowTests()
        {
            _playerSession.Start(new PlayerIdentity { PlayerId = SampleClients.AnaId, Username = SampleClients.AnaUsername, Email = SampleClients.AnaEmail });
            _profileFlow = new ProfileFlow(_gameSessionConnection, _playerSession, new RecordingAvatarCache());
        }

        [Fact]
        public void Test_TakeOutcome_WithServerUnreachable_ShowsTheServerError()
        {
            _gameSessionConnection.GetProfileReply = SampleClients.CreateFailedCallAsync<ProfileResponse>();
            _profileFlow.Open();
            _profileFlow.Follow();
            _profileFlow.TakeOutcome();

            ProfileOutcome? outcome = _profileFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.Message?.TextKey);
        }

        [Fact]
        public void Test_FindScreenToOpenSettings_FromTheProfile_ReturnsAccountSettings()
        {
            ScreenId nextScreen = ProfileFlow.FindScreenToOpenSettings();

            Assert.Equal(ScreenId.AccountSettings, nextScreen);
        }

        [Fact]
        public void Test_TakeOutcome_AfterOpening_ShowsTheUsernameOfTheSession()
        {
            _profileFlow.Open();

            ProfileOutcome? outcome = _profileFlow.TakeOutcome();

            Assert.Equal(SampleClients.AnaUsername, outcome?.Details?.Username);
        }

        [Fact]
        public void Test_TakeOutcome_AfterOpeningWithTheSessionClosed_ShowsTheServerError()
        {
            _gameSessionConnection.IsSessionOpen = false;
            _profileFlow.Open();
            _profileFlow.TakeOutcome();

            ProfileOutcome? outcome = _profileFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.Message?.TextKey);
        }

        [Fact]
        public void Test_TakeOutcome_AfterOpeningAsAGuest_ReturnsNull()
        {
            _playerSession.StartAsGuest(SampleClients.GuestAlias);
            _profileFlow.Open();

            ProfileOutcome? outcome = _profileFlow.TakeOutcome();

            Assert.Null(outcome);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheProfileArrives_ShowsTheUsernameOfTheServer()
        {
            _gameSessionConnection.GetProfileReply = Task.FromResult(new ProfileResponse { Username = ServerUsername, Email = SampleClients.AnaEmail });
            _profileFlow.Open();
            _profileFlow.Follow();
            _profileFlow.TakeOutcome();

            ProfileOutcome? outcome = _profileFlow.TakeOutcome();

            Assert.Equal(ServerUsername, outcome?.Details?.Username);
        }

        [Fact]
        public void Test_Follow_AfterTheProfileArrives_RenamesTheSession()
        {
            _gameSessionConnection.GetProfileReply = Task.FromResult(new ProfileResponse { Username = ServerUsername, Email = SampleClients.AnaEmail });
            _profileFlow.Open();

            _profileFlow.Follow();

            Assert.Equal(ServerUsername, _playerSession.DisplayName);
        }

        [Fact]
        public void Test_TakeOutcome_AfterSavingWithTheSessionClosed_ShowsTheServerError()
        {
            _gameSessionConnection.IsSessionOpen = false;
            _profileFlow.RequestSave(NewUsername);

            ProfileOutcome? outcome = _profileFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.Message?.TextKey);
        }

        [Fact]
        public void Test_RequestSave_WithTheSessionOpen_AsksTheServerToSave()
        {
            _profileFlow.RequestSave(NewUsername);

            Assert.Contains(UpdateProfileCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheProfileWasUpdated_MarksItAsSaved()
        {
            SaveWith(UpdateProfileStatus.Updated);

            ProfileOutcome? outcome = _profileFlow.TakeOutcome();

            bool? isSaved = outcome?.IsSaved;
            Assert.True(isSaved);
        }

        [Fact]
        public void Test_TakeOutcome_AfterTheProfileWasUpdated_ShowsTheTrimmedUsername()
        {
            SaveWith(UpdateProfileStatus.Updated);

            ProfileOutcome? outcome = _profileFlow.TakeOutcome();

            Assert.Equal(NewUsername, outcome?.Details?.Username);
        }

        [Fact]
        public void Test_Follow_AfterTheProfileWasUpdated_RenamesTheSession()
        {
            _gameSessionConnection.UpdateProfileReply = Task.FromResult(new UpdateProfileResult { Status = UpdateProfileStatus.Updated });
            _profileFlow.RequestSave(PaddedNewUsername);

            _profileFlow.Follow();

            Assert.Equal(NewUsername, _playerSession.DisplayName);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheUsernameIsTaken_ShowsUsernameUnavailable()
        {
            SaveWith(UpdateProfileStatus.UsernameTaken);

            ProfileOutcome? outcome = _profileFlow.TakeOutcome();

            Assert.Equal(TextKeys.Profile.UsernameUnavailable, outcome?.Message?.TextKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheUsernameIsInvalid_ShowsTheFormatError()
        {
            SaveWith(UpdateProfileStatus.InvalidUsername);

            ProfileOutcome? outcome = _profileFlow.TakeOutcome();

            Assert.Equal(TextKeys.Register.UsernameInvalidFormat, outcome?.Message?.TextKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheAvatarIsIncompleteWithoutAvatar_ShowsTheServerError()
        {
            SaveWith(UpdateProfileStatus.AvatarIncomplete);

            ProfileOutcome? outcome = _profileFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.Message?.TextKey);
        }

        [Fact]
        public void Test_TakeOutcome_DuringAMatch_ShowsMatchInProgress()
        {
            SaveWith(UpdateProfileStatus.RoomInMatch);

            ProfileOutcome? outcome = _profileFlow.TakeOutcome();

            Assert.Equal(TextKeys.Profile.MatchInProgress, outcome?.Message?.TextKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheStorageIsUnavailable_ShowsTheServerError()
        {
            SaveWith(UpdateProfileStatus.StorageUnavailable);

            ProfileOutcome? outcome = _profileFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.Message?.TextKey);
        }

        [Fact]
        public void Test_TakeOutcome_WhenTheSaveCallFails_ShowsTheServerError()
        {
            _gameSessionConnection.UpdateProfileReply = SampleClients.CreateFailedCallAsync<UpdateProfileResult>();
            _profileFlow.RequestSave(NewUsername);
            _profileFlow.Follow();

            ProfileOutcome? outcome = _profileFlow.TakeOutcome();

            Assert.Equal(TextKeys.Common.ServerErrorTitle, outcome?.Message?.TextKey);
        }

        [Fact]
        public void Test_IsInRoom_InsideARoom_ReturnsTrue()
        {
            _playerSession.EnterRoom(SampleClients.CreateRoom());

            bool isInRoom = _profileFlow.IsInRoom;

            Assert.True(isInRoom);
        }

        [Fact]
        public void Test_IsGuest_WithAnAccount_ReturnsFalse()
        {
            bool isGuest = _profileFlow.IsGuest;

            Assert.False(isGuest);
        }

        [Fact]
        public void Test_DisplayName_WithAnAccount_ReturnsTheUsername()
        {
            string displayName = _profileFlow.DisplayName;

            Assert.Equal(SampleClients.AnaUsername, displayName);
        }

        [Fact]
        public void Test_DiscardAvatarChange_WithoutAvatar_LeavesNoChange()
        {
            _profileFlow.DiscardAvatarChange();

            Assert.False(_profileFlow.HasAvatarChange);
        }

        [Fact]
        public void Test_UploadProgress_WithoutUpload_ReturnsNull()
        {
            float? uploadProgress = _profileFlow.UploadProgress;

            Assert.Null(uploadProgress);
        }

        private void SaveWith(UpdateProfileStatus saveStatus)
        {
            _gameSessionConnection.UpdateProfileReply = Task.FromResult(new UpdateProfileResult { Status = saveStatus });
            _profileFlow.RequestSave(PaddedNewUsername);
            _profileFlow.Follow();
        }
    }
}
