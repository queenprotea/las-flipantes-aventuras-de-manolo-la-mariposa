using System;

using Game.Contracts;

namespace Game.Services
{
    internal static class AvatarImageCheck
    {
        internal const string PngExtension = "png";
        internal const string JpgExtension = "jpg";

        private const int PngWidthOffset = 16;
        private const int PngHeightOffset = 20;
        private const int PngHeaderBytes = 24;
        private const int BitsPerByte = 8;
        private const int BytesPerSide = 4;

        private static readonly byte[] _pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] _jpgSignature = new byte[] { 0xFF, 0xD8, 0xFF };

        internal static string? FindExtension(byte[] fileStart)
        {
            string? extension = null;
            if (StartsWith(fileStart, _pngSignature))
            {
                extension = PngExtension;
            }
            else if (StartsWith(fileStart, _jpgSignature))
            {
                extension = JpgExtension;
            }

            return extension;
        }

        internal static bool IsValidThumbnailStart(byte[] thumbnailStart)
        {
            bool hasPngHeader = (thumbnailStart.Length >= PngHeaderBytes) && StartsWith(thumbnailStart, _pngSignature);
            bool isValidStart = hasPngHeader
                && (ReadSide(thumbnailStart, PngWidthOffset) == ProfileLimits.ThumbnailSide)
                && (ReadSide(thumbnailStart, PngHeightOffset) == ProfileLimits.ThumbnailSide);

            return isValidStart;
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
