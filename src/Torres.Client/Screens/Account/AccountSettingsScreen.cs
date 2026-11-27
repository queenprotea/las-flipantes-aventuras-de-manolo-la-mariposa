using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class AccountSettingsScreen : Screen
    {
        private const int BlockSpacing = 12;
        private const int HeaderSpacing = 4;

        private readonly AccountDeletionFlow _accountDeletionFlow;
        private readonly LocalizedLabel _errorLabel = LabelFactory.CreateErrorMessage(TextKeys.Common.ServerErrorTitle);
        private readonly LocalizedButton _deleteButton = ButtonFactory.CreateDestructiveButton(TextKeys.AccountSettings.DeleteAccountButton);
        private readonly LocalizedButton _backButton = ButtonFactory.CreateSecondaryButton(TextKeys.AccountSettings.BackToProfileButton);

        internal AccountSettingsScreen(AccountDeletionFlow accountDeletionFlow)
            : base(TextKeys.AccountSettings.HeaderLabel, true)
        {
            _accountDeletionFlow = accountDeletionFlow;
        }

        internal bool WasAccountDeleted { get; private set; }

        internal override void Open()
        {
            WasAccountDeleted = false;
            _errorLabel.Visible = false;
            HideConfirmation();
            _accountDeletionFlow.Open();
            RefreshButtons();
        }

        internal override void Update(GameTime gameTime)
        {
            AccountDeletionOutcome? outcome = _accountDeletionFlow.TakeOutcome();
            if (outcome is not null)
            {
                Show(outcome);
            }

            RefreshButtons();
        }

        internal void ClearRequests()
        {
            WasAccountDeleted = false;
        }

        protected override Widget Build()
        {
            VerticalStackPanel card = PanelFactory.CreateCard(Sizes.AccountSettingsCardWidth);
            card.Widgets.Add(BuildDeleteBlock());

            VerticalStackPanel page = PanelFactory.CreatePage();
            page.Widgets.Add(card);
            StackPanel.SetProportionType(card, ProportionType.Fill);

            _backButton.Click += BackButtonOnClick;
            page.Widgets.Add(PanelFactory.CreateBackBar(_backButton));

            return page;
        }

        private VerticalStackPanel BuildDeleteBlock()
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

            _deleteButton.HorizontalAlignment = HorizontalAlignment.Left;
            _deleteButton.Click += DeleteButtonOnClick;
            deleteAccountBlock.Widgets.Add(_deleteButton);
            deleteAccountBlock.Widgets.Add(_errorLabel);
            deleteAccountBlock.Widgets.Add(ConfirmationPlaceholder);

            return deleteAccountBlock;
        }

        private void Show(AccountDeletionOutcome outcome)
        {
            if (outcome.WasDeleted)
            {
                WasAccountDeleted = true;
            }
            else if (outcome.ErrorKey is string errorKey)
            {
                _errorLabel.TextKey = errorKey;
                _errorLabel.Visible = true;
            }
        }

        private void RefreshButtons()
        {
            _deleteButton.Enabled = !_accountDeletionFlow.IsDeleting;
            _backButton.Enabled = !_accountDeletionFlow.IsDeleting;
        }

        private void AskDeletionConfirmation()
        {
            _errorLabel.Visible = false;
            ShowConfirmation(TextKeys.Alerts.DeleteAccountTitle, TextKeys.Alerts.DeleteAccountBody);

            LocalizedButton deleteConfirmButton = ButtonFactory.CreateDestructiveButton(TextKeys.Alerts.DeleteConfirmButton);
            deleteConfirmButton.Click += DeleteConfirmButtonOnClick;
            LocalizedButton cancelButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.CancelButton);
            cancelButton.Click += CancelButtonOnClick;
            AddConfirmationButtons(deleteConfirmButton, cancelButton);
        }

        private void DeleteButtonOnClick(object sender, MyraEventArgs e)
        {
            AskDeletionConfirmation();
        }

        private void DeleteConfirmButtonOnClick(object sender, MyraEventArgs e)
        {
            HideConfirmation();
            _accountDeletionFlow.DeleteAccount();
            RefreshButtons();
        }

        private void CancelButtonOnClick(object sender, MyraEventArgs e)
        {
            HideConfirmation();
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Profile;
        }
    }
}
