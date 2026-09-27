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
            HorizontalStackPanel columns = Columns();
            columns.Widgets.Add(BuildAvatarCard());
            columns.Widgets.Add(BuildAccountCard());

            VerticalStackPanel page = Page();
            page.Widgets.Add(columns);
            StackPanel.SetProportionType(columns, ProportionType.Fill);

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += OnBackClick;
            page.Widgets.Add(BackBar(backButton));
            return page;
        }

        private static VerticalStackPanel BuildAvatarCard()
        {
            VerticalStackPanel card = Card(Sizes.ProfileAvatarCardWidth);
            card.Spacing = 0;

            Panel avatar = Avatar();
            avatar.Margin = new Thickness(0, 0, 0, AvatarSpacing);
            card.Widgets.Add(avatar);

            LocalizedButton changeAvatarButton = SmallSecondaryButton(TextKeys.Profile.ChangeAvatarButton);
            changeAvatarButton.HorizontalAlignment = HorizontalAlignment.Center;
            card.Widgets.Add(changeAvatarButton);

            LocalizedLabel formatHint = Hint(TextKeys.Profile.AvatarFormatHint);
            formatHint.HorizontalAlignment = HorizontalAlignment.Center;
            formatHint.Margin = new Thickness(0, AvatarHintSpacing, 0, 0);
            card.Widgets.Add(formatHint);
            return card;
        }

        private VerticalStackPanel BuildAccountCard()
        {
            VerticalStackPanel card = Card(Sizes.ProfileAccountCardWidth);
            card.Widgets.Add(CardHeader(TextKeys.Profile.AccountTitle, TextKeys.Profile.AccountHint));
            card.Widgets.Add(_usernameField);
            card.Widgets.Add(_emailField);

            LocalizedButton saveButton = PrimaryButton(TextKeys.Profile.SaveButton);
            LocalizedButton settingsButton = SecondaryButton(TextKeys.Profile.AccountSettingsButton);
            settingsButton.Click += OnSettingsClick;
            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(saveButton);
            actions.Widgets.Add(settingsButton);
            card.Widgets.Add(actions);
            return card;
        }

        private void OnSettingsClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.AccountSettings;
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
