using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

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
            VerticalStackPanel card = PanelFactory.CreateCard(Sizes.AccountSettingsCardWidth);
            card.Widgets.Add(BuildDeleteBlock());

            VerticalStackPanel page = PanelFactory.CreatePage();
            page.Widgets.Add(card);
            StackPanel.SetProportionType(card, ProportionType.Fill);

            LocalizedButton backButton = ButtonFactory.CreateSecondaryButton(TextKeys.AccountSettings.BackToProfileButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(PanelFactory.CreateBackBar(backButton));

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
            deleteAccountHeader.Widgets.Add(LabelFactory.CreateHint(TextKeys.AccountSettings.DeleteAccountHint));
            deleteAccountBlock.Widgets.Add(deleteAccountHeader);

            LocalizedButton deleteButton = ButtonFactory.CreateDestructiveButton(TextKeys.AccountSettings.DeleteAccountButton);
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
