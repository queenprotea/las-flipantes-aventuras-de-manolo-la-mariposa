using System.IO;
using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class ProfileServiceFailureTests
    {
        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly ProfileService _profileService;

        public ProfileServiceFailureTests()
        {
            RoomService roomService = SampleServices.CreateRoomService(_callerSessions, new RoomRegistry(), _fakeTimeProvider);
            var avatarStore = new AvatarStore(Path.Combine(Path.GetTempPath(), SampleServices.AvatarFolderName));
            AvatarUploadRegistry avatarUploadRegistry = SampleServices.CreateAvatarUploadRegistry(_callerSessions, _fakeTimeProvider);
            _profileService = new ProfileService(_callerSessions.Registry, new FailingPlayerRepository(), avatarStore, avatarUploadRegistry, roomService, new RecordingMatchRecordRepository(new RecordingMatchParticipantRepository()));
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public async Task Test_GetProfileAsync_WithTheDatabaseDown_ThrowsFaultException()
        {
            await Assert.ThrowsAsync<FaultException>(() => _profileService.GetProfileAsync());
        }

        [Fact]
        public async Task Test_UpdateProfileAsync_WithTheDatabaseDown_ReturnsDatabaseUnavailable()
        {
            UpdateProfileResult updateResult = await _profileService.UpdateProfileAsync(SamplePlayers.AnaUsername, false);

            Assert.Equal(UpdateProfileStatus.DatabaseUnavailable, updateResult.Status);
        }
    }
}
