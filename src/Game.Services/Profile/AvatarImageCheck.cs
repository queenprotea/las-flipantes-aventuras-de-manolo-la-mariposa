using System;
using System.IO;

using Game.Contracts;

using log4net;

using StbImageSharp;

namespace Game.Services
{
    internal static class AvatarImageCheck
    {
        private const int PngWidthOffset = 16;
        private const int PngHeightOffset = 20;
        private const int PngHeaderBytes = 24;
        private const int BitsPerByte = 8;
        private const int BytesPerSide = 4;
        private const string UndecodableThumbnailMessage = "A completed avatar thumbnail could not be decoded; the upload is rejected.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(AvatarImageCheck));

        private static readonly byte[] _pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        internal static bool IsValidThumbnailStart(byte[] thumbnailStart)
        {
            bool hasPngHeader = (thumbnailStart.Length >= PngHeaderBytes) && StartsWith(thumbnailStart, _pngSignature);
            bool isValidStart = hasPngHeader
                && (ReadSide(thumbnailStart, PngWidthOffset) == ProfileLimits.ThumbnailSide)
                && (ReadSide(thumbnailStart, PngHeightOffset) == ProfileLimits.ThumbnailSide);

            return isValidStart;
        }

        internal static bool IsDecodableThumbnail(byte[] thumbnail)
        {
            bool isDecodable = false;
            try
            {
                ImageResult decodedThumbnail = ImageResult.FromMemory(thumbnail, ColorComponents.RedGreenBlueAlpha);
                isDecodable = (decodedThumbnail.Width == ProfileLimits.ThumbnailSide) && (decodedThumbnail.Height == ProfileLimits.ThumbnailSide);
            }
            catch (InvalidOperationException exception)
            {
                _log.Warn(UndecodableThumbnailMessage, exception);
            }
            catch (IOException exception)
            {
                _log.Warn(UndecodableThumbnailMessage, exception);
            }
            catch (ArgumentException exception)
            {
                _log.Warn(UndecodableThumbnailMessage, exception);
            }

            return isDecodable;
        }

        private static bool StartsWith(byte[] fileStart, byte[] signature)
        {
            bool startsWithSignature = (fileStart.Length >= signature.Length)
                && fileStart.AsSpan(0, signature.Length).SequenceEqual(signature);

            return startsWithSignature;
        }

        private static int ReadSide(byte[] pngStart, int sideOffset)
        {
            int sideLength = 0;
            for (var byteIndex = 0; byteIndex < BytesPerSide; byteIndex++)
            {
                sideLength = (sideLength << BitsPerByte) | pngStart[sideOffset + byteIndex];
            }

            return sideLength;
        }
    }
}
