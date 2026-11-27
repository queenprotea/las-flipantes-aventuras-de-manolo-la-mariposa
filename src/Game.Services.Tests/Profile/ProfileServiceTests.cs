using System;
using System.IO;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Persistence;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class ProfileServiceTests
    {
        private const string NewUsername = "ana_flores";
        private const string BadUsername = "a!";
        private const string MissingAvatarReference = "avatar-que-no-existe";
        private const string AvatarFolderName = "torres-avatares-de-prueba";
        private const int ValidThumbnailBytes = 1024;
        private const int NoRoomId = 0;
        private const int TooLargeThumbnailBytes = ProfileLimits.LargestThumbnailBytes + 1;

        private static readonly TimeSpan _uploadIdleLimit = TimeSpan.FromMinutes(1);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly RoomRegistry _roomRegistry = new RoomRegistry();
        private readonly RecordingPlayerRepository _playerRepository = new RecordingPlayerRepository();
        private readonly RoomService _roomService;
        private readonly ProfileService _profileService;

        public ProfileServiceTests()
        {
            _roomService = new RoomService(_roomRegistry, _callerSessions.Registry, new RecordingRoomRecordRepository(), new RecordingSanctionRepository(), _playerRepository, _fakeTimeProvider);
            var avatarStore = new AvatarStore(Path.Combine(Path.GetTempPath(), AvatarFolderName));
            var avatarUploadRegistry = new AvatarUploadRegistry(_callerSessions.Registry, _fakeTimeProvider, new AvatarUploadTiming(_uploadIdleLimit));
            _profileService = new ProfileService(_callerSessions.Registry, _playerRepository, avatarStore, avatarUploadRegistry, _roomService);
            _callerSessions.Open(SamplePlayers.Beto);
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public async Task Test_UpdateProfileAsync_WithNewUsername_ReturnsUpdated()
        {
            UpdateProfileResult updateResult = await _profileService.UpdateProfileAsync(NewUsername, false);

            Assert.Equal(UpdateProfileStatus.Updated, updateResult.Status);
        }

        [Fact]
        public async Task Test_UpdateProfileAsync_WithNewUsername_StoresIt()
        {
            await _profileService.UpdateProfileAsync(NewUsername, false);

            Player? storedPlayer = _playerRepository.FindByUsername(NewUsername);
            Assert.NotNull(storedPlayer);
        }

        [Fact]
        public async Task Test_UpdateProfileAsync_WithTakenUsername_ReturnsUsernameTaken()
        {
            UpdateProfileResult updateResult = await _profileService.UpdateProfileAsync(SamplePlayers.BetoUsername, false);

            Assert.Equal(UpdateProfileStatus.UsernameTaken, updateResult.Status);
        }

        [Fact]
        public async Task Test_UpdateProfileAsync_WithInvalidUsername_ReturnsInvalidUsername()
        {
            UpdateProfileResult updateResult = await _profileService.UpdateProfileAsync(BadUsername, false);

            Assert.Equal(UpdateProfileStatus.InvalidUsername, updateResult.Status);
        }

        [Fact]
        public async Task Test_UpdateProfileAsync_WithoutNewAvatar_KeepsTheStoredAvatar()
        {
            await _profileService.UpdateProfileAsync(NewUsername, false);

            string? storedAvatarReference = await _playerRepository.FindAvatarReferenceAsync(SamplePlayers.AnaId);
            Assert.Equal(SamplePlayers.AnaAvatarReference, storedAvatarReference);
        }

        [Fact]
        public async Task Test_UpdateProfileAsync_DuringAMatch_ReturnsRoomInMatch()
        {
            CreateRoomResult creation = await _roomService.CreateRoomAsync(RoomVisibility.Private);
            int roomId = creation.Room?.RoomId ?? NoRoomId;
            _callerSessions.ActAs(SamplePlayers.Beto);
            await _roomService.JoinRoomAsync(roomId);
            _roomRegistry.FindById(roomId)?.StartMatch(SamplePlayers.AnaId);
            _callerSessions.ActAs(SamplePlayers.Ana);

            UpdateProfileResult updateResult = await _profileService.UpdateProfileAsync(NewUsername, false);

            Assert.Equal(UpdateProfileStatus.RoomInMatch, updateResult.Status);
        }

        [Fact]
        public void Test_BeginAvatarUpload_WithValidSize_ReturnsStarted()
        {
            BeginAvatarUploadResult beginResult = _profileService.BeginAvatarUpload(ValidThumbnailBytes);

            Assert.Equal(BeginAvatarUploadResult.Started, beginResult);
        }

        [Fact]
        public void Test_BeginAvatarUpload_WithTooLargeThumbnail_ReturnsThumbnailInvalid()
        {
            BeginAvatarUploadResult beginResult = _profileService.BeginAvatarUpload(TooLargeThumbnailBytes);

            Assert.Equal(BeginAvatarUploadResult.ThumbnailInvalid, beginResult);
        }

        [Fact]
        public async Task Test_GetAvatarThumbnailAsync_WithMissingFile_ReturnsNotFound()
        {
            AvatarResponse avatar = await _profileService.GetAvatarThumbnailAsync(MissingAvatarReference);

            Assert.Equal(AvatarLookupStatus.NotFound, avatar.Status);
        }
    }
}
