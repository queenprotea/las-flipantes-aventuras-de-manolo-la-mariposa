using System;
using System.IO;

using Torres.Client.Avatars;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class AvatarThumbnailMakerTests : IDisposable
    {
        private const int SourceSide = 2;
        private const int SignatureLength = 8;
        private const long TooLargeFileBytes = 6 * 1024 * 1024;
        private const string TextContent = "esto no es una imagen";
        private const string PngExtension = ".png";
        private const string TextExtension = ".txt";

        private static readonly byte[] _pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] _opaquePixels = new byte[] { 255, 0, 0, 255, 0, 255, 0, 255, 0, 0, 255, 255, 255, 255, 255, 255 };

        private readonly string _imagePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + PngExtension);
        private readonly string _textPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + TextExtension);
        private readonly string _largePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + PngExtension);

        public AvatarThumbnailMakerTests()
        {
            File.WriteAllBytes(_imagePath, PngEncoder.Encode(_opaquePixels, SourceSide, true));
            File.WriteAllText(_textPath, TextContent);
        }

        [Fact]
        public void Test_Prepare_OfAPngImage_ReturnsPrepared()
        {
            AvatarPreparation preparation = AvatarThumbnailMaker.Prepare(_imagePath);

            Assert.Equal(AvatarPreparationStatus.Prepared, preparation.Status);
        }

        [Fact]
        public void Test_Prepare_OfAPngImage_ReturnsAPngThumbnail()
        {
            AvatarPreparation preparation = AvatarThumbnailMaker.Prepare(_imagePath);

            byte[]? signatureBytes = preparation.Avatar?.Thumbnail[..SignatureLength];
            Assert.Equal(_pngSignature, signatureBytes);
        }

        [Fact]
        public void Test_Prepare_OfATextFile_ReturnsInvalidFormat()
        {
            AvatarPreparation preparation = AvatarThumbnailMaker.Prepare(_textPath);

            Assert.Equal(AvatarPreparationStatus.InvalidFormat, preparation.Status);
        }

        [Fact]
        public void Test_Prepare_OfAFileLargerThanTheLimit_ReturnsTooLarge()
        {
            using (var largeFile = new FileStream(_largePath, FileMode.Create))
            {
                largeFile.SetLength(TooLargeFileBytes);
            }

            AvatarPreparation preparation = AvatarThumbnailMaker.Prepare(_largePath);

            Assert.Equal(AvatarPreparationStatus.TooLarge, preparation.Status);
        }

        public void Dispose()
        {
            File.Delete(_imagePath);
            File.Delete(_textPath);
            File.Delete(_largePath);
        }
    }
}
