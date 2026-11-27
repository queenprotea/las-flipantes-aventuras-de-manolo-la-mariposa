using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;
using Game.Persistence;

using log4net;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Game.Services
{
    public sealed class ProfileService : IProfileService
    {
        private const string ProfileFeatureName = "the profile";
        private const string ProfileUnavailableMessage = "The profile is not available right now.";
        private const string ProfileReadFailedMessage = "The database could not be read while player {0} opened their profile.";
        private const string ProfileWriteFailedMessage = "The database could not be used while player {0} updated their profile.";
        private const string ProfileMissingMessage = "Player {0} has an open session but their account was not found.";
        private const string AvatarSaveFailedMessage = "The avatar of player {0} could not be written to the disk.";
        private const string AvatarDeleteFailedMessage = "An avatar file of player {0} could not be deleted and stays on the disk.";
        private const string ThumbnailReadFailedMessage = "An avatar thumbnail asked for by session owner {0} could not be read from the disk.";
        private const string ProfileUpdatedMessage = "Player {0} updated their profile.";
        private const string AccountDeleteFailedMessage = "The database could not be used while player {0} deleted their account.";
        private const string EmptyMatchesKeptMessage = "The matches left without participants by player {0} could not be deleted and stay in the database.";
        private const string AccountDeletedMessage = "Player {0} deleted their account.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(ProfileService));

        private readonly SessionRegistry _sessionRegistry;
        private readonly PlayerRepository _playerRepository;
        private readonly AvatarStore _avatarStore;
        private readonly AvatarUploadRegistry _avatarUploadRegistry;
        private readonly RoomService _roomService;
        private readonly MatchRecordRepository _matchRecordRepository;

        public ProfileService(SessionRegistry sessionRegistry, PlayerRepository playerRepository, AvatarStore avatarStore, AvatarUploadRegistry avatarUploadRegistry, RoomService roomService, MatchRecordRepository matchRecordRepository)
        {
            _sessionRegistry = sessionRegistry;
            _playerRepository = playerRepository;
            _avatarStore = avatarStore;
            _avatarUploadRegistry = avatarUploadRegistry;
            _roomService = roomService;
            _matchRecordRepository = matchRecordRepository;
        }

        /// <inheritdoc/>
        public async Task<ProfileResponse> GetProfileAsync()
        {
            int callerPlayerId = RequireCallerPlayerId();
            Player? player;
            try
            {
                player = await _playerRepository.FindByIdAsync(callerPlayerId);
            }
            catch (NpgsqlException exception)
            {
                _log.Error(string.Format(CultureInfo.InvariantCulture, ProfileReadFailedMessage, callerPlayerId), exception);
                throw new FaultException(ProfileUnavailableMessage);
            }

            if (player is null)
            {
                _log.WarnFormat(CultureInfo.InvariantCulture, ProfileMissingMessage, callerPlayerId);
                throw new FaultException(ProfileUnavailableMessage);
            }

            var profile = new ProfileResponse
            {
                Username = player.Username,
                Email = player.Email,
                AvatarReference = player.AvatarReference,
            };

            return profile;
        }

        /// <inheritdoc/>
        public BeginAvatarUploadResult BeginAvatarUpload(int originalBytes, int thumbnailBytes)
        {
            int callerPlayerId = RequireCallerPlayerId();
            BeginAvatarUploadResult beginResult;
            if (_roomService.HasMatchInProgress(callerPlayerId))
            {
                _avatarUploadRegistry.DiscardForMatch(callerPlayerId);
                beginResult = BeginAvatarUploadResult.RoomInMatch;
            }
            else
            {
                beginResult = _avatarUploadRegistry.Begin(callerPlayerId, originalBytes, thumbnailBytes);
            }

            return beginResult;
        }

        /// <inheritdoc/>
        public SendAvatarPartResult SendAvatarPart(AvatarFileKind fileKind, int offset, byte[] part)
        {
            int callerPlayerId = RequireCallerPlayerId();
            bool isWellFormedPart = (part is { Length: > 0 }) && Enum.IsDefined(fileKind);
            SendAvatarPartResult partResult;
            if (_roomService.HasMatchInProgress(callerPlayerId))
            {
                _avatarUploadRegistry.DiscardForMatch(callerPlayerId);
                partResult = SendAvatarPartResult.RoomInMatch;
            }
            else if (isWellFormedPart)
            {
                partResult = _avatarUploadRegistry.AcceptPart(callerPlayerId, new ReceivedAvatarPart(fileKind, offset, part));
            }
            else
            {
                partResult = SendAvatarPartResult.OutOfRange;
            }

            return partResult;
        }

        /// <inheritdoc/>
        public void CancelAvatarUpload()
        {
            int callerPlayerId = RequireCallerPlayerId();
            _avatarUploadRegistry.Cancel(callerPlayerId);
        }

        /// <inheritdoc/>
        public async Task<UpdateProfileResult> UpdateProfileAsync(string username, bool includesNewAvatar)
        {
            PlayerSession callerSession = AccountOwnerGuard.RequireAccountSession(_sessionRegistry, _log, ProfileFeatureName);
            int callerPlayerId = callerSession.Owner.OwnerId;
            string trimmedUsername = (username ?? string.Empty).Trim();
            UpdateProfileStatus? rejectionStatus = FindRejection(callerPlayerId, trimmedUsername);
            if (rejectionStatus is UpdateProfileStatus rejectedStatus)
            {
                var rejectedResult = new UpdateProfileResult { Status = rejectedStatus };
                return rejectedResult;
            }

            CompletedAvatarUpload? completedUpload = includesNewAvatar ? _avatarUploadRegistry.FindCompleted(callerPlayerId) : null;
            bool isUploadMissing = includesNewAvatar && (completedUpload is null);
            UpdateProfileResult updateResult = isUploadMissing
                ? new UpdateProfileResult { Status = UpdateProfileStatus.AvatarIncomplete }
                : await SaveProfileAsync(callerSession, trimmedUsername, completedUpload);

            return updateResult;
        }

        /// <inheritdoc/>
        public async Task<DeleteAccountResult> DeleteAccountAsync()
        {
            PlayerSession callerSession = AccountOwnerGuard.RequireAccountSession(_sessionRegistry, _log, ProfileFeatureName);
            int callerPlayerId = callerSession.Owner.OwnerId;
            DeleteAccountResult deletionResult = _roomService.HasMatchInProgress(callerPlayerId)
                ? DeleteAccountResult.MatchInProgress
                : await DeleteStoredAccountAsync(callerPlayerId);
            if (deletionResult == DeleteAccountResult.Deleted)
            {
                await LeaveAfterDeletionAsync(callerSession);
            }

            return deletionResult;
        }

        /// <inheritdoc/>
        public async Task<AvatarResponse> GetAvatarThumbnailAsync(string avatarReference)
        {
            PlayerSession callerSession = AccountOwnerGuard.RequireSession(_sessionRegistry);
            byte[]? thumbnail = string.IsNullOrWhiteSpace(avatarReference) ? null : await ReadThumbnailAsync(avatarReference, callerSession.Owner.OwnerId);
            var avatarResponse = new AvatarResponse
            {
                Status = thumbnail is null ? AvatarLookupStatus.NotFound : AvatarLookupStatus.Found,
                Image = thumbnail,
            };

            return avatarResponse;
        }

        private static UpdateProfileStatus ToUpdateStatus(ProfileUpdateOutcome updateOutcome)
        {
            UpdateProfileStatus updateStatus = updateOutcome switch
            {
                ProfileUpdateOutcome.Updated => UpdateProfileStatus.Updated,
                ProfileUpdateOutcome.UsernameTaken => UpdateProfileStatus.UsernameTaken,
                ProfileUpdateOutcome.PlayerNotFound => UpdateProfileStatus.DatabaseUnavailable,
                _ => throw new ArgumentOutOfRangeException(nameof(updateOutcome)),
            };

            return updateStatus;
        }

        private int RequireCallerPlayerId()
        {
            int callerPlayerId = AccountOwnerGuard.RequireAccountOwner(_sessionRegistry, _log, ProfileFeatureName).OwnerId;

            return callerPlayerId;
        }

        private UpdateProfileStatus? FindRejection(int playerId, string username)
        {
            UpdateProfileStatus? rejectionStatus = null;
            if (!UsernameRules.IsValid(username))
            {
                rejectionStatus = UpdateProfileStatus.InvalidUsername;
            }
            else if (_roomService.HasMatchInProgress(playerId))
            {
                rejectionStatus = UpdateProfileStatus.RoomInMatch;
            }

            return rejectionStatus;
        }

        private async Task<UpdateProfileResult> SaveProfileAsync(PlayerSession callerSession, string username, CompletedAvatarUpload? completedUpload)
        {
            int playerId = callerSession.Owner.OwnerId;
            UpdateProfileResult saveResult;
            try
            {
                string? previousReference = await _playerRepository.FindAvatarReferenceAsync(playerId);
                var profileChange = new ProfileChange(username, previousReference, previousReference);
                saveResult = completedUpload is null
                    ? await WriteProfileAsync(callerSession, profileChange)
                    : await SaveWithNewAvatarAsync(callerSession, profileChange, completedUpload);
            }
            catch (NpgsqlException exception)
            {
                saveResult = LogSaveFailure(playerId, exception);
            }
            catch (DbUpdateException exception)
            {
                saveResult = LogSaveFailure(playerId, exception);
            }

            return saveResult;
        }

        private static UpdateProfileResult LogSaveFailure(int playerId, Exception exception)
        {
            _log.Error(string.Format(CultureInfo.InvariantCulture, ProfileWriteFailedMessage, playerId), exception);
            var failedResult = new UpdateProfileResult { Status = UpdateProfileStatus.DatabaseUnavailable };

            return failedResult;
        }

        private async Task<UpdateProfileResult> SaveWithNewAvatarAsync(PlayerSession callerSession, ProfileChange profileChange, CompletedAvatarUpload completedUpload)
        {
            int playerId = callerSession.Owner.OwnerId;
            string? newReference = StoreAvatar(playerId, completedUpload);
            if (newReference is null)
            {
                var storageFailure = new UpdateProfileResult { Status = UpdateProfileStatus.StorageUnavailable };
                return storageFailure;
            }

            UpdateProfileResult writeResult;
            try
            {
                writeResult = await WriteProfileAsync(callerSession, profileChange with { NewReference = newReference });
            }
            catch (NpgsqlException)
            {
                DeleteAvatarFiles(newReference, playerId);
                throw;
            }
            catch (DbUpdateException)
            {
                DeleteAvatarFiles(newReference, playerId);
                throw;
            }

            bool isUpdated = writeResult.Status == UpdateProfileStatus.Updated;
            string? replacedReference = isUpdated ? profileChange.PreviousReference : newReference;
            if (isUpdated)
            {
                _avatarUploadRegistry.Release(playerId);
            }

            if (replacedReference is not null)
            {
                DeleteAvatarFiles(replacedReference, playerId);
            }

            return writeResult;
        }

        private async Task<UpdateProfileResult> WriteProfileAsync(PlayerSession callerSession, ProfileChange profileChange)
        {
            int playerId = callerSession.Owner.OwnerId;
            ProfileUpdateOutcome updateOutcome = await _playerRepository.UpdateProfileAsync(playerId, profileChange.Username, profileChange.NewReference);
            UpdateProfileStatus updateStatus = ToUpdateStatus(updateOutcome);
            if (updateOutcome == ProfileUpdateOutcome.PlayerNotFound)
            {
                _log.WarnFormat(CultureInfo.InvariantCulture, ProfileMissingMessage, playerId);
            }
            else if (updateStatus == UpdateProfileStatus.Updated)
            {
                callerSession.ApplyUsername(profileChange.Username);
                _roomService.ApplyProfile(playerId, profileChange.Username, profileChange.NewReference);
                _log.InfoFormat(CultureInfo.InvariantCulture, ProfileUpdatedMessage, playerId);
            }

            var writeResult = new UpdateProfileResult
            {
                Status = updateStatus,
                AvatarReference = updateStatus == UpdateProfileStatus.Updated ? profileChange.NewReference : null,
            };

            return writeResult;
        }

        private async Task<DeleteAccountResult> DeleteStoredAccountAsync(int playerId)
        {
            DeleteAccountResult deletionResult;
            try
            {
                deletionResult = await _matchRecordRepository.HasInProgressByPlayerAsync(playerId)
                    ? DeleteAccountResult.MatchInProgress
                    : await RemoveAccountAsync(playerId);
            }
            catch (NpgsqlException exception)
            {
                deletionResult = LogDeletionFailure(playerId, exception);
            }
            catch (DbUpdateException exception)
            {
                deletionResult = LogDeletionFailure(playerId, exception);
            }

            return deletionResult;
        }

        private static DeleteAccountResult LogDeletionFailure(int playerId, Exception exception)
        {
            _log.Error(string.Format(CultureInfo.InvariantCulture, AccountDeleteFailedMessage, playerId), exception);
            DeleteAccountResult failedResult = DeleteAccountResult.DatabaseUnavailable;

            return failedResult;
        }

        private async Task<DeleteAccountResult> RemoveAccountAsync(int playerId)
        {
            string? avatarReference = await _playerRepository.FindAvatarReferenceAsync(playerId);
            List<int> playedMatchIds = await _matchRecordRepository.ListIdsByPlayerAsync(playerId);
            bool wasRemoved = await _playerRepository.DeleteAsync(playerId);
            if (!wasRemoved)
            {
                _log.WarnFormat(CultureInfo.InvariantCulture, ProfileMissingMessage, playerId);
                DeleteAccountResult missingResult = DeleteAccountResult.DatabaseUnavailable;
                return missingResult;
            }

            if (avatarReference is not null)
            {
                DeleteAvatarFiles(avatarReference, playerId);
            }

            await DeleteEmptyMatchesAsync(playerId, playedMatchIds);
            _log.InfoFormat(CultureInfo.InvariantCulture, AccountDeletedMessage, playerId);
            DeleteAccountResult deletedResult = DeleteAccountResult.Deleted;

            return deletedResult;
        }

        private async Task DeleteEmptyMatchesAsync(int playerId, List<int> playedMatchIds)
        {
            try
            {
                await _matchRecordRepository.DeleteFinishedWithoutParticipantsAsync(playedMatchIds);
            }
            catch (NpgsqlException exception)
            {
                _log.Warn(string.Format(CultureInfo.InvariantCulture, EmptyMatchesKeptMessage, playerId), exception);
            }
        }

        private async Task LeaveAfterDeletionAsync(PlayerSession callerSession)
        {
            int playerId = callerSession.Owner.OwnerId;
            _avatarUploadRegistry.DiscardForClosedSession(playerId);
            await _roomService.LeaveForClosedSessionAsync(playerId);
            _sessionRegistry.Close(callerSession);
        }

        private string? StoreAvatar(int playerId, CompletedAvatarUpload completedUpload)
        {
            string? avatarReference = null;
            try
            {
                avatarReference = _avatarStore.Save(playerId, completedUpload);
            }
            catch (IOException exception)
            {
                _log.Error(string.Format(CultureInfo.InvariantCulture, AvatarSaveFailedMessage, playerId), exception);
            }
            catch (UnauthorizedAccessException exception)
            {
                _log.Error(string.Format(CultureInfo.InvariantCulture, AvatarSaveFailedMessage, playerId), exception);
            }

            return avatarReference;
        }

        private void DeleteAvatarFiles(string avatarReference, int playerId)
        {
            try
            {
                _avatarStore.Delete(avatarReference);
            }
            catch (IOException exception)
            {
                _log.Warn(string.Format(CultureInfo.InvariantCulture, AvatarDeleteFailedMessage, playerId), exception);
            }
            catch (UnauthorizedAccessException exception)
            {
                _log.Warn(string.Format(CultureInfo.InvariantCulture, AvatarDeleteFailedMessage, playerId), exception);
            }
        }

        private async Task<byte[]?> ReadThumbnailAsync(string avatarReference, int readerOwnerId)
        {
            byte[]? thumbnail = null;
            try
            {
                thumbnail = await _avatarStore.ReadThumbnailAsync(avatarReference);
            }
            catch (IOException exception)
            {
                _log.Warn(string.Format(CultureInfo.InvariantCulture, ThumbnailReadFailedMessage, readerOwnerId), exception);
            }
            catch (UnauthorizedAccessException exception)
            {
                _log.Warn(string.Format(CultureInfo.InvariantCulture, ThumbnailReadFailedMessage, readerOwnerId), exception);
            }

            return thumbnail;
        }
    }
}
