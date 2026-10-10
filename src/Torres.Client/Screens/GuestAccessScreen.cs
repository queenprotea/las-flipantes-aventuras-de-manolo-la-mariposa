using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class GuestAccessScreen : Screen
    {
        internal GuestAccessScreen()
            : base(TextKeys.GuestAccess.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = CreateColumns();
            columns.Widgets.Add(BuildCard());
            columns.Widgets.Add(CreateAmbiencePanel());
            StackPanel.SetProportionType(columns.Widgets[1], ProportionType.Fill);

            return columns;
        }

        private VerticalStackPanel BuildCard()
        {
            VerticalStackPanel card = CreateCard(Sizes.GuestCardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(CreateCardHeader(TextKeys.GuestAccess.Title, TextKeys.GuestAccess.Hint));

            var nameField = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            nameField.Widgets.Add(new LabeledTextBox(TextKeys.GuestAccess.MatchNameLabel, false));
            nameField.Widgets.Add(CreateSuccessMessage(TextKeys.GuestAccess.RepeatableName));
            card.Widgets.Add(nameField);

            card.Widgets.Add(CreateParagraph(TextKeys.GuestAccess.Limitations));

            LocalizedButton enterButton = CreatePrimaryButton(TextKeys.GuestAccess.EnterButton);
            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackButton);
            backButton.Click += BackButtonOnClick;
            HorizontalStackPanel guestActions = CreateButtonRow();
            guestActions.Widgets.Add(enterButton);
            guestActions.Widgets.Add(backButton);
            card.Widgets.Add(guestActions);

            return card;
        }

        private void BackButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Login;
        }
    }
}
