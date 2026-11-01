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
        private readonly LocalizedLabel _headerLabel = new LocalizedLabel(TextKeys.MainMenu.HeaderLabel);
        private readonly Label _usernameLabel = new Label();
        private readonly LocalizedLabel _roleLabel = new LocalizedLabel(TextKeys.Common.RegisteredRole);
        private readonly Label _connectionDot = new Label();
        private readonly LocalizedLabel _connectionLabel = new LocalizedLabel(TextKeys.Common.OfflineStatus);
        private readonly PlayerSession _session;
        private readonly LocalizedButton _logInButton;
        private readonly HorizontalStackPanel _accountSummary;

        internal TopBarView(PlayerSession session)
        {
            ArgumentNullException.ThrowIfNull(session);

            _session = session;
            _logInButton = BuildLogInButton();
            _accountSummary = BuildAccountSummary();

            _headerLabel.Font = Fonts.Label;
            _headerLabel.TextColor = Theme.MutedInk;

            var barContent = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(0, 0, 0, Metrics.AppBarSpacing),
            };
            barContent.Widgets.Add(BuildLogo());
            barContent.Widgets.Add(BuildIdentity());

            var barRoot = new VerticalStackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, Metrics.AppBarBottomSpacing),
            };
            barRoot.Widgets.Add(barContent);
            barRoot.Widgets.Add(Screen.CreateDivider());
            Root = barRoot;
        }

        internal Widget Root { get; }

        internal bool WasLogInRequested { get; private set; }

        internal bool WasProfileRequested { get; private set; }

        internal void FollowScreen(Screen shownScreen)
        {
            Root.Visible = shownScreen.ShowsTopBar;
            if (shownScreen.ShowsTopBar)
            {
                _headerLabel.TextKey = shownScreen.HeaderKey;
                _headerLabel.TextArguments = shownScreen.HeaderArguments;
            }

            FollowSession();
        }

        internal void ClearRequests()
        {
            WasLogInRequested = false;
            WasProfileRequested = false;
        }

        private void FollowSession()
        {
            _logInButton.Visible = !_session.IsLoggedIn;
            _accountSummary.Visible = _session.IsLoggedIn;
            _usernameLabel.Text = _session.DisplayName;
            _roleLabel.TextKey = _session.IsGuest ? TextKeys.Common.GuestRole : TextKeys.Common.RegisteredRole;
            _connectionDot.TextColor = _session.IsLoggedIn ? Theme.MintLine : Theme.Blush;
            _connectionLabel.TextKey = _session.IsLoggedIn ? TextKeys.Common.ConnectedStatus : TextKeys.Common.OfflineStatus;
        }

        private VerticalStackPanel BuildLogo()
        {
            var logoPanel = new VerticalStackPanel
            {
                Spacing = Metrics.LogoSpacing,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            logoPanel.Widgets.Add(new LocalizedLabel(TextKeys.Common.GameName)
            {
                Font = Fonts.Brand,
                TextColor = Theme.Ink,
            });
            logoPanel.Widgets.Add(_headerLabel);

            return logoPanel;
        }

        private HorizontalStackPanel BuildIdentity()
        {
            var identityPanel = new HorizontalStackPanel
            {
                Spacing = Metrics.ColumnSpacing,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
            };
            identityPanel.Widgets.Add(BuildConnectionStatus());
            identityPanel.Widgets.Add(_logInButton);
            identityPanel.Widgets.Add(_accountSummary);
            FollowSession();

            return identityPanel;
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

        private HorizontalStackPanel BuildAccountSummary()
        {
            _usernameLabel.Font = Fonts.Control;
            _usernameLabel.TextColor = Theme.Ink;
            _usernameLabel.HorizontalAlignment = HorizontalAlignment.Right;

            var accountNames = new VerticalStackPanel { VerticalAlignment = VerticalAlignment.Center };
            accountNames.Widgets.Add(_usernameLabel);
            _roleLabel.Font = Fonts.Label;
            _roleLabel.TextColor = Theme.MutedInk;
            _roleLabel.HorizontalAlignment = HorizontalAlignment.Right;
            accountNames.Widgets.Add(_roleLabel);

            var accountSummary = new HorizontalStackPanel
            {
                Spacing = Metrics.ButtonSpacing,
                VerticalAlignment = VerticalAlignment.Center,
            };
            accountSummary.Widgets.Add(accountNames);
            accountSummary.Widgets.Add(BuildAvatarButton());

            return accountSummary;
        }

        private Button BuildAvatarButton()
        {
            var avatarPanel = new Panel
            {
                Width = Sizes.AppBarAvatarSize,
                Height = Sizes.AppBarAvatarSize,
                Background = Theme.MintAltBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
            };

            var avatarButton = new Button
            {
                Content = avatarPanel,
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
            var connectionStatus = new HorizontalStackPanel
            {
                Spacing = Metrics.FieldLabelSpacing,
                VerticalAlignment = VerticalAlignment.Center,
            };
            _connectionDot.Text = DisplaySymbols.ConnectionDot;
            _connectionDot.Font = Fonts.Label;
            _connectionDot.VerticalAlignment = VerticalAlignment.Center;
            connectionStatus.Widgets.Add(_connectionDot);
            _connectionLabel.Font = Fonts.Label;
            _connectionLabel.TextColor = Theme.MutedInk;
            _connectionLabel.VerticalAlignment = VerticalAlignment.Center;
            connectionStatus.Widgets.Add(_connectionLabel);

            return connectionStatus;
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
