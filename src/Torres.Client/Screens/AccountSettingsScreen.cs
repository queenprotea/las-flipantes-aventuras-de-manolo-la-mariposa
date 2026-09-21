using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class AccountSettingsScreen : Screen
    {
        private const int BlockSpacing = 12;
        private const int HeaderSpacing = 4;

        internal AccountSettingsScreen()
            : base(TextKeys.AccountSettings.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel card = Card(Theme.AccountSettingsCardWidth);
            card.Widgets.Add(BuildDeleteBlock());

            VerticalStackPanel page = Page();
            page.Widgets.Add(card);
            StackPanel.SetProportionType(card, ProportionType.Fill);

            LocalizedButton backButton = SecondaryButton(TextKeys.AccountSettings.BackToProfileButton);
            backButton.Click += OnBackClick;
            page.Widgets.Add(BackBar(backButton));
            return page;
        }

        private static VerticalStackPanel BuildDeleteBlock()
        {
            var block = new VerticalStackPanel { Spacing = BlockSpacing };
            var header = new VerticalStackPanel { Spacing = HeaderSpacing };
            header.Widgets.Add(new LocalizedLabel(TextKeys.AccountSettings.DeleteAccountTitle)
            {
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                Wrap = true,
            });
            header.Widgets.Add(Hint(TextKeys.AccountSettings.DeleteAccountHint));
            block.Widgets.Add(header);

            LocalizedButton deleteButton = DestructiveButton(TextKeys.AccountSettings.DeleteAccountButton);
            deleteButton.HorizontalAlignment = HorizontalAlignment.Left;
            block.Widgets.Add(deleteButton);
            return block;
        }

        private void OnBackClick(object sender, Myra.Events.MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.Profile;
        }
    }
}
