using Xunit;

namespace Game.Services.Tests
{
    public sealed class AvatarImageCheckTests
    {
        private static readonly byte[] _thumbnailStart = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13, 0x49, 0x48, 0x44, 0x52, 0, 0, 0, 128, 0, 0, 0, 128 };
        private static readonly byte[] _smallImageStart = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13, 0x49, 0x48, 0x44, 0x52, 0, 0, 0, 64, 0, 0, 0, 64 };
        private static readonly byte[] _jpegStart = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0, 16, 0x4A, 0x46, 0x49, 0x46, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0 };
        private static readonly byte[] _signatureOnly = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        [Fact]
        public void Test_IsValidThumbnailStart_OfAPngOfTheThumbnailSide_ReturnsTrue()
        {
            bool isValidStart = AvatarImageCheck.IsValidThumbnailStart(_thumbnailStart);

            Assert.True(isValidStart);
        }

        [Fact]
        public void Test_IsValidThumbnailStart_OfASmallerPng_ReturnsFalse()
        {
            bool isValidStart = AvatarImageCheck.IsValidThumbnailStart(_smallImageStart);

            Assert.False(isValidStart);
        }

        [Fact]
        public void Test_IsValidThumbnailStart_OfAJpeg_ReturnsFalse()
        {
            bool isValidStart = AvatarImageCheck.IsValidThumbnailStart(_jpegStart);

            Assert.False(isValidStart);
        }

        [Fact]
        public void Test_IsValidThumbnailStart_OfAnIncompleteHeader_ReturnsFalse()
        {
            bool isValidStart = AvatarImageCheck.IsValidThumbnailStart(_signatureOnly);

            Assert.False(isValidStart);
        }

        [Fact]
        public void Test_IsDecodableThumbnail_OfAnIncompletePng_ReturnsFalse()
        {
            bool isDecodable = AvatarImageCheck.IsDecodableThumbnail(_thumbnailStart);

            Assert.False(isDecodable);
        }
    }
}
