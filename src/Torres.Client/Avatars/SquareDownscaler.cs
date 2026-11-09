using System;

using StbImageSharp;

namespace Torres.Client.Avatars
{
    internal sealed class SquareDownscaler
    {
        private const int CenteringDivisor = 2;

        private readonly byte[] _sourcePixels;
        private readonly int _sourceWidth;
        private readonly int _cropLeft;
        private readonly int _cropTop;
        private readonly int _cropSide;

        internal SquareDownscaler(ImageResult decodedImage)
        {
            _sourcePixels = decodedImage.Data;
            _sourceWidth = decodedImage.Width;
            _cropSide = Math.Min(decodedImage.Width, decodedImage.Height);
            _cropLeft = (decodedImage.Width - _cropSide) / CenteringDivisor;
            _cropTop = (decodedImage.Height - _cropSide) / CenteringDivisor;
        }

        internal byte[] Downscale(int targetSide)
        {
            var targetPixels = new byte[targetSide * targetSide * PngEncoder.SourceBytesPerPixel];
            for (var targetIndex = 0; targetIndex < targetSide * targetSide; targetIndex++)
            {
                SourceArea sourceArea = FindSourceArea(targetIndex % targetSide, targetIndex / targetSide, targetSide);
                AverageInto(targetPixels, targetIndex, sourceArea);
            }

            return targetPixels;
        }

        private SourceArea FindSourceArea(int targetColumn, int targetRow, int targetSide)
        {
            int firstColumn = _cropLeft + (targetColumn * _cropSide / targetSide);
            int endColumn = Math.Max(firstColumn + 1, _cropLeft + ((targetColumn + 1) * _cropSide / targetSide));
            int firstRow = _cropTop + (targetRow * _cropSide / targetSide);
            int endRow = Math.Max(firstRow + 1, _cropTop + ((targetRow + 1) * _cropSide / targetSide));
            var sourceArea = new SourceArea(firstColumn, endColumn, firstRow, endRow);

            return sourceArea;
        }

        private void AverageInto(byte[] targetPixels, int targetIndex, SourceArea sourceArea)
        {
            var channelSums = new long[PngEncoder.SourceBytesPerPixel];
            for (var sourceRow = sourceArea.FirstRow; sourceRow < sourceArea.EndRow; sourceRow++)
            {
                AddRow(channelSums, sourceRow, sourceArea);
            }

            long sampleCount = (long)(sourceArea.EndRow - sourceArea.FirstRow) * (sourceArea.EndColumn - sourceArea.FirstColumn);
            for (var channelIndex = 0; channelIndex < PngEncoder.SourceBytesPerPixel; channelIndex++)
            {
                targetPixels[(targetIndex * PngEncoder.SourceBytesPerPixel) + channelIndex] = (byte)(channelSums[channelIndex] / sampleCount);
            }
        }

        private void AddRow(long[] channelSums, int sourceRow, SourceArea sourceArea)
        {
            for (var sourceColumn = sourceArea.FirstColumn; sourceColumn < sourceArea.EndColumn; sourceColumn++)
            {
                int pixelStart = ((sourceRow * _sourceWidth) + sourceColumn) * PngEncoder.SourceBytesPerPixel;
                AddPixel(channelSums, pixelStart);
            }
        }

        private void AddPixel(long[] channelSums, int pixelStart)
        {
            for (var channelIndex = 0; channelIndex < PngEncoder.SourceBytesPerPixel; channelIndex++)
            {
                channelSums[channelIndex] += _sourcePixels[pixelStart + channelIndex];
            }
        }
    }
}
