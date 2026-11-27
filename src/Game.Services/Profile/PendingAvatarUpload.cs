using System;

using Game.Contracts;

namespace Game.Services
{
    internal sealed class PendingAvatarUpload
    {
        private readonly byte[] _thumbnail;
        private readonly bool[] _receivedParts;

        internal PendingAvatarUpload(int thumbnailBytes, DateTime startedAtUtc)
        {
            _thumbnail = new byte[thumbnailBytes];
            _receivedParts = new bool[CountParts(thumbnailBytes)];
            LastActivityUtc = startedAtUtc;
        }

        internal DateTime LastActivityUtc { get; private set; }

        internal bool IsComplete()
        {
            bool isComplete = Array.TrueForAll(_receivedParts, IsReceived);

            return isComplete;
        }

        internal SendAvatarPartResult Write(ReceivedAvatarPart receivedPart, DateTime nowUtc)
        {
            if (!FitsInBuffer(_thumbnail.Length, receivedPart.Offset, receivedPart.Bytes.Length))
            {
                return SendAvatarPartResult.OutOfRange;
            }

            bool isValidStart = (receivedPart.Offset != 0) || AvatarImageCheck.IsValidThumbnailStart(receivedPart.Bytes);
            SendAvatarPartResult writeResult = SendAvatarPartResult.ThumbnailInvalid;
            if (isValidStart)
            {
                receivedPart.Bytes.CopyTo(_thumbnail, receivedPart.Offset);
                _receivedParts[receivedPart.Offset / ProfileLimits.AvatarPartBytes] = true;
                LastActivityUtc = nowUtc;
                writeResult = CheckCompletedThumbnail();
            }

            return writeResult;
        }

        internal void Touch(DateTime nowUtc)
        {
            LastActivityUtc = nowUtc;
        }

        internal CompletedAvatarUpload ToCompleted()
        {
            var completedUpload = new CompletedAvatarUpload(_thumbnail);

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

        private SendAvatarPartResult CheckCompletedThumbnail()
        {
            bool isUndecodable = IsComplete() && !AvatarImageCheck.IsDecodableThumbnail(_thumbnail);
            SendAvatarPartResult completionResult = isUndecodable ? SendAvatarPartResult.ThumbnailInvalid : SendAvatarPartResult.Accepted;

            return completionResult;
        }
    }
}
