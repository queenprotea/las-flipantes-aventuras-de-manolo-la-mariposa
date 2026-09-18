using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class TopBarView
    {
        private readonly LocalizedLabel _headerLabel = new LocalizedLabel(TextKeys.MainMenu.HeaderLabel);
        private readonly HorizontalStackPanel _panel;

        internal TopBarView()
        {
            _headerLabel.TextColor = Theme.MutedText;

            LocalizedButton logInButton = new LocalizedButton(TextKeys.Common.LogInButton) { LabelColor = Theme.Text };
            logInButton.Click += OnLogInClick;

            _panel = new HorizontalStackPanel
            {
                Spacing = 16,
                Padding = new Thickness(Theme.Margin, 10),
                Background = Theme.TopBarBrush,
            };
            _panel.Widgets.Add(new LocalizedLabel(TextKeys.Common.GameName) { TextColor = Theme.Accent });
            _panel.Widgets.Add(_headerLabel);
            _panel.Widgets.Add(new LocalizedLabel(TextKeys.Common.OfflineStatus) { TextColor = Theme.Danger });
            _panel.Widgets.Add(logInButton);
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

        private void OnLogInClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            WasLogInRequested = true;
        }
    }
}
