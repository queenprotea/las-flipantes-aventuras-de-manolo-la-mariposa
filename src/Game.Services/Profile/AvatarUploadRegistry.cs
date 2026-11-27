using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Game.Contracts;

using log4net;

namespace Game.Services
{
    public sealed class AvatarUploadRegistry
    {
        private const string UploadDiscardedMessage = "The unfinished avatar upload of player {0} was discarded: {1}.";
        private const string CancelledReason = "cancelled by the player";
        private const string RejectedImageReason = "it was not an accepted image";
        private const string SessionLostReason = "the session was lost";
        private const string SessionClosedReason = "the session was closed";
        private const string MatchInProgressReason = "the room of the player is in a match";
        private const string IdleReason = "no part arrived in time";

        private static readonly ILog _log = LogManager.GetLogger(typeof(AvatarUploadRegistry));

        private readonly Lock _registryLock = new Lock();
        private readonly Dictionary<int, PendingAvatarUpload> _uploadsByOwnerId = new Dictionary<int, PendingAvatarUpload>();
        private readonly TimeProvider _timeProvider;
        private readonly TimeSpan _idleLimit;

        public AvatarUploadRegistry(SessionRegistry sessionRegistry, TimeProvider timeProvider, AvatarUploadTiming uploadTiming)
        {
            _timeProvider = timeProvider;
            _idleLimit = uploadTiming.IdleLimit;
            sessionRegistry.SessionLost += SessionRegistryOnSessionLost;
        }

        internal BeginAvatarUploadResult Begin(int ownerId, int originalBytes, int thumbnailBytes)
        {
            BeginAvatarUploadResult beginResult = JudgeSizes(originalBytes, thumbnailBytes);
            lock (_registryLock)
            {
                _uploadsByOwnerId.Remove(ownerId);
                if (beginResult == BeginAvatarUploadResult.Started)
                {
                    _uploadsByOwnerId[ownerId] = new PendingAvatarUpload(originalBytes, thumbnailBytes, ReadNow());
                }
            }

            return beginResult;
        }

        internal SendAvatarPartResult AcceptPart(int ownerId, ReceivedAvatarPart receivedPart)
        {
            SendAvatarPartResult partResult = SendAvatarPartResult.NoUploadInProgress;
            lock (_registryLock)
            {
                PendingAvatarUpload? pendingUpload = _uploadsByOwnerId.GetValueOrDefault(ownerId);
                if (pendingUpload is not null)
                {
                    partResult = pendingUpload.Write(receivedPart, ReadNow());
                }
            }

            if (IsRejectedImage(partResult))
            {
                Discard(ownerId, RejectedImageReason);
            }

            return partResult;
        }

        internal CompletedAvatarUpload? FindCompleted(int ownerId)
        {
            CompletedAvatarUpload? completedUpload = null;
            lock (_registryLock)
            {
                PendingAvatarUpload? pendingUpload = _uploadsByOwnerId.GetValueOrDefault(ownerId);
                if ((pendingUpload is not null) && pendingUpload.IsComplete())
                {
                    pendingUpload.Touch(ReadNow());
                    completedUpload = pendingUpload.ToCompleted();
                }
            }

            return completedUpload;
        }

        internal void Release(int ownerId)
        {
            lock (_registryLock)
            {
                _uploadsByOwnerId.Remove(ownerId);
            }
        }

        internal void Cancel(int ownerId)
        {
            Discard(ownerId, CancelledReason);
        }

        internal void DiscardForMatch(int ownerId)
        {
            Discard(ownerId, MatchInProgressReason);
        }

        internal void DiscardForClosedSession(int ownerId)
        {
            Discard(ownerId, SessionClosedReason);
        }

        internal void DiscardIdle()
        {
            DateTime idleThresholdAt = ReadNow() - _idleLimit;
            List<int> idleOwnerIds;
            lock (_registryLock)
            {
                idleOwnerIds = _uploadsByOwnerId
                    .Where(pendingEntry => pendingEntry.Value.LastActivityAt < idleThresholdAt)
                    .Select(pendingEntry => pendingEntry.Key)
                    .ToList();
            }

            foreach (int idleOwnerId in idleOwnerIds)
            {
                Discard(idleOwnerId, IdleReason);
            }
        }

        private static BeginAvatarUploadResult JudgeSizes(int originalBytes, int thumbnailBytes)
        {
            BeginAvatarUploadResult sizeCheckResult = BeginAvatarUploadResult.Started;
            if ((originalBytes <= 0) || (originalBytes > ProfileLimits.LargestAvatarBytes))
            {
                sizeCheckResult = BeginAvatarUploadResult.AvatarTooLarge;
            }
            else if ((thumbnailBytes <= 0) || (thumbnailBytes > ProfileLimits.LargestThumbnailBytes))
            {
                sizeCheckResult = BeginAvatarUploadResult.ThumbnailInvalid;
            }

            return sizeCheckResult;
        }

        private static bool IsRejectedImage(SendAvatarPartResult partResult)
        {
            bool isRejectedImage = (partResult == SendAvatarPartResult.AvatarInvalidFormat)
                || (partResult == SendAvatarPartResult.ThumbnailInvalid);

            return isRejectedImage;
        }

        private void Discard(int ownerId, string discardReason)
        {
            bool wasUploadPending;
            lock (_registryLock)
            {
                wasUploadPending = _uploadsByOwnerId.Remove(ownerId);
            }

            if (wasUploadPending)
            {
                _log.InfoFormat(CultureInfo.InvariantCulture, UploadDiscardedMessage, ownerId, discardReason);
            }
        }

        private void SessionRegistryOnSessionLost(object? sender, SessionOwnerEventArgs e)
        {
            Discard(e.OwnerId, SessionLostReason);
        }

        private DateTime ReadNow()
        {
            DateTime now = _timeProvider.GetUtcNow().UtcDateTime;

            return now;
        }
    }
}
