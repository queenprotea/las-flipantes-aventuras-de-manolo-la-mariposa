using System;

using Game.Contracts;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class ProfileScreen : Screen
    {
        private const int AvatarSpacing = 14;
        private const int AvatarHintSpacing = 10;

        private readonly PlayerSession _session;
        private readonly LabeledTextBox _usernameField = new LabeledTextBox(TextKeys.Profile.UsernameLabel, false);
        private readonly LabeledTextBox _emailField = new LabeledTextBox(TextKeys.Profile.EmailLabel, false);
        private readonly HorizontalStackPanel _accountColumns;
        private readonly HorizontalStackPanel _guestColumns;
        private readonly Panel _contentPlaceholder = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        private readonly Label _guestAliasLabel;
        private readonly LocalizedButton _logOutButton;
        private readonly LocalizedButton _backToMenuButton;
        private readonly LocalizedButton _backToRoomButton;

        internal ProfileScreen(PlayerSession session)
            : base(TextKeys.Profile.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(session);

            _session = session;
            _accountColumns = CreateColumns();
            _guestColumns = CreateColumns();
            _guestAliasLabel = CreatePlayerNameLabel(string.Empty, Theme.Ink);
            _logOutButton = CreateSecondaryButton(TextKeys.Profile.LogOutButton);
            _backToMenuButton = CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            _backToRoomButton = CreateSecondaryButton(TextKeys.Common.BackToRoomButton);
        }

        internal bool WasLogOutRequested { get; private set; }

        internal override void Open()
        {
            HideConfirmation();
            bool isGuest = _session.IsGuest;
            HorizontalStackPanel shownColumns = isGuest ? _guestColumns : _accountColumns;
            _contentPlaceholder.Widgets.Clear();
            _contentPlaceholder.Widgets.Add(shownColumns);
            _logOutButton.Visible = !isGuest;
            bool isInRoom = _session.CurrentRoom is not null;
            _backToMenuButton.Visible = !isInRoom;
            _backToRoomButton.Visible = isInRoom;
            _guestAliasLabel.Text = _session.DisplayName;
            if (_session.Player is PlayerIdentity player)
            {
                _usernameField.Box.Text = player.Username;
                _emailField.Box.Text = player.Email;
            }
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

        private static VerticalStackPanel BuildAvatarCard()
        {
            VerticalStackPanel card = CreateCard(Sizes.ProfileAvatarCardWidth);
            card.Spacing = 0;

            Panel avatar = CreateAvatar();
            avatar.Margin = new Thickness(0, 0, 0, AvatarSpacing);
            card.Widgets.Add(avatar);

            LocalizedButton changeAvatarButton = CreateSmallSecondaryButton(TextKeys.Profile.ChangeAvatarButton);
            changeAvatarButton.HorizontalAlignment = HorizontalAlignment.Center;
            card.Widgets.Add(changeAvatarButton);

            LocalizedLabel formatHint = CreateHint(TextKeys.Profile.AvatarFormatHint);
            formatHint.HorizontalAlignment = HorizontalAlignment.Center;
            formatHint.Margin = new Thickness(0, AvatarHintSpacing, 0, 0);
            card.Widgets.Add(formatHint);

            return card;
        }

        private VerticalStackPanel BuildAccountCard()
        {
            VerticalStackPanel card = CreateCard(Sizes.ProfileAccountCardWidth);
            card.Widgets.Add(CreateCardHeader(TextKeys.Profile.AccountTitle, TextKeys.Profile.AccountHint));
            card.Widgets.Add(_usernameField);
            card.Widgets.Add(_emailField);

            LocalizedButton saveButton = CreatePrimaryButton(TextKeys.Profile.SaveButton);
            LocalizedButton settingsButton = CreateSecondaryButton(TextKeys.Profile.AccountSettingsButton);
            settingsButton.Click += SettingsButtonOnClick;
            HorizontalStackPanel accountActions = CreateButtonRow();
            accountActions.Widgets.Add(saveButton);
            accountActions.Widgets.Add(settingsButton);
            card.Widgets.Add(accountActions);

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

        private void AskLogOutConfirmation()
        {
            ShowConfirmation(TextKeys.Alerts.LogOutTitle, TextKeys.Alerts.LogOutBody);

            LocalizedButton logOutConfirmButton = CreateDestructiveButton(TextKeys.Alerts.LogOutConfirmButton);
            logOutConfirmButton.Click += LogOutConfirmButtonOnClick;
            LocalizedButton stayButton = CreateSecondaryButton(TextKeys.Alerts.StayButton);
            stayButton.Click += StayButtonOnClick;
            AddConfirmationButtons(logOutConfirmButton, stayButton);
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
