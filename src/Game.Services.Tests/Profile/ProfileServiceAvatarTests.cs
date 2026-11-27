using System;
using System.IO;
using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class ProfileServiceAvatarTests
    {
        private const int ValidThumbnailBytes = 1024;
        private const int FirstOffset = 0;

        private static readonly byte[] _somePart = new byte[] { 1, 2, 3 };

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly ProfileService _profileService;

        public ProfileServiceAvatarTests()
        {
            RoomService roomService = SampleServices.CreateRoomService(_callerSessions, new RoomRegistry(), _fakeTimeProvider);
            var avatarStore = new AvatarStore(Path.Combine(Path.GetTempPath(), SampleServices.AvatarFolderName));
            AvatarUploadRegistry avatarUploadRegistry = SampleServices.CreateAvatarUploadRegistry(_callerSessions, _fakeTimeProvider);
            _profileService = new ProfileService(_callerSessions.Registry, new RecordingPlayerRepository(), avatarStore, avatarUploadRegistry, roomService, new RecordingMatchRecordRepository(new RecordingMatchParticipantRepository()));
            _callerSessions.Open(SamplePlayers.CaroGuest);
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public async Task Test_GetProfileAsync_OfAnAccount_ReturnsItsUsername()
        {
            ProfileResponse profile = await _profileService.GetProfileAsync();

            Assert.Equal(SamplePlayers.AnaUsername, profile.Username);
        }

        [Fact]
        public async Task Test_GetProfileAsync_AsAGuest_ThrowsFaultException()
        {
            _callerSessions.ActAs(SamplePlayers.CaroGuest);

            await Assert.ThrowsAsync<FaultException>(() => _profileService.GetProfileAsync());
        }

        [Fact]
        public void Test_SendAvatarPart_WithAnEmptyPart_ReturnsOutOfRange()
        {
            _profileService.BeginAvatarUpload(ValidThumbnailBytes);

            SendAvatarPartResult partResult = _profileService.SendAvatarPart(FirstOffset, Array.Empty<byte>());

            Assert.Equal(SendAvatarPartResult.OutOfRange, partResult);
        }

        [Fact]
        public void Test_SendAvatarPart_WithoutUpload_ReturnsNoUploadInProgress()
        {
            SendAvatarPartResult partResult = _profileService.SendAvatarPart(FirstOffset, _somePart);

            Assert.Equal(SendAvatarPartResult.NoUploadInProgress, partResult);
        }

        [Fact]
        public void Test_CancelAvatarUpload_DuringAnUpload_DiscardsIt()
        {
            _profileService.BeginAvatarUpload(ValidThumbnailBytes);

            _profileService.CancelAvatarUpload();

            SendAvatarPartResult partResult = _profileService.SendAvatarPart(FirstOffset, _somePart);
            Assert.Equal(SendAvatarPartResult.NoUploadInProgress, partResult);
        }

        [Fact]
        public async Task Test_UpdateProfileAsync_WithANewAvatarNotUploaded_ReturnsAvatarIncomplete()
        {
            UpdateProfileResult updateResult = await _profileService.UpdateProfileAsync(SamplePlayers.AnaUsername, true);

            Assert.Equal(UpdateProfileStatus.AvatarIncomplete, updateResult.Status);
        }

        [Fact]
        public async Task Test_GetAvatarThumbnailAsync_WithABlankReference_ReturnsNotFound()
        {
            AvatarResponse avatarResponse = await _profileService.GetAvatarThumbnailAsync(string.Empty);

            Assert.Equal(AvatarLookupStatus.NotFound, avatarResponse.Status);
        }
    }
}
