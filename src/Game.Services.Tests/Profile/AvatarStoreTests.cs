using System;
using System.IO;
using System.Threading.Tasks;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class AvatarStoreTests : IDisposable
    {
        private const string MissingReference = "avatars/no-existe.png";
        private const string FolderOnlyReference = "avatars/";

        private static readonly byte[] _thumbnail = new byte[] { 1, 2, 3, 4 };

        private readonly string _folderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        private readonly AvatarStore _avatarStore;

        public AvatarStoreTests()
        {
            _avatarStore = new AvatarStore(_folderPath);
            _avatarStore.EnsureFolder();
        }

        [Fact]
        public void Test_EnsureFolder_WithoutFolder_CreatesIt()
        {
            bool folderExists = Directory.Exists(_folderPath);

            Assert.True(folderExists);
        }

        [Fact]
        public async Task Test_ReadThumbnailAsync_OfASavedAvatar_ReturnsItsBytes()
        {
            string avatarReference = _avatarStore.Save(SamplePlayers.AnaId, new CompletedAvatarUpload(_thumbnail));

            byte[]? thumbnail = await _avatarStore.ReadThumbnailAsync(avatarReference);

            Assert.Equal(_thumbnail, thumbnail);
        }

        [Fact]
        public async Task Test_ReadThumbnailAsync_OfAMissingAvatar_ReturnsNull()
        {
            byte[]? thumbnail = await _avatarStore.ReadThumbnailAsync(MissingReference);

            Assert.Null(thumbnail);
        }

        [Fact]
        public async Task Test_ReadThumbnailAsync_OfAReferenceWithoutFileName_ReturnsNull()
        {
            byte[]? thumbnail = await _avatarStore.ReadThumbnailAsync(FolderOnlyReference);

            Assert.Null(thumbnail);
        }

        [Fact]
        public async Task Test_Delete_OfASavedAvatar_RemovesIt()
        {
            string avatarReference = _avatarStore.Save(SamplePlayers.AnaId, new CompletedAvatarUpload(_thumbnail));

            _avatarStore.Delete(avatarReference);

            byte[]? thumbnail = await _avatarStore.ReadThumbnailAsync(avatarReference);
            Assert.Null(thumbnail);
        }

        [Fact]
        public void Test_Delete_OfAReferenceWithoutFileName_FinishesWithoutError()
        {
            Exception? deleteFailure = Record.Exception(() => _avatarStore.Delete(FolderOnlyReference));

            Assert.Null(deleteFailure);
        }

        public void Dispose()
        {
            Directory.Delete(_folderPath, true);
        }
    }
}
