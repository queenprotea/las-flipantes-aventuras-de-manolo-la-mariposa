using StbImageSharp;

using Torres.Client.Avatars;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class SquareDownscalerTests
    {
        private const int SourceSide = 2;
        private const int SinglePixelSide = 1;
        private const int LargerSide = 4;
        private const int LargerImageBytes = 64;

        private static readonly byte[] _stripedPixels = new byte[] { 0, 0, 0, 255, 200, 200, 200, 255, 0, 0, 0, 255, 200, 200, 200, 255 };
        private static readonly byte[] _averagePixel = new byte[] { 100, 100, 100, 255 };

        private readonly SquareDownscaler _downscaler;

        public SquareDownscalerTests()
        {
            byte[] pngBytes = PngEncoder.Encode(_stripedPixels, SourceSide, true);
            ImageResult decodedImage = ImageResult.FromMemory(pngBytes, ColorComponents.RedGreenBlueAlpha);
            _downscaler = new SquareDownscaler(decodedImage);
        }

        [Fact]
        public void Test_Downscale_ToOnePixel_AveragesTheWholeImage()
        {
            byte[] targetPixels = _downscaler.Downscale(SinglePixelSide);

            Assert.Equal(_averagePixel, targetPixels);
        }

        [Fact]
        public void Test_Downscale_ToALargerSide_ReturnsFourBytesPerTargetPixel()
        {
            byte[] targetPixels = _downscaler.Downscale(LargerSide);

            Assert.Equal(LargerImageBytes, targetPixels.Length);
        }
    }
}
