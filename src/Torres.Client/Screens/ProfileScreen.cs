using System;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;

using Torres.Client.Avatars;
using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class ProfileScreen : Screen
    {
        private const int AvatarSpacing = 14;
        private const int AvatarHintSpacing = 10;
        private const string AvatarFileFilter = "*.png|*.jpg|*.jpeg";
        private const float UploadProgressStart = 0f;
        private const float UploadProgressEnd = 1f;

        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _session;
        private readonly AvatarCache _avatarCache;
        private readonly AvatarView _avatarView;
        private readonly LabeledTextBox _usernameField = new LabeledTextBox(TextKeys.Profile.UsernameLabel, false);
        private readonly LabeledTextBox _emailField = new LabeledTextBox(TextKeys.Profile.EmailLabel, false);
        private readonly LocalizedLabel _avatarErrorLabel = CreateErrorMessage(TextKeys.Profile.AvatarInvalidFormat);
        private readonly LocalizedLabel _avatarMissingLabel = CreateHint(TextKeys.Profile.AvatarMissing);
        private readonly LocalizedLabel _usernameErrorLabel = CreateErrorMessage(TextKeys.Profile.UsernameUnavailable);
        private readonly LocalizedLabel _saveErrorLabel = CreateErrorMessage(TextKeys.Common.ServerErrorTitle);
        private readonly LocalizedLabel _savedLabel = CreateSuccessMessage(TextKeys.Profile.SavedMessage);
        private readonly HorizontalProgressBar _uploadProgressBar = new HorizontalProgressBar();
        private readonly HorizontalStackPanel _accountColumns;
        private readonly HorizontalStackPanel _guestColumns;
        private readonly Panel _contentPlaceholder = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        private readonly Label _guestAliasLabel;
        private readonly LocalizedButton _saveButton;
        private readonly LocalizedButton _logOutButton;
        private readonly LocalizedButton _backToMenuButton;
        private readonly LocalizedButton _backToRoomButton;

        private Task<ProfileResponse>? _profileRequest;
        private Task<AvatarPreparation>? _preparationRequest;
        private Task<UpdateProfileResult>? _saveRequest;
        private AvatarUpload? _upload;
        private PreparedAvatar? _preparedAvatar;
        private string _savedUsername = string.Empty;
        private bool _isSaveWaitingForUpload;
        private bool _wasUploadResent;

        internal ProfileScreen(GameSessionConnection gameSessionConnection, PlayerSession session, AvatarCache avatarCache)
            : base(TextKeys.Profile.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(session);
            ArgumentNullException.ThrowIfNull(avatarCache);

            _gameSessionConnection = gameSessionConnection;
            _session = session;
            _avatarCache = avatarCache;
            _avatarView = new AvatarView(avatarCache, Sizes.LargeAvatarSize);
            _accountColumns = CreateColumns();
            _guestColumns = CreateColumns();
            _guestAliasLabel = CreatePlayerNameLabel(string.Empty, Theme.Ink);
            _saveButton = CreatePrimaryButton(TextKeys.Profile.SaveButton);
            _logOutButton = CreateSecondaryButton(TextKeys.Profile.LogOutButton);
            _backToMenuButton = CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            _backToRoomButton = CreateSecondaryButton(TextKeys.Common.BackToRoomButton);
            _emailField.Box.Readonly = true;
            _usernameField.Box.ValueChanging += UsernameBoxOnValueChanging;
        }

        internal bool WasLogOutRequested { get; private set; }

        internal override void Open()
        {
            HideConfirmation();
            bool isGuest = _session.IsGuest;
            HorizontalStackPanel visibleColumns = isGuest ? _guestColumns : _accountColumns;
            _contentPlaceholder.Widgets.Clear();
            _contentPlaceholder.Widgets.Add(visibleColumns);
            _logOutButton.Visible = !isGuest;
            bool isInRoom = _session.CurrentRoom is not null;
            _backToMenuButton.Visible = !isInRoom;
            _backToRoomButton.Visible = isInRoom;
            _guestAliasLabel.Text = _session.DisplayName;
            DiscardAvatarChange();
            HideMessages();
            _saveRequest = null;
            if (_session.Player is PlayerIdentity player)
            {
                ShowProfile(player.Username, player.Email, _session.AvatarReference);
                RequestProfile();
            }
        }

        internal override void Close()
        {
            DiscardAvatarChange();
        }

        internal override void Update(GameTime gameTime)
        {
            FollowProfileRequest();
            FollowPreparationRequest();
            FollowUpload();
            FollowSaveRequest();
            RefreshControls();
        }

        internal void ClearRequests()
        {
            WasLogOutRequested = false;
        }

        protected override Widget Build()
        {
            _accountColumns.Widgets.Add(BuildAvatarCard());
            _accountColumns.Widgets.Add(BuildAccountCard());
            _guestColumns.Widgets.Add(BuildGuestCard());

            VerticalStackPanel page = CreatePage();
            page.Widgets.Add(ConfirmationPlaceholder);
            page.Widgets.Add(_contentPlaceholder);
            StackPanel.SetProportionType(_contentPlaceholder, ProportionType.Fill);

            _backToMenuButton.Click += BackToMenuButtonOnClick;
            _backToRoomButton.Click += BackToRoomButtonOnClick;
            _logOutButton.Click += LogOutButtonOnClick;
            Panel backBar = CreateBackBar(_backToMenuButton, _logOutButton);
            _backToRoomButton.HorizontalAlignment = HorizontalAlignment.Left;
            backBar.Widgets.Add(_backToRoomButton);
            page.Widgets.Add(backBar);

            return page;
        }

        private VerticalStackPanel BuildAvatarCard()
        {
            VerticalStackPanel card = CreateCard(Sizes.ProfileAvatarCardWidth);
            card.Spacing = 0;

            _avatarView.Root.HorizontalAlignment = HorizontalAlignment.Center;
            _avatarView.Root.Margin = new Thickness(0, 0, 0, AvatarSpacing);
            card.Widgets.Add(_avatarView.Root);

            LocalizedButton changeAvatarButton = CreateSmallSecondaryButton(TextKeys.Profile.ChangeAvatarButton);
            changeAvatarButton.HorizontalAlignment = HorizontalAlignment.Center;
            changeAvatarButton.Click += ChangeAvatarButtonOnClick;
            card.Widgets.Add(changeAvatarButton);

            LocalizedLabel formatHint = CreateHint(TextKeys.Profile.AvatarFormatHint);
            formatHint.HorizontalAlignment = HorizontalAlignment.Center;
            formatHint.Margin = new Thickness(0, AvatarHintSpacing, 0, 0);
            card.Widgets.Add(formatHint);

            _uploadProgressBar.Minimum = UploadProgressStart;
            _uploadProgressBar.Maximum = UploadProgressEnd;
            _uploadProgressBar.Margin = new Thickness(0, AvatarHintSpacing, 0, 0);
            _uploadProgressBar.HorizontalAlignment = HorizontalAlignment.Stretch;
            card.Widgets.Add(_uploadProgressBar);

            _avatarErrorLabel.Margin = new Thickness(0, AvatarHintSpacing, 0, 0);
            card.Widgets.Add(_avatarErrorLabel);
            _avatarMissingLabel.Margin = new Thickness(0, AvatarHintSpacing, 0, 0);
            card.Widgets.Add(_avatarMissingLabel);

            return card;
        }

        private VerticalStackPanel BuildAccountCard()
        {
            VerticalStackPanel card = CreateCard(Sizes.ProfileAccountCardWidth);
            card.Widgets.Add(CreateCardHeader(TextKeys.Profile.AccountTitle, TextKeys.Profile.AccountHint));
            card.Widgets.Add(_usernameField);
            card.Widgets.Add(_usernameErrorLabel);
            card.Widgets.Add(_emailField);

            _saveButton.Click += SaveButtonOnClick;
            LocalizedButton settingsButton = CreateSecondaryButton(TextKeys.Profile.AccountSettingsButton);
            settingsButton.Click += SettingsButtonOnClick;
            HorizontalStackPanel accountActions = CreateButtonRow();
            accountActions.Widgets.Add(_saveButton);
            accountActions.Widgets.Add(settingsButton);
            card.Widgets.Add(accountActions);
            card.Widgets.Add(_savedLabel);
            card.Widgets.Add(_saveErrorLabel);

            return card;
        }

        private VerticalStackPanel BuildGuestCard()
        {
            VerticalStackPanel card = CreateCard(Sizes.ProfileAccountCardWidth);
            card.Widgets.Add(CreateCardHeader(TextKeys.Profile.GuestTitle, TextKeys.Profile.GuestHint));

            HorizontalStackPanel aliasRow = CreateButtonRow();
            aliasRow.Widgets.Add(_guestAliasLabel);
            aliasRow.Widgets.Add(CreateStatusBadge(TextKeys.Common.GuestRole));
            card.Widgets.Add(aliasRow);

            card.Widgets.Add(CreateParagraph(TextKeys.GuestAccess.Limitations));

            LocalizedButton guestLogInButton = CreatePrimaryButton(TextKeys.Profile.GuestLogInButton);
            LocalizedButton guestCreateAccountButton = CreateSecondaryButton(TextKeys.Profile.GuestCreateAccountButton);
            guestLogInButton.Click += GuestLogInButtonOnClick;
            guestCreateAccountButton.Click += GuestCreateAccountButtonOnClick;
            HorizontalStackPanel guestActions = CreateButtonRow();
            guestActions.Widgets.Add(guestLogInButton);
            guestActions.Widgets.Add(guestCreateAccountButton);
            card.Widgets.Add(guestActions);

            return card;
        }

        private void ShowProfile(string username, string email, string? avatarReference)
        {
            _savedUsername = username;
            _usernameField.Box.Text = username;
            _emailField.Box.Text = email;
            _avatarView.AvatarReference = avatarReference;
        }

        private void RequestProfile()
        {
            _profileRequest = _gameSessionConnection.IsSessionOpen ? _gameSessionConnection.GetProfileAsync() : null;
            _saveErrorLabel.Visible = !_gameSessionConnection.IsSessionOpen;
        }

        private void FollowProfileRequest()
        {
            if ((_profileRequest is null) || !_profileRequest.IsCompleted)
            {
                return;
            }

            if (_profileRequest.IsCompletedSuccessfully)
            {
                ProfileResponse profile = _profileRequest.Result;
                ShowProfile(profile.Username, profile.Email, profile.AvatarReference);
                _session.ApplyProfile(profile.Username, profile.AvatarReference);
            }
            else
            {
                ShowSaveError(TextKeys.Common.ServerErrorTitle);
            }

            _profileRequest = null;
        }

        private void StartPreparation(string filePath)
        {
            DiscardAvatarChange();
            HideMessages();
            _preparationRequest = new SessionCall<string, AvatarPreparation>(AvatarThumbnailMaker.Prepare, filePath).StartAsync();
        }

        private void FollowPreparationRequest()
        {
            if ((_preparationRequest is null) || !_preparationRequest.IsCompleted)
            {
                return;
            }

            AvatarPreparation? preparation = _preparationRequest.IsCompletedSuccessfully ? _preparationRequest.Result : null;
            _preparationRequest = null;
            if (preparation?.Avatar is PreparedAvatar preparedAvatar)
            {
                ShowPreview(preparedAvatar);
            }
            else if (preparation?.Status == AvatarPreparationStatus.TooLarge)
            {
                ShowAvatarError(TextKeys.Profile.AvatarTooLarge, DisplayFormats.FormatMegabytes(preparation.FileBytes));
            }
            else
            {
                ShowAvatarError(TextKeys.Profile.AvatarInvalidFormat, null);
            }
        }

        private void ShowPreview(PreparedAvatar preparedAvatar)
        {
            _preparedAvatar = preparedAvatar;
            _avatarView.PreviewImage = _avatarCache.CreatePreview(preparedAvatar.Thumbnail);
            _wasUploadResent = false;
            StartUpload();
        }

        private void StartUpload()
        {
            if ((_preparedAvatar is null) || !_gameSessionConnection.IsSessionOpen)
            {
                ShowSaveError(TextKeys.Common.ServerErrorTitle);
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
                    ShowAvatarError(TextKeys.Profile.AvatarInvalidFormat, null);
                    break;

                case AvatarUploadFailure.RoomInMatch:
                    ShowSaveError(TextKeys.Profile.MatchInProgress);
                    break;

                case AvatarUploadFailure.ServerError:
                case AvatarUploadFailure.None:
                    ShowSaveError(TextKeys.Common.ServerErrorTitle);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(failure));
            }
        }

        private void RequestSave()
        {
            HideMessages();
            bool isUploading = (_upload is not null) && (_upload.State != AvatarUploadState.Completed);
            if (!_gameSessionConnection.IsSessionOpen)
            {
                ShowSaveError(TextKeys.Common.ServerErrorTitle);
            }
            else if (isUploading)
            {
                _isSaveWaitingForUpload = true;
            }
            else
            {
                StartSave();
            }
        }

        private void StartSave()
        {
            _isSaveWaitingForUpload = false;
            bool includesNewAvatar = _upload?.State == AvatarUploadState.Completed;
            _saveRequest = _gameSessionConnection.UpdateProfileAsync(_usernameField.Value.Trim(), includesNewAvatar);
        }

        private void FollowSaveRequest()
        {
            if ((_saveRequest is null) || !_saveRequest.IsCompleted)
            {
                return;
            }

            UpdateProfileResult saveResult = _saveRequest.IsCompletedSuccessfully ? _saveRequest.Result : new UpdateProfileResult { Status = UpdateProfileStatus.DatabaseUnavailable };
            _saveRequest = null;
            ShowSaveOutcome(saveResult);
        }

        private void ShowSaveOutcome(UpdateProfileResult saveResult)
        {
            switch (saveResult.Status)
            {
                case UpdateProfileStatus.Updated:
                    ApplySavedProfile(saveResult.AvatarReference);
                    break;

                case UpdateProfileStatus.UsernameTaken:
                    ShowUsernameError(TextKeys.Profile.UsernameUnavailable);
                    break;

                case UpdateProfileStatus.InvalidUsername:
                    ShowUsernameError(TextKeys.Register.UsernameInvalidFormat);
                    break;

                case UpdateProfileStatus.AvatarIncomplete:
                    ResendUpload();
                    break;

                case UpdateProfileStatus.RoomInMatch:
                    ShowSaveError(TextKeys.Profile.MatchInProgress);
                    break;

                case UpdateProfileStatus.DatabaseUnavailable:
                case UpdateProfileStatus.StorageUnavailable:
                    ShowSaveError(TextKeys.Common.ServerErrorTitle);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(saveResult));
            }
        }

        private void ApplySavedProfile(string? avatarReference)
        {
            string savedUsername = _usernameField.Value.Trim();
            if ((avatarReference is not null) && (_preparedAvatar is not null) && (_upload is not null))
            {
                _avatarCache.Put(avatarReference, _preparedAvatar.Thumbnail);
            }

            _upload = null;
            _preparedAvatar = null;
            _avatarView.PreviewImage = null;
            ShowProfile(savedUsername, _emailField.Value, avatarReference);
            _session.ApplyProfile(savedUsername, avatarReference);
            _savedLabel.Visible = true;
        }

        private void ResendUpload()
        {
            if (_wasUploadResent || (_preparedAvatar is null))
            {
                DiscardAvatarChange();
                ShowSaveError(TextKeys.Common.ServerErrorTitle);
                return;
            }

            _wasUploadResent = true;
            StartUpload();
            _isSaveWaitingForUpload = _upload is not null;
        }

        private void DiscardAvatarChange()
        {
            _upload?.Cancel();
            _upload = null;
            _preparedAvatar = null;
            _preparationRequest = null;
            _isSaveWaitingForUpload = false;
            _avatarView.PreviewImage = null;
        }

        private void RefreshControls()
        {
            _avatarView.Refresh();
            bool isUploading = (_upload is not null) && (_upload.State != AvatarUploadState.Completed);
            _uploadProgressBar.Visible = isUploading;
            _uploadProgressBar.Value = _upload?.Progress ?? UploadProgressStart;
            _avatarMissingLabel.Visible = (_avatarView.PreviewImage is null) && _avatarCache.IsMissing(_avatarView.AvatarReference);
            bool isSaving = (_saveRequest is not null) || _isSaveWaitingForUpload;
            bool hasChanges = (_upload is not null) || !string.Equals(_usernameField.Value.Trim(), _savedUsername, StringComparison.Ordinal);
            _saveButton.Enabled = hasChanges && !isSaving;
            _backToMenuButton.Enabled = _saveRequest is null;
            _backToRoomButton.Enabled = _saveRequest is null;
        }

        private void HideMessages()
        {
            _avatarErrorLabel.Visible = false;
            _usernameErrorLabel.Visible = false;
            _saveErrorLabel.Visible = false;
            _savedLabel.Visible = false;
        }

        private void ShowAvatarError(string textKey, string? sizeText)
        {
            _avatarErrorLabel.TextKey = textKey;
            _avatarErrorLabel.TextArguments = sizeText is null ? null : new string[] { sizeText };
            _avatarErrorLabel.Visible = true;
        }

        private void ShowUsernameError(string textKey)
        {
            _usernameErrorLabel.TextKey = textKey;
            _usernameErrorLabel.Visible = true;
        }

        private void ShowSaveError(string textKey)
        {
            _saveErrorLabel.TextKey = textKey;
            _saveErrorLabel.Visible = true;
        }

        private void AskLogOutConfirmation()
        {
            ShowConfirmation(TextKeys.Alerts.LogOutTitle, TextKeys.Alerts.LogOutBody);

            LocalizedButton logOutConfirmButton = CreateDestructiveButton(TextKeys.Alerts.LogOutConfirmButton);
            logOutConfirmButton.Click += LogOutConfirmButtonOnClick;
            LocalizedButton stayButton = CreateSecondaryButton(TextKeys.Alerts.StayButton);
            stayButton.Click += StayButtonOnClick;
            AddConfirmationButtons(logOutConfirmButton, stayButton);
        }

        private void ChangeAvatarButtonOnClick(object sender, MyraEventArgs e)
        {
            var fileDialog = new FileDialog(FileDialogMode.OpenFile) { Filter = AvatarFileFilter };
            fileDialog.Closed += FileDialogOnClosed;
            fileDialog.ShowModal(Root.Desktop);
        }

        private void FileDialogOnClosed(object sender, MyraEventArgs e)
        {
            if ((sender is FileDialog fileDialog) && fileDialog.Result && !string.IsNullOrEmpty(fileDialog.FilePath))
            {
                StartPreparation(fileDialog.FilePath);
            }
        }

        private void SaveButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestSave();
        }

        private void UsernameBoxOnValueChanging(object sender, ValueChangingEventArgs<string> e)
        {
            if ((e.NewValue is not null) && (e.NewValue.Length > AccountLimits.LongestUsername))
            {
                e.Cancel = true;
            }
        }

        private void LogOutButtonOnClick(object sender, MyraEventArgs e)
        {
            if (_session.CurrentRoom is null)
            {
                WasLogOutRequested = true;
            }
            else
            {
                AskLogOutConfirmation();
            }
        }

        private void LogOutConfirmButtonOnClick(object sender, MyraEventArgs e)
        {
            HideConfirmation();
            WasLogOutRequested = true;
        }

        private void StayButtonOnClick(object sender, MyraEventArgs e)
        {
            HideConfirmation();
        }

        private void GuestLogInButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Login;
        }

        private void GuestCreateAccountButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Register;
        }

        private void SettingsButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.AccountSettings;
        }

        private void BackToMenuButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MainMenu;
        }

        private void BackToRoomButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Room;
        }
    }
}
