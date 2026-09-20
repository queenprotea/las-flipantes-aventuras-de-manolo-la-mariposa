using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    /// <summary>
    /// PT-06 del prototipo: la tarjeta del alias de invitado y la ambientación a la derecha.
    /// </summary>
    internal sealed class GuestAccessScreen : Screen
    {
        internal GuestAccessScreen()
            : base(TextKeys.GuestAccess.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = Columns();
            columns.Widgets.Add(BuildCard());
            columns.Widgets.Add(Ambience());
            StackPanel.SetProportionType(columns.Widgets[1], ProportionType.Fill);
            return columns;
        }

        private VerticalStackPanel BuildCard()
        {
            VerticalStackPanel card = Card(Theme.GuestCardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(CardHeader(TextKeys.GuestAccess.Title, TextKeys.GuestAccess.Hint));

            var nameField = new VerticalStackPanel { Spacing = Theme.FieldLabelSpacing };
            nameField.Widgets.Add(new LabeledTextBox(TextKeys.GuestAccess.MatchNameLabel, false));
            nameField.Widgets.Add(Success(TextKeys.GuestAccess.RepeatableName));
            card.Widgets.Add(nameField);

            card.Widgets.Add(Paragraph(TextKeys.GuestAccess.Limitations));

            LocalizedButton enterButton = PrimaryButton(TextKeys.GuestAccess.EnterButton);
            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackButton);
            backButton.Click += OnBackClick;
            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(enterButton);
            actions.Widgets.Add(backButton);
            card.Widgets.Add(actions);
            return card;
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Login;
        }
    }
}
