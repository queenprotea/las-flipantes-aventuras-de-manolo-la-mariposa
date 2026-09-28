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
        private const string PreviewRoomCode = "K7QM";

        internal MatchInProgressScreen()
            : base(TextKeys.MatchInProgress.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            LocalizedLabel abandonHint = Hint(TextKeys.MatchInProgress.AbandonHint);
            abandonHint.TextAlign = TextHorizontalAlignment.Center;
            abandonHint.HorizontalAlignment = HorizontalAlignment.Center;
            abandonHint.Margin = new Thickness(0, HintTopSpacing, 0, 0);

            var content = new VerticalStackPanel
            {
                Width = Sizes.MatchInProgressWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            content.Widgets.Add(BuildDialog());
            content.Widgets.Add(abandonHint);

            return content;
        }

        private VerticalStackPanel BuildDialog()
        {
            LocalizedButton returnButton = PrimaryButton(TextKeys.MatchInProgress.ReturnToMatchButton);
            LocalizedButton abandonButton = DestructiveButton(TextKeys.MatchInProgress.AbandonButton);
            returnButton.Click += ReturnButtonOnClick;
            abandonButton.Click += AbandonButtonOnClick;

            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(returnButton);
            actions.Widgets.Add(abandonButton);

            LocalizedLabel message = Paragraph(TextKeys.MatchInProgress.LeftWhileOfflineMessage);
            message.Margin = new Thickness(0, 0, 0, MessageBottomSpacing);
            message.TextArguments = new object[] { PreviewRoomCode };

            VerticalStackPanel dialog = Card(Sizes.DialogWidth);
            dialog.Spacing = 0;
            dialog.HorizontalAlignment = HorizontalAlignment.Center;
            dialog.Widgets.Add(new LocalizedLabel(TextKeys.MatchInProgress.Title)
            {
                Font = Fonts.Title,
                TextColor = Theme.Ink,
                Margin = new Thickness(0, 0, 0, TitleBottomSpacing),
            });
            dialog.Widgets.Add(message);
            dialog.Widgets.Add(actions);

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
