using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Session;

namespace Torres.Client.Avatars
{
    internal sealed class AvatarUpload
    {
        private const int MostPartsInFlight = 4;

        private readonly GameSessionConnection _gameSessionConnection;
        private readonly Queue<AvatarPart> _pendingParts = new Queue<AvatarPart>();
        private readonly Dictionary<Task<SendAvatarPartResult>, int> _bytesByPartSending = new Dictionary<Task<SendAvatarPartResult>, int>();
        private readonly int _totalBytes;

        private Task<BeginAvatarUploadResult>? _beginRequest;
        private int _acceptedBytes;

        internal AvatarUpload(GameSessionConnection gameSessionConnection, PreparedAvatar preparedAvatar)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(preparedAvatar);

            _gameSessionConnection = gameSessionConnection;
            QueueParts(AvatarFileKind.Thumbnail, preparedAvatar.Thumbnail);
            QueueParts(AvatarFileKind.Original, preparedAvatar.Original);
            _totalBytes = preparedAvatar.Thumbnail.Length + preparedAvatar.Original.Length;
            _beginRequest = gameSessionConnection.BeginAvatarUploadAsync(preparedAvatar);
        }

        internal AvatarUploadState State { get; private set; } = AvatarUploadState.Starting;

        internal AvatarUploadFailure Failure { get; private set; } = AvatarUploadFailure.None;

        internal float Progress => (float)_acceptedBytes / _totalBytes;

        internal void Update()
        {
            if (State == AvatarUploadState.Starting)
            {
                FollowBeginRequest();
            }
            else if (State == AvatarUploadState.Sending)
            {
                FollowPartSendings();
                StartPartSendings();
            }
        }

        internal void Cancel()
        {
            bool isServerHoldingParts = (State != AvatarUploadState.Cancelled) && (State != AvatarUploadState.Failed);
            State = AvatarUploadState.Cancelled;
            _pendingParts.Clear();
            if (isServerHoldingParts && _gameSessionConnection.IsSessionOpen)
            {
                _gameSessionConnection.RequestAvatarUploadCancellation();
            }
        }

        private static AvatarUploadFailure ToFailure(BeginAvatarUploadResult beginResult)
        {
            AvatarUploadFailure failure = beginResult == BeginAvatarUploadResult.RoomInMatch ? AvatarUploadFailure.RoomInMatch : AvatarUploadFailure.ServerError;

            return failure;
        }

        private static AvatarUploadFailure ToFailure(SendAvatarPartResult partResult)
        {
            AvatarUploadFailure failure = partResult switch
            {
                SendAvatarPartResult.RoomInMatch => AvatarUploadFailure.RoomInMatch,
                SendAvatarPartResult.AvatarInvalidFormat => AvatarUploadFailure.InvalidFormat,
                SendAvatarPartResult.NoUploadInProgress => AvatarUploadFailure.ServerError,
                SendAvatarPartResult.OutOfRange => AvatarUploadFailure.ServerError,
                SendAvatarPartResult.ThumbnailInvalid => AvatarUploadFailure.ServerError,
                _ => throw new ArgumentOutOfRangeException(nameof(partResult)),
            };

            return failure;
        }

        private void QueueParts(AvatarFileKind fileKind, byte[] fileBytes)
        {
            for (var partOffset = 0; partOffset < fileBytes.Length; partOffset += ProfileLimits.AvatarPartBytes)
            {
                int partBytes = Math.Min(ProfileLimits.AvatarPartBytes, fileBytes.Length - partOffset);
                _pendingParts.Enqueue(new AvatarPart(fileKind, partOffset, fileBytes.AsSpan(partOffset, partBytes).ToArray()));
            }
        }

        private void FollowBeginRequest()
        {
            if ((_beginRequest is null) || !_beginRequest.IsCompleted)
            {
                return;
            }

            if (!_beginRequest.IsCompletedSuccessfully)
            {
                Fail(AvatarUploadFailure.ServerError);
            }
            else if (_beginRequest.Result == BeginAvatarUploadResult.Started)
            {
                State = AvatarUploadState.Sending;
            }
            else
            {
                Fail(ToFailure(_beginRequest.Result));
            }

            _beginRequest = null;
        }

        private void FollowPartSendings()
        {
            List<Task<SendAvatarPartResult>> finishedSendings = _bytesByPartSending.Keys.Where(partSending => partSending.IsCompleted).ToList();
            foreach (Task<SendAvatarPartResult> finishedSending in finishedSendings)
            {
                int partBytes = _bytesByPartSending[finishedSending];
                _bytesByPartSending.Remove(finishedSending);
                FollowPartSending(finishedSending, partBytes);
            }

            bool isEverythingAccepted = (State == AvatarUploadState.Sending) && (_pendingParts.Count == 0) && (_bytesByPartSending.Count == 0);
            if (isEverythingAccepted)
            {
                State = AvatarUploadState.Completed;
            }
        }

        private void FollowPartSending(Task<SendAvatarPartResult> finishedSending, int partBytes)
        {
            if (!finishedSending.IsCompletedSuccessfully)
            {
                Fail(AvatarUploadFailure.ServerError);
            }
            else if (finishedSending.Result == SendAvatarPartResult.Accepted)
            {
                _acceptedBytes += partBytes;
            }
            else
            {
                Fail(ToFailure(finishedSending.Result));
            }
        }

        private void StartPartSendings()
        {
            while ((State == AvatarUploadState.Sending) && (_bytesByPartSending.Count < MostPartsInFlight) && (_pendingParts.Count > 0))
            {
                AvatarPart nextPart = _pendingParts.Dequeue();
                _bytesByPartSending[_gameSessionConnection.SendAvatarPartAsync(nextPart)] = nextPart.Bytes.Length;
            }
        }

        private void Fail(AvatarUploadFailure failure)
        {
            if ((State != AvatarUploadState.Sending) && (State != AvatarUploadState.Starting))
            {
                return;
            }

            State = AvatarUploadState.Failed;
            Failure = failure;
            _pendingParts.Clear();
        }
    }
}
