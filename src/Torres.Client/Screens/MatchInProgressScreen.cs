using FontStashSharp.RichText;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MatchInProgressScreen : Screen
    {
        private const int TitleBottomSpacing = 6;
        private const int MessageBottomSpacing = 14;
        private const int HintTopSpacing = 12;
        internal MatchInProgressScreen()
            : base(TextKeys.MatchInProgress.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            LocalizedLabel abandonHint = CreateHint(TextKeys.MatchInProgress.AbandonHint);
            abandonHint.TextAlign = TextHorizontalAlignment.Center;
            abandonHint.HorizontalAlignment = HorizontalAlignment.Center;
            abandonHint.Margin = new Thickness(0, HintTopSpacing, 0, 0);

            var screenLayout = new VerticalStackPanel
            {
                Width = Sizes.MatchInProgressWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            screenLayout.Widgets.Add(BuildDialog());
            screenLayout.Widgets.Add(abandonHint);

            return screenLayout;
        }

        private VerticalStackPanel BuildDialog()
        {
            LocalizedButton returnButton = CreatePrimaryButton(TextKeys.MatchInProgress.ReturnToMatchButton);
            LocalizedButton abandonButton = CreateDestructiveButton(TextKeys.MatchInProgress.AbandonButton);
            returnButton.Click += ReturnButtonOnClick;
            abandonButton.Click += AbandonButtonOnClick;

            HorizontalStackPanel dialogActions = CreateButtonRow();
            dialogActions.Widgets.Add(returnButton);
            dialogActions.Widgets.Add(abandonButton);

            LocalizedLabel offlineMessage = CreateParagraph(TextKeys.MatchInProgress.LeftWhileOfflineMessage);
            offlineMessage.Margin = new Thickness(0, 0, 0, MessageBottomSpacing);
            offlineMessage.TextArguments = new object[] { PreviewSamples.MatchRoomCode };

            VerticalStackPanel dialog = CreateCard(Sizes.DialogWidth);
            dialog.Spacing = 0;
            dialog.HorizontalAlignment = HorizontalAlignment.Center;
            dialog.Widgets.Add(new LocalizedLabel(TextKeys.MatchInProgress.Title)
            {
                Font = Fonts.Title,
                TextColor = Theme.Ink,
                Margin = new Thickness(0, 0, 0, TitleBottomSpacing),
            });
            dialog.Widgets.Add(offlineMessage);
            dialog.Widgets.Add(dialogActions);

            return dialog;
        }

        private void ReturnButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Resume;
        }

        private void AbandonButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
