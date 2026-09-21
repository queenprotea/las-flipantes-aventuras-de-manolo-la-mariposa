using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class TopBarView
    {
        private const string ConnectionDot = "●";

        private readonly LocalizedLabel _headerLabel = new LocalizedLabel(TextKeys.MainMenu.HeaderLabel);
        private readonly VerticalStackPanel _panel;

        internal TopBarView()
        {
            _headerLabel.Font = Fonts.Label;
            _headerLabel.TextColor = Theme.MutedInk;

            var content = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(0, 0, 0, Theme.AppBarSpacing),
            };
            content.Widgets.Add(BuildLogo());
            content.Widgets.Add(BuildIdentity());

            _panel = new VerticalStackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, Theme.AppBarBottomSpacing),
            };
            _panel.Widgets.Add(content);
            _panel.Widgets.Add(Screen.Divider());
        }

        internal Widget Panel => _panel;

        internal bool WasLogInRequested { get; private set; }

        internal void Follow(Screen screen)
        {
            _panel.Visible = screen.ShowsTopBar;
            if (screen.ShowsTopBar)
            {
                _headerLabel.TextKey = screen.HeaderKey;
            }
        }

        internal void ClearRequest()
        {
            WasLogInRequested = false;
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
            var logInButton = new LocalizedButton(TextKeys.Common.LogInButton)
            {
                LabelColor = Theme.Ink,
                LabelFont = Fonts.Small,
                Padding = Theme.PillButtonPadding,
                Background = Theme.SurfaceBrush,
                OverBackground = Theme.SurfaceSunkenBrush,
                PressedBackground = Theme.SurfaceSunkenBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Theme.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };
            logInButton.Click += OnLogInClick;

            var identity = new HorizontalStackPanel
            {
                Spacing = Theme.ColumnSpacing,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
            };
            identity.Widgets.Add(BuildConnectionStatus());
            identity.Widgets.Add(logInButton);
            return identity;
        }

        private HorizontalStackPanel BuildConnectionStatus()
        {
            var status = new HorizontalStackPanel
            {
                Spacing = Theme.FieldLabelSpacing,
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

        private void OnLogInClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            WasLogInRequested = true;
        }
    }
}
