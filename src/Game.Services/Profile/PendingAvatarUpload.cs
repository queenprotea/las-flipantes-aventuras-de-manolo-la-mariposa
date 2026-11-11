using System;

using Game.Contracts;

namespace Game.Services
{
    internal sealed class PendingAvatarUpload
    {
        private const string IncompleteUploadMessage = "The avatar upload has not received the start of the image.";

        private readonly byte[] _original;
        private readonly byte[] _thumbnail;
        private readonly bool[] _receivedOriginalParts;
        private readonly bool[] _receivedThumbnailParts;

        private string? _extension;

        internal PendingAvatarUpload(int originalBytes, int thumbnailBytes, DateTime startedAtUtc)
        {
            _original = new byte[originalBytes];
            _thumbnail = new byte[thumbnailBytes];
            _receivedOriginalParts = new bool[CountParts(originalBytes)];
            _receivedThumbnailParts = new bool[CountParts(thumbnailBytes)];
            LastActivityUtc = startedAtUtc;
        }

        internal DateTime LastActivityUtc { get; private set; }

        internal bool IsComplete()
        {
            bool isComplete = (_extension is not null)
                && Array.TrueForAll(_receivedOriginalParts, IsReceived)
                && Array.TrueForAll(_receivedThumbnailParts, IsReceived);

            return isComplete;
        }

        internal SendAvatarPartResult Write(ReceivedAvatarPart receivedPart, DateTime nowUtc)
        {
            byte[] targetBuffer = receivedPart.FileKind == AvatarFileKind.Thumbnail ? _thumbnail : _original;
            if (!FitsInBuffer(targetBuffer.Length, receivedPart.Offset, receivedPart.Bytes.Length))
            {
                return SendAvatarPartResult.OutOfRange;
            }

            SendAvatarPartResult writeResult = CheckFileStart(receivedPart);
            if (writeResult == SendAvatarPartResult.Accepted)
            {
                receivedPart.Bytes.CopyTo(targetBuffer, receivedPart.Offset);
                bool[] receivedParts = receivedPart.FileKind == AvatarFileKind.Thumbnail ? _receivedThumbnailParts : _receivedOriginalParts;
                receivedParts[receivedPart.Offset / ProfileLimits.AvatarPartBytes] = true;
                LastActivityUtc = nowUtc;
            }

            return writeResult;
        }

        internal void Touch(DateTime nowUtc)
        {
            LastActivityUtc = nowUtc;
        }

        internal CompletedAvatarUpload ToCompleted()
        {
            if (_extension is null)
            {
                throw new InvalidOperationException(IncompleteUploadMessage);
            }

            var completedUpload = new CompletedAvatarUpload(_original, _extension, _thumbnail);

            return completedUpload;
        }

        private static int CountParts(int fileBytes)
        {
            int partCount = (int)Math.Ceiling((double)fileBytes / ProfileLimits.AvatarPartBytes);

            return partCount;
        }

        private static bool FitsInBuffer(int bufferBytes, int partOffset, int partBytes)
        {
            bool isAlignedOffset = (partOffset >= 0) && (partOffset < bufferBytes) && (partOffset % ProfileLimits.AvatarPartBytes == 0);
            bool fitsInBuffer = isAlignedOffset && (partBytes == Math.Min(ProfileLimits.AvatarPartBytes, bufferBytes - partOffset));

            return fitsInBuffer;
        }

        private static bool IsReceived(bool isPartReceived)
        {
            return isPartReceived;
        }

        private SendAvatarPartResult CheckFileStart(ReceivedAvatarPart receivedPart)
        {
            SendAvatarPartResult fileStartResult = SendAvatarPartResult.Accepted;
            if (receivedPart.Offset != 0)
            {
                return fileStartResult;
            }

            if (receivedPart.FileKind == AvatarFileKind.Thumbnail)
            {
                fileStartResult = AvatarImageCheck.IsValidThumbnailStart(receivedPart.Bytes) ? SendAvatarPartResult.Accepted : SendAvatarPartResult.ThumbnailInvalid;
            }
            else
            {
                _extension = AvatarImageCheck.FindExtension(receivedPart.Bytes);
                fileStartResult = _extension is null ? SendAvatarPartResult.AvatarInvalidFormat : SendAvatarPartResult.Accepted;
            }

            return fileStartResult;
        }
    }
}
