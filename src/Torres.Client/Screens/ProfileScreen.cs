using System;

using Game.Contracts;

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

        internal ProfileScreen(PlayerSession session)
            : base(TextKeys.Profile.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(session);

            _session = session;
        }

        internal override void Open()
        {
            if (_session.Player is not PlayerIdentity player)
            {
                return;
            }

            _usernameField.Box.Text = player.Username;
            _emailField.Box.Text = player.Email;
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = CreateColumns();
            columns.Widgets.Add(BuildAvatarCard());
            columns.Widgets.Add(BuildAccountCard());

            VerticalStackPanel page = CreatePage();
            page.Widgets.Add(columns);
            StackPanel.SetProportionType(columns, ProportionType.Fill);

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(CreateBackBar(backButton));

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

        private void SettingsButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.AccountSettings;
        }

        private void BackButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
