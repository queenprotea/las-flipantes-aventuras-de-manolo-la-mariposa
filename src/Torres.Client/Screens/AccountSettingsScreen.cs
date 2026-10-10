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
            VerticalStackPanel card = CreateCard(Sizes.AccountSettingsCardWidth);
            card.Widgets.Add(BuildDeleteBlock());

            VerticalStackPanel page = CreatePage();
            page.Widgets.Add(card);
            StackPanel.SetProportionType(card, ProportionType.Fill);

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.AccountSettings.BackToProfileButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(CreateBackBar(backButton));

            return page;
        }

        private static VerticalStackPanel BuildDeleteBlock()
        {
            var deleteAccountBlock = new VerticalStackPanel { Spacing = BlockSpacing };
            var deleteAccountHeader = new VerticalStackPanel { Spacing = HeaderSpacing };
            deleteAccountHeader.Widgets.Add(new LocalizedLabel(TextKeys.AccountSettings.DeleteAccountTitle)
            {
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                Wrap = true,
            });
            deleteAccountHeader.Widgets.Add(CreateHint(TextKeys.AccountSettings.DeleteAccountHint));
            deleteAccountBlock.Widgets.Add(deleteAccountHeader);

            LocalizedButton deleteButton = CreateDestructiveButton(TextKeys.AccountSettings.DeleteAccountButton);
            deleteButton.HorizontalAlignment = HorizontalAlignment.Left;
            deleteAccountBlock.Widgets.Add(deleteButton);

            return deleteAccountBlock;
        }

        private void BackButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Profile;
        }
    }
}
