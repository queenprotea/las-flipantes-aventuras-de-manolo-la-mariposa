using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class SettingsFailureScreen : Screen
    {
        private const int NoticeSpacing = 14;

        internal SettingsFailureScreen()
            : base(TextKeys.Common.GameName, false)
        {
        }

        internal bool WasExitRequested { get; private set; }

        protected override Widget Build()
        {
            VerticalStackPanel failureMessage = PanelFactory.CreateFailureBox(TextKeys.Startup.SettingsError);

            LocalizedButton exitButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.ExitButton);
            exitButton.Click += ExitButtonOnClick;
            HorizontalStackPanel failureActions = PanelFactory.CreateButtonRow();
            failureActions.HorizontalAlignment = HorizontalAlignment.Center;
            failureActions.Widgets.Add(exitButton);

            var failureNotice = new VerticalStackPanel
            {
                Width = Metrics.NoticeWidth,
                Spacing = Metrics.FieldSpacing,
                Margin = new Thickness(0, NoticeSpacing, 0, 0),
            };
            failureNotice.Widgets.Add(failureMessage);
            failureNotice.Widgets.Add(failureActions);

            var failureLayout = new VerticalStackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            failureLayout.Widgets.Add(LabelFactory.CreateGameName());
            failureLayout.Widgets.Add(failureNotice);

            return failureLayout;
        }

        private void ExitButtonOnClick(object sender, MyraEventArgs e)
        {
            WasExitRequested = true;
        }
    }
}
