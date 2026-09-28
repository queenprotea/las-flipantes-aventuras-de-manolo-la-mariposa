using System;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class TopBarView
    {
        private const string ConnectionDot = "●";

        private readonly LocalizedLabel _headerLabel = new LocalizedLabel(TextKeys.MainMenu.HeaderLabel);
        private readonly Label _usernameLabel = new Label();
        private readonly PlayerSession _session;
        private readonly LocalizedButton _logInButton;
        private readonly HorizontalStackPanel _account;

        internal TopBarView(PlayerSession session)
        {
            ArgumentNullException.ThrowIfNull(session);

            _session = session;
            _logInButton = BuildLogInButton();
            _account = BuildAccount();

            _headerLabel.Font = Fonts.Label;
            _headerLabel.TextColor = Theme.MutedInk;

            var content = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(0, 0, 0, Metrics.AppBarSpacing),
            };
            content.Widgets.Add(BuildLogo());
            content.Widgets.Add(BuildIdentity());

            var panel = new VerticalStackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, Metrics.AppBarBottomSpacing),
            };
            panel.Widgets.Add(content);
            panel.Widgets.Add(Screen.Divider());
            Panel = panel;
        }

        internal Widget Panel { get; }

        internal bool WasLogInRequested { get; private set; }

        internal bool WasProfileRequested { get; private set; }

        internal void Follow(Screen screen)
        {
            Panel.Visible = screen.ShowsTopBar;
            if (screen.ShowsTopBar)
            {
                _headerLabel.TextKey = screen.HeaderKey;
                _headerLabel.TextArguments = screen.HeaderArguments;
            }

            FollowSession();
        }

        internal void ClearRequest()
        {
            WasLogInRequested = false;
            WasProfileRequested = false;
        }

        private void FollowSession()
        {
            _logInButton.Visible = !_session.IsLoggedIn;
            _account.Visible = _session.IsLoggedIn;
            _usernameLabel.Text = _session.Player?.Username ?? string.Empty;
        }

        private VerticalStackPanel BuildLogo()
        {
            var logo = new VerticalStackPanel
            {
                Spacing = 2,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            logo.Widgets.Add(new LocalizedLabel(TextKeys.Common.GameName)
            {
                Font = Fonts.Brand,
                TextColor = Theme.Ink,
            });
            logo.Widgets.Add(_headerLabel);

            return logo;
        }

        private HorizontalStackPanel BuildIdentity()
        {
            var identity = new HorizontalStackPanel
            {
                Spacing = Metrics.ColumnSpacing,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
            };
            identity.Widgets.Add(BuildConnectionStatus());
            identity.Widgets.Add(_logInButton);
            identity.Widgets.Add(_account);
            FollowSession();

            return identity;
        }

        private LocalizedButton BuildLogInButton()
        {
            var logInButton = new LocalizedButton(TextKeys.Common.LogInButton)
            {
                LabelColor = Theme.Ink,
                LabelFont = Fonts.Small,
                Padding = Metrics.PillButtonPadding,
                Background = Theme.SurfaceBrush,
                OverBackground = Theme.SurfaceSunkenBrush,
                PressedBackground = Theme.SurfaceSunkenBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };
            logInButton.Click += LogInButtonOnClick;

            return logInButton;
        }

        private HorizontalStackPanel BuildAccount()
        {
            _usernameLabel.Font = Fonts.Control;
            _usernameLabel.TextColor = Theme.Ink;
            _usernameLabel.HorizontalAlignment = HorizontalAlignment.Right;

            var names = new VerticalStackPanel { VerticalAlignment = VerticalAlignment.Center };
            names.Widgets.Add(_usernameLabel);
            names.Widgets.Add(new LocalizedLabel(TextKeys.Common.RegisteredRole)
            {
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
                HorizontalAlignment = HorizontalAlignment.Right,
            });

            var account = new HorizontalStackPanel
            {
                Spacing = Metrics.ButtonSpacing,
                VerticalAlignment = VerticalAlignment.Center,
            };
            account.Widgets.Add(names);
            account.Widgets.Add(BuildAvatarButton());

            return account;
        }

        private Button BuildAvatarButton()
        {
            var avatar = new Panel
            {
                Width = Sizes.AppBarAvatarSize,
                Height = Sizes.AppBarAvatarSize,
                Background = Theme.MintAltBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
            };

            var avatarButton = new Button
            {
                Content = avatar,
                Padding = new Thickness(0),
                Background = null,
                OverBackground = null,
                PressedBackground = null,
                Border = null,
                BorderThickness = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
            };
            avatarButton.Click += AvatarButtonOnClick;

            return avatarButton;
        }

        private HorizontalStackPanel BuildConnectionStatus()
        {
            var status = new HorizontalStackPanel
            {
                Spacing = Metrics.FieldLabelSpacing,
                VerticalAlignment = VerticalAlignment.Center,
            };
            status.Widgets.Add(new Label
            {
                Text = ConnectionDot,
                Font = Fonts.Label,
                TextColor = Theme.Blush,
                VerticalAlignment = VerticalAlignment.Center,
            });
            status.Widgets.Add(new LocalizedLabel(TextKeys.Common.OfflineStatus)
            {
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            });

            return status;
        }

        private void LogInButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            WasLogInRequested = true;
        }

        private void AvatarButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            WasProfileRequested = true;
        }
    }
}
