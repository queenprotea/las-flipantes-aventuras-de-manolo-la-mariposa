using System;
using System.Collections.Generic;

using Game.Contracts;

using Torres.Client.Avatars;
using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class ProfileFlow
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly AvatarCache _avatarCache;
        private readonly Queue<ProfileOutcome> _outcomes = new Queue<ProfileOutcome>();
        private readonly PendingRequest<ProfileResponse> _profileRequest = new PendingRequest<ProfileResponse>();
        private readonly PendingRequest<AvatarPreparation> _preparationRequest = new PendingRequest<AvatarPreparation>();
        private readonly PendingRequest<UpdateProfileResult> _saveRequest = new PendingRequest<UpdateProfileResult>();

        private AvatarUpload? _upload;
        private PreparedAvatar? _preparedAvatar;
        private string _savedEmail = string.Empty;
        private string _requestedUsername = string.Empty;
        private bool _isSaveWaitingForUpload;
        private bool _wasUploadResent;

        internal ProfileFlow(GameSessionConnection gameSessionConnection, PlayerSession playerSession, AvatarCache avatarCache)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _avatarCache = avatarCache;
        }

        internal bool IsGuest => _playerSession.IsGuest;

        internal bool IsInRoom => _playerSession.CurrentRoom is not null;

        internal string DisplayName => _playerSession.DisplayName;

        internal string SavedUsername { get; private set; } = string.Empty;

        internal bool HasAvatarChange => _upload is not null;

        internal bool IsUploading => (_upload is not null) && (_upload.State != AvatarUploadState.Completed);

        internal float? UploadProgress => _upload?.Progress;

        internal bool IsSaveRequestWaiting => _saveRequest.IsWaiting;

        internal bool IsSaving => _saveRequest.IsWaiting || _isSaveWaitingForUpload;

        internal void Open()
        {
            DiscardAvatarChange();
            _saveRequest.Forget();
            _outcomes.Clear();
            if (_playerSession.Player is PlayerIdentity player)
            {
                ShowDetails(new ProfileDetails(player.Username, player.Email, _playerSession.AvatarReference), false);
                RequestProfile();
            }
        }

        internal void StartAvatarPreparation(string filePath)
        {
            DiscardAvatarChange();
            _preparationRequest.Start(new BackgroundCall<string, AvatarPreparation>(AvatarThumbnailMaker.Prepare, filePath).StartAsync());
        }

        internal void RequestSave(string username)
        {
            _requestedUsername = username.Trim();
            if (!_gameSessionConnection.IsSessionOpen)
            {
                AddMessage(ProfileMessageTarget.Save, TextKeys.Common.ServerErrorTitle);
            }
            else if (IsUploading)
            {
                _isSaveWaitingForUpload = true;
            }
            else
            {
                StartSave();
            }
        }

        internal void DiscardAvatarChange()
        {
            _upload?.Cancel();
            _upload = null;
            _preparedAvatar = null;
            _preparationRequest.Forget();
            _isSaveWaitingForUpload = false;
        }

        internal void Follow()
        {
            FollowProfileRequest();
            FollowPreparationRequest();
            FollowUpload();
            FollowSaveRequest();
        }

        internal ProfileOutcome? TakeOutcome()
        {
            _outcomes.TryDequeue(out ProfileOutcome? outcome);

            return outcome;
        }

        private void ShowDetails(ProfileDetails details, bool isSaved)
        {
            SavedUsername = details.Username;
            _savedEmail = details.Email;
            _outcomes.Enqueue(new ProfileOutcome(details, null, null, isSaved));
        }

        private void AddMessage(ProfileMessageTarget target, string textKey)
        {
            _outcomes.Enqueue(new ProfileOutcome(null, new ProfileMessage(target, textKey, null), null, false));
        }

        private void RequestProfile()
        {
            _profileRequest.Forget();
            if (_gameSessionConnection.IsSessionOpen)
            {
                _profileRequest.Start(_gameSessionConnection.GetProfileAsync());
            }
            else
            {
                AddMessage(ProfileMessageTarget.Save, TextKeys.Common.ServerErrorTitle);
            }
        }

        private void FollowProfileRequest()
        {
            FinishedRequest<ProfileResponse>? finishedProfile = _profileRequest.TakeFinished();
            if (finishedProfile is null)
            {
                return;
            }

            if (finishedProfile.IsSuccessful && (finishedProfile.Reply is ProfileResponse profile))
            {
                ShowDetails(new ProfileDetails(profile.Username, profile.Email, profile.AvatarReference), false);
                _playerSession.ApplyProfile(profile.Username, profile.AvatarReference);
            }
            else
            {
                AddMessage(ProfileMessageTarget.Save, TextKeys.Common.ServerErrorTitle);
            }
        }

        private void FollowPreparationRequest()
        {
            FinishedRequest<AvatarPreparation>? finishedPreparation = _preparationRequest.TakeFinished();
            if (finishedPreparation is null)
            {
                return;
            }

            AvatarPreparation? preparation = finishedPreparation.Reply;
            if (preparation?.Avatar is PreparedAvatar preparedAvatar)
            {
                _preparedAvatar = preparedAvatar;
                _wasUploadResent = false;
                _outcomes.Enqueue(new ProfileOutcome(null, null, preparedAvatar, false));
                StartUpload();
            }
            else if ((preparation is not null) && (preparation.Status == AvatarPreparationStatus.TooLarge))
            {
                var tooLargeMessage = new ProfileMessage(ProfileMessageTarget.Avatar, TextKeys.Profile.AvatarTooLarge, preparation.FileBytes);
                _outcomes.Enqueue(new ProfileOutcome(null, tooLargeMessage, null, false));
            }
            else
            {
                AddMessage(ProfileMessageTarget.Avatar, TextKeys.Profile.AvatarInvalidFormat);
            }
        }

        private void StartUpload()
        {
            if ((_preparedAvatar is null) || !_gameSessionConnection.IsSessionOpen)
            {
                AddMessage(ProfileMessageTarget.Save, TextKeys.Common.ServerErrorTitle);
                return;
            }

            _upload = new AvatarUpload(_gameSessionConnection, _preparedAvatar);
        }

        private void FollowUpload()
        {
            if (_upload is null)
            {
                return;
            }

            _upload.Update();
            if (_upload.State == AvatarUploadState.Failed)
            {
                AvatarUploadFailure failure = _upload.Failure;
                DiscardAvatarChange();
                ShowUploadFailure(failure);
            }
            else if (_isSaveWaitingForUpload && (_upload.State == AvatarUploadState.Completed))
            {
                StartSave();
            }
        }

        private void ShowUploadFailure(AvatarUploadFailure failure)
        {
            switch (failure)
            {
                case AvatarUploadFailure.InvalidFormat:
                    AddMessage(ProfileMessageTarget.Avatar, TextKeys.Profile.AvatarInvalidFormat);
                    break;

                case AvatarUploadFailure.RoomInMatch:
                    AddMessage(ProfileMessageTarget.Save, TextKeys.Profile.MatchInProgress);
                    break;

                case AvatarUploadFailure.ServerError:
                case AvatarUploadFailure.None:
                    AddMessage(ProfileMessageTarget.Save, TextKeys.Common.ServerErrorTitle);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(failure));
            }
        }

        private void StartSave()
        {
            _isSaveWaitingForUpload = false;
            bool includesNewAvatar = _upload?.State == AvatarUploadState.Completed;
            if (!_saveRequest.IsWaiting)
            {
                _saveRequest.Start(_gameSessionConnection.UpdateProfileAsync(_requestedUsername, includesNewAvatar));
            }
        }

        private void FollowSaveRequest()
        {
            FinishedRequest<UpdateProfileResult>? finishedSave = _saveRequest.TakeFinished();
            if (finishedSave is null)
            {
                return;
            }

            var saveResult = new UpdateProfileResult { Status = UpdateProfileStatus.DatabaseUnavailable };
            if (finishedSave.IsSuccessful && (finishedSave.Reply is UpdateProfileResult savedProfile))
            {
                saveResult = savedProfile;
            }

            FollowSaveResult(saveResult);
        }

        private void FollowSaveResult(UpdateProfileResult saveResult)
        {
            switch (saveResult.Status)
            {
                case UpdateProfileStatus.Updated:
                    ApplySavedProfile(saveResult.AvatarReference);
                    break;

                case UpdateProfileStatus.UsernameTaken:
                    AddMessage(ProfileMessageTarget.Username, TextKeys.Profile.UsernameUnavailable);
                    break;

                case UpdateProfileStatus.InvalidUsername:
                    AddMessage(ProfileMessageTarget.Username, TextKeys.Register.UsernameInvalidFormat);
                    break;

                case UpdateProfileStatus.AvatarIncomplete:
                    ResendUpload();
                    break;

                case UpdateProfileStatus.RoomInMatch:
                    AddMessage(ProfileMessageTarget.Save, TextKeys.Profile.MatchInProgress);
                    break;

                case UpdateProfileStatus.DatabaseUnavailable:
                case UpdateProfileStatus.StorageUnavailable:
                    AddMessage(ProfileMessageTarget.Save, TextKeys.Common.ServerErrorTitle);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(saveResult));
            }
        }

        private void ApplySavedProfile(string? avatarReference)
        {
            if ((avatarReference is not null) && (_preparedAvatar is not null) && (_upload is not null))
            {
                _avatarCache.Put(avatarReference, _preparedAvatar.Thumbnail);
            }

            _upload = null;
            _preparedAvatar = null;
            ShowDetails(new ProfileDetails(_requestedUsername, _savedEmail, avatarReference), true);
            _playerSession.ApplyProfile(_requestedUsername, avatarReference);
        }

        private void ResendUpload()
        {
            if (_wasUploadResent || (_preparedAvatar is null))
            {
                DiscardAvatarChange();
                AddMessage(ProfileMessageTarget.Save, TextKeys.Common.ServerErrorTitle);
                return;
            }

            _wasUploadResent = true;
            StartUpload();
            _isSaveWaitingForUpload = _upload is not null;
        }
    }
}
