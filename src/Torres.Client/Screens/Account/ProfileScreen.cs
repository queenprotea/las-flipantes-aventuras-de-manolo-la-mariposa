using System;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;

using Torres.Client.Avatars;
using Torres.Client.Contracts;
using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class ProfileScreen : Screen
    {
        private const int AvatarSpacing = 14;
        private const int AvatarHintSpacing = 10;
        private const string AvatarFileFilter = "*.png|*.jpg|*.jpeg";
        private const float UploadProgressStart = 0f;
        private const float UploadProgressEnd = 1f;

        private readonly ProfileFlow _profileFlow;
        private readonly AvatarCache _avatarCache;
        private readonly AvatarView _avatarView;
        private readonly LabeledTextBox _usernameField = new LabeledTextBox(TextKeys.Profile.UsernameLabel, false, AccountLimits.LongestUsername);
        private readonly LabeledText _emailText = new LabeledText(TextKeys.Profile.EmailLabel);
        private readonly LocalizedLabel _avatarErrorLabel = LabelFactory.CreateErrorMessage(TextKeys.Profile.AvatarInvalidFormat);
        private readonly LocalizedLabel _avatarMissingLabel = LabelFactory.CreateHint(TextKeys.Profile.AvatarMissing);
        private readonly LocalizedLabel _usernameErrorLabel = LabelFactory.CreateErrorMessage(TextKeys.Profile.UsernameUnavailable);
        private readonly LocalizedLabel _saveErrorLabel = LabelFactory.CreateErrorMessage(TextKeys.Common.ServerErrorTitle);
        private readonly LocalizedLabel _savedLabel = LabelFactory.CreateSuccessMessage(TextKeys.Profile.SavedMessage);
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

        internal ProfileScreen(ProfileFlow profileFlow, AvatarCache avatarCache)
            : base(TextKeys.Profile.HeaderLabel, true)
        {
            _profileFlow = profileFlow;
            _avatarCache = avatarCache;
            _avatarView = new AvatarView(avatarCache, Sizes.LargeAvatarSize);
            _accountColumns = PanelFactory.CreateColumns();
            _guestColumns = PanelFactory.CreateColumns();
            _guestAliasLabel = LabelFactory.CreatePlayerNameLabel(string.Empty, Theme.Ink);
            _saveButton = ButtonFactory.CreatePrimaryButton(TextKeys.Profile.SaveButton);
            _logOutButton = ButtonFactory.CreateSecondaryButton(TextKeys.Profile.LogOutButton);
            _backToMenuButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            _backToRoomButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.BackToRoomButton);
        }

        internal bool WasLogOutRequested { get; private set; }

        internal override void Open()
        {
            HideConfirmation();
            bool isGuest = _profileFlow.IsGuest;
            HorizontalStackPanel visibleColumns = isGuest ? _guestColumns : _accountColumns;
            _contentPlaceholder.Widgets.Clear();
            _contentPlaceholder.Widgets.Add(visibleColumns);
            _logOutButton.Visible = !isGuest;
            bool isInRoom = _profileFlow.IsInRoom;
            _backToMenuButton.Visible = !isInRoom;
            _backToRoomButton.Visible = isInRoom;
            _guestAliasLabel.Text = _profileFlow.DisplayName;
            _avatarView.PreviewImage = null;
            HideMessages();
            _profileFlow.Open();
        }

        internal override void Close()
        {
            _profileFlow.DiscardAvatarChange();
            _avatarView.PreviewImage = null;
        }

        internal override void Update(GameTime gameTime)
        {
            _profileFlow.Follow();
            ProfileOutcome? outcome = _profileFlow.TakeOutcome();
            while (outcome is not null)
            {
                Show(outcome);
                outcome = _profileFlow.TakeOutcome();
            }

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

            VerticalStackPanel page = PanelFactory.CreatePage();
            page.Widgets.Add(ConfirmationPlaceholder);
            page.Widgets.Add(_contentPlaceholder);
            StackPanel.SetProportionType(_contentPlaceholder, ProportionType.Fill);

            _backToMenuButton.Click += BackToMenuButtonOnClick;
            _backToRoomButton.Click += BackToRoomButtonOnClick;
            _logOutButton.Click += LogOutButtonOnClick;
            Panel backBar = PanelFactory.CreateBackBar(_backToMenuButton, _logOutButton);
            _backToRoomButton.HorizontalAlignment = HorizontalAlignment.Left;
            backBar.Widgets.Add(_backToRoomButton);
            page.Widgets.Add(backBar);

            return page;
        }

        private VerticalStackPanel BuildAvatarCard()
        {
            VerticalStackPanel card = PanelFactory.CreateCard(Sizes.ProfileAvatarCardWidth);
            card.Spacing = 0;

            _avatarView.Root.HorizontalAlignment = HorizontalAlignment.Center;
            _avatarView.Root.Margin = new Thickness(0, 0, 0, AvatarSpacing);
            card.Widgets.Add(_avatarView.Root);

            LocalizedButton changeAvatarButton = ButtonFactory.CreateSmallSecondaryButton(TextKeys.Profile.ChangeAvatarButton);
            changeAvatarButton.HorizontalAlignment = HorizontalAlignment.Center;
            changeAvatarButton.Click += ChangeAvatarButtonOnClick;
            card.Widgets.Add(changeAvatarButton);

            LocalizedLabel formatHint = LabelFactory.CreateHint(TextKeys.Profile.AvatarFormatHint);
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
            VerticalStackPanel card = PanelFactory.CreateCard(Sizes.ProfileAccountCardWidth);
            card.Widgets.Add(PanelFactory.CreateCardHeader(TextKeys.Profile.AccountTitle, TextKeys.Profile.AccountHint));
            card.Widgets.Add(_usernameField);
            card.Widgets.Add(_usernameErrorLabel);
            card.Widgets.Add(_emailText);

            _saveButton.Click += SaveButtonOnClick;
            LocalizedButton settingsButton = ButtonFactory.CreateSecondaryButton(TextKeys.Profile.AccountSettingsButton);
            settingsButton.Click += SettingsButtonOnClick;
            HorizontalStackPanel accountActions = PanelFactory.CreateButtonRow();
            accountActions.Widgets.Add(_saveButton);
            accountActions.Widgets.Add(settingsButton);
            card.Widgets.Add(accountActions);
            card.Widgets.Add(_savedLabel);
            card.Widgets.Add(_saveErrorLabel);

            return card;
        }

        private VerticalStackPanel BuildGuestCard()
        {
            VerticalStackPanel card = PanelFactory.CreateCard(Sizes.ProfileAccountCardWidth);
            card.Widgets.Add(PanelFactory.CreateCardHeader(TextKeys.Profile.GuestTitle, TextKeys.Profile.GuestHint));

            HorizontalStackPanel aliasRow = PanelFactory.CreateButtonRow();
            aliasRow.Widgets.Add(_guestAliasLabel);
            aliasRow.Widgets.Add(PanelFactory.CreateStatusBadge(TextKeys.Common.GuestRole));
            card.Widgets.Add(aliasRow);

            card.Widgets.Add(LabelFactory.CreateParagraph(TextKeys.GuestAccess.Limitations));

            LocalizedButton guestLogInButton = ButtonFactory.CreatePrimaryButton(TextKeys.Profile.GuestLogInButton);
            LocalizedButton guestCreateAccountButton = ButtonFactory.CreateSecondaryButton(TextKeys.Profile.GuestCreateAccountButton);
            guestLogInButton.Click += GuestLogInButtonOnClick;
            guestCreateAccountButton.Click += GuestCreateAccountButtonOnClick;
            HorizontalStackPanel guestActions = PanelFactory.CreateButtonRow();
            guestActions.Widgets.Add(guestLogInButton);
            guestActions.Widgets.Add(guestCreateAccountButton);
            card.Widgets.Add(guestActions);

            return card;
        }

        private void Show(ProfileOutcome outcome)
        {
            if (outcome.Details is ProfileDetails details)
            {
                ShowProfile(details);
            }

            if (outcome.Message is ProfileMessage message)
            {
                ShowMessage(message);
            }

            if (outcome.Preview is PreparedAvatar preview)
            {
                _avatarView.PreviewImage = _avatarCache.CreatePreview(preview.Thumbnail);
            }

            _savedLabel.Visible |= outcome.IsSaved;
        }

        private void ShowProfile(ProfileDetails details)
        {
            _usernameField.Box.Text = details.Username;
            _emailText.ValueLabel.Text = details.Email;
            _avatarView.AvatarReference = details.AvatarReference;
        }

        private void ShowMessage(ProfileMessage message)
        {
            LocalizedLabel messageLabel = message.Target switch
            {
                ProfileMessageTarget.Avatar => _avatarErrorLabel,
                ProfileMessageTarget.Username => _usernameErrorLabel,
                ProfileMessageTarget.Save => _saveErrorLabel,
                _ => throw new ArgumentOutOfRangeException(nameof(message), message.Target, null),
            };
            messageLabel.TextKey = message.TextKey;
            messageLabel.TextArguments = message.FileBytes is long fileBytes ? new string[] { DisplayFormats.FormatMegabytes(fileBytes) } : null;
            messageLabel.Visible = true;
        }

        private void RefreshControls()
        {
            if (!_profileFlow.HasAvatarChange && !_profileFlow.IsUploading)
            {
                _avatarView.PreviewImage = null;
            }

            _avatarView.Refresh();
            _uploadProgressBar.Visible = _profileFlow.IsUploading;
            _uploadProgressBar.Value = _profileFlow.UploadProgress ?? UploadProgressStart;
            _avatarMissingLabel.Visible = (_avatarView.PreviewImage is null) && _avatarCache.IsMissing(_avatarView.AvatarReference);
            bool hasChanges = _profileFlow.HasAvatarChange || !string.Equals(_usernameField.Value.Trim(), _profileFlow.SavedUsername, StringComparison.Ordinal);
            _saveButton.Enabled = hasChanges && !_profileFlow.IsSaving;
            _backToMenuButton.Enabled = !_profileFlow.IsSaveRequestWaiting;
            _backToRoomButton.Enabled = !_profileFlow.IsSaveRequestWaiting;
        }

        private void HideMessages()
        {
            _avatarErrorLabel.Visible = false;
            _usernameErrorLabel.Visible = false;
            _saveErrorLabel.Visible = false;
            _savedLabel.Visible = false;
        }

        private void AskLogOutConfirmation()
        {
            ShowConfirmation(TextKeys.Alerts.LogOutTitle, TextKeys.Alerts.LogOutBody);

            LocalizedButton logOutConfirmButton = ButtonFactory.CreateDestructiveButton(TextKeys.Alerts.LogOutConfirmButton);
            logOutConfirmButton.Click += LogOutConfirmButtonOnClick;
            LocalizedButton stayButton = ButtonFactory.CreateSecondaryButton(TextKeys.Alerts.StayButton);
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
                HideMessages();
                _avatarView.PreviewImage = null;
                _profileFlow.StartAvatarPreparation(fileDialog.FilePath);
            }
        }

        private void SaveButtonOnClick(object sender, MyraEventArgs e)
        {
            HideMessages();
            _profileFlow.RequestSave(_usernameField.Value);
        }

        private void LogOutButtonOnClick(object sender, MyraEventArgs e)
        {
            if (!_profileFlow.IsInRoom)
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
            RequestedScreen = ProfileFlow.FindScreenToOpenSettings();
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
