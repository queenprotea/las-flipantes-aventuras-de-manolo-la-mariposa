using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class StartupScreen : Screen
    {
        private const double ConnectingSeconds = 1.6;
        private const double ReadySeconds = 2.4;

        private LocalizedLabel? _statusLabel;
        private double _elapsedSeconds;

        internal StartupScreen()
            : base(TextKeys.Startup.ConnectingStatus, false)
        {
        }

        internal override void Update(GameTime gameTime)
        {
            _elapsedSeconds += gameTime.ElapsedGameTime.TotalSeconds;
            if (_statusLabel is not null)
            {
                _statusLabel.TextKey = _elapsedSeconds < ConnectingSeconds
                    ? TextKeys.Startup.ConnectingStatus
                    : TextKeys.Startup.ReadyStatus;
            }

            if (_elapsedSeconds >= ReadySeconds)
            {
                RequestedScreen = ScreenId.MainMenu;
            }
        }

        protected override Widget Build()
        {
            _statusLabel = new LocalizedLabel(TextKeys.Startup.ConnectingStatus)
            {
                TextColor = Theme.MutedText,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            return new VerticalStackPanel
            {
                Spacing = 12,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Widgets =
                {
                    new LocalizedLabel(TextKeys.Common.GameName)
                    {
                        TextColor = Theme.Accent,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    },
                    _statusLabel,
                },
            };
        }
    }
}
