using System;
using System.IO;

using Microsoft.Xna.Framework;

using StbImageSharp;

using Torres.Client.Ui;

namespace Torres.Client.Avatars
{
    internal static class AvatarThumbnailMaker
    {
        private const byte OpaqueAlpha = 255;
        private const int AlphaOffset = 3;
        private const int GreenOffset = 1;
        private const int BlueOffset = 2;

        private static readonly byte[] _pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] _jpgSignature = new byte[] { 0xFF, 0xD8, 0xFF };

        internal static AvatarPreparation Prepare(string filePath)
        {
            var chosenFile = new FileInfo(filePath);
            long fileBytes = chosenFile.Length;
            var preparation = new AvatarPreparation(AvatarPreparationStatus.TooLarge, null, fileBytes);
            if (fileBytes <= ProfileLimits.LargestAvatarBytes)
            {
                preparation = PrepareImage(File.ReadAllBytes(filePath));
            }

            return preparation;
        }

        private static AvatarPreparation PrepareImage(byte[] originalFile)
        {
            var preparation = new AvatarPreparation(AvatarPreparationStatus.InvalidFormat, null, originalFile.Length);
            if (StartsWith(originalFile, _pngSignature) || StartsWith(originalFile, _jpgSignature))
            {
                ImageResult decodedImage = ImageResult.FromMemory(originalFile, ColorComponents.RedGreenBlueAlpha);
                var preparedAvatar = new PreparedAvatar(originalFile, MakeThumbnail(decodedImage));
                preparation = new AvatarPreparation(AvatarPreparationStatus.Prepared, preparedAvatar, originalFile.Length);
            }

            return preparation;
        }

        private static byte[] MakeThumbnail(ImageResult decodedImage)
        {
            byte[] pixels = new SquareDownscaler(decodedImage).Downscale(ProfileLimits.ThumbnailSide);
            bool hasTransparency = HasTransparency(pixels);
            byte[] thumbnail = PngEncoder.Encode(pixels, ProfileLimits.ThumbnailSide, hasTransparency);
            if (thumbnail.Length > ProfileLimits.LargestThumbnailBytes)
            {
                FlattenOnto(pixels, Theme.MintAlt);
                thumbnail = PngEncoder.Encode(pixels, ProfileLimits.ThumbnailSide, false);
            }

            return thumbnail;
        }

        private static bool HasTransparency(byte[] pixels)
        {
            bool hasTransparency = false;
            for (var alphaIndex = AlphaOffset; alphaIndex < pixels.Length; alphaIndex += PngEncoder.SourceBytesPerPixel)
            {
                hasTransparency = hasTransparency || (pixels[alphaIndex] < OpaqueAlpha);
            }

            return hasTransparency;
        }

        private static void FlattenOnto(byte[] pixels, Color backgroundColor)
        {
            for (var pixelStart = 0; pixelStart < pixels.Length; pixelStart += PngEncoder.SourceBytesPerPixel)
            {
                int pixelAlpha = pixels[pixelStart + AlphaOffset];
                pixels[pixelStart] = Blend(pixels[pixelStart], backgroundColor.R, pixelAlpha);
                pixels[pixelStart + GreenOffset] = Blend(pixels[pixelStart + GreenOffset], backgroundColor.G, pixelAlpha);
                pixels[pixelStart + BlueOffset] = Blend(pixels[pixelStart + BlueOffset], backgroundColor.B, pixelAlpha);
                pixels[pixelStart + AlphaOffset] = OpaqueAlpha;
            }
        }

        private static byte Blend(byte foregroundChannel, byte backgroundChannel, int pixelAlpha)
        {
            byte blendedChannel = (byte)(((foregroundChannel * pixelAlpha) + (backgroundChannel * (OpaqueAlpha - pixelAlpha))) / OpaqueAlpha);

            return blendedChannel;
        }

        private static bool StartsWith(byte[] fileStart, byte[] signature)
        {
            bool startsWithSignature = (fileStart.Length >= signature.Length)
                && fileStart.AsSpan(0, signature.Length).SequenceEqual(signature);

            return startsWithSignature;
        }
    }
}
