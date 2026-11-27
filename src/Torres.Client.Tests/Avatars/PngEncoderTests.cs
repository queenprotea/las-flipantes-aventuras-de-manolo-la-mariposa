using StbImageSharp;

using Torres.Client.Avatars;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class PngEncoderTests
    {
        private const int ImageSide = 2;
        private const int SignatureLength = 8;

        private static readonly byte[] _pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] _pixels = new byte[] { 255, 0, 0, 255, 0, 255, 0, 128, 0, 0, 255, 255, 255, 255, 255, 0 };
        private static readonly byte[] _pixelsWithoutAlpha = new byte[] { 255, 0, 0, 0, 255, 0, 0, 0, 255, 255, 255, 255 };

        [Fact]
        public void Test_Encode_AnyImage_StartsWithThePngSignature()
        {
            byte[] pngBytes = PngEncoder.Encode(_pixels, ImageSide, true);

            byte[] signatureBytes = pngBytes[..SignatureLength];
            Assert.Equal(_pngSignature, signatureBytes);
        }

        [Fact]
        public void Test_Encode_WithAlpha_KeepsTheSideOfTheImage()
        {
            byte[] pngBytes = PngEncoder.Encode(_pixels, ImageSide, true);

            ImageResult decodedImage = ImageResult.FromMemory(pngBytes, ColorComponents.RedGreenBlueAlpha);
            Assert.Equal(ImageSide, decodedImage.Width);
        }

        [Fact]
        public void Test_Encode_WithAlpha_KeepsEveryPixel()
        {
            byte[] pngBytes = PngEncoder.Encode(_pixels, ImageSide, true);

            ImageResult decodedImage = ImageResult.FromMemory(pngBytes, ColorComponents.RedGreenBlueAlpha);
            Assert.Equal(_pixels, decodedImage.Data);
        }

        [Fact]
        public void Test_Encode_WithoutAlpha_KeepsTheColorOfEveryPixel()
        {
            byte[] pngBytes = PngEncoder.Encode(_pixels, ImageSide, false);

            ImageResult decodedImage = ImageResult.FromMemory(pngBytes, ColorComponents.RedGreenBlue);
            Assert.Equal(_pixelsWithoutAlpha, decodedImage.Data);
        }
    }
}
