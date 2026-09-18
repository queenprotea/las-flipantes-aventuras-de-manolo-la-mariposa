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
            var panel = Card();
            panel.Widgets.Add(Title(TextKeys.GuestAccess.Title));
            panel.Widgets.Add(Hint(TextKeys.GuestAccess.Hint));
            panel.Widgets.Add(new LabeledTextBox(TextKeys.GuestAccess.MatchNameLabel, false));
            panel.Widgets.Add(new LocalizedLabel(TextKeys.GuestAccess.RepeatableName)
            {
                TextColor = Theme.Accent,
                Wrap = true,
            });
            panel.Widgets.Add(Hint(TextKeys.GuestAccess.Limitations));

            LocalizedButton enterButton = PrimaryButton(TextKeys.GuestAccess.EnterButton);
            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackButton);
            backButton.Click += OnBackClick;
            HorizontalStackPanel actions = Row();
            actions.Widgets.Add(enterButton);
            actions.Widgets.Add(backButton);
            panel.Widgets.Add(actions);
            return panel;
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Login;
        }
    }
}
