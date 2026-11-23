using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class SettingsFailureScreen : Screen
    {
        private const int NoticeSpacing = 14;
        private const int NoticeWidth = 420;

        internal SettingsFailureScreen()
            : base(TextKeys.Common.GameName, false)
        {
        }

        internal bool WasExitRequested { get; private set; }

        protected override Widget Build()
        {
            var failureMessage = new VerticalStackPanel
            {
                Padding = Metrics.CardPadding,
                Background = Theme.BlushTintBrush,
                Border = Theme.BlushBrush,
                BorderThickness = Sizes.Border,
            };
            failureMessage.Widgets.Add(new LocalizedLabel(TextKeys.Startup.SettingsError)
            {
                Font = Fonts.Body,
                TextColor = Theme.BlushInk,
                Wrap = true,
            });

            LocalizedButton exitButton = CreateSecondaryButton(TextKeys.Common.ExitButton);
            exitButton.Click += ExitButtonOnClick;
            HorizontalStackPanel failureActions = CreateButtonRow();
            failureActions.HorizontalAlignment = HorizontalAlignment.Center;
            failureActions.Widgets.Add(exitButton);

            var failureNotice = new VerticalStackPanel
            {
                Width = NoticeWidth,
                Spacing = Metrics.FieldSpacing,
                Margin = new Thickness(0, NoticeSpacing, 0, 0),
            };
            failureNotice.Widgets.Add(failureMessage);
            failureNotice.Widgets.Add(failureActions);

            var failureLayout = new VerticalStackPanel
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
                    failureNotice,
                },
            };

            return failureLayout;
        }

        private void ExitButtonOnClick(object sender, MyraEventArgs e)
        {
            WasExitRequested = true;
        }
    }
}
