using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    /// <summary>
    /// PT-01 del prototipo: el nombre del juego centrado y, debajo, el estado de la conexión.
    /// </summary>
    internal sealed class StartupScreen : Screen
    {
        private const double ConnectingSeconds = 1.6;
        private const double ReadySeconds = 2.4;
        private const int StatusSpacing = 14;

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
                Font = Fonts.Body,
                TextColor = Theme.MutedInk,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, StatusSpacing, 0, 0),
            };

            return new VerticalStackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Widgets =
                {
                    new LocalizedLabel(TextKeys.Common.GameName)
                    {
                        Font = Fonts.Display,
                        TextColor = Theme.Ink,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    },
                    _statusLabel,
                },
            };
        }
    }
}
