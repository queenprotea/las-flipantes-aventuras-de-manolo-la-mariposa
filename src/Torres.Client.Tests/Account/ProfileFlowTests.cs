using Game.Contracts;

using Torres.Client.Flows;
using Torres.Client.Localization;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class ProfileFlowTests
    {
        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly ProfileFlow _profileFlow;

        public ProfileFlowTests()
        {
            _profileFlow = new ProfileFlow(_gameSessionConnection, SampleClients.CreateSessionOfAna(), new RecordingAvatarCache());
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
    }
}
