using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class GuestAccessScreen : Screen
    {
        private readonly GuestAccessFlow _guestAccessFlow;
        private readonly LabeledTextBox _aliasField = new LabeledTextBox(TextKeys.GuestAccess.MatchNameLabel, false, AccountLimits.LongestGuestAlias);
        private readonly LocalizedLabel _aliasErrorLabel;
        private readonly LocalizedButton _enterButton;

        internal GuestAccessScreen(GuestAccessFlow guestAccessFlow)
            : base(TextKeys.GuestAccess.HeaderLabel, true)
        {
            _guestAccessFlow = guestAccessFlow;
            _aliasErrorLabel = LabelFactory.CreateErrorMessage(TextKeys.Common.RequiredField);
            _enterButton = ButtonFactory.CreatePrimaryButton(TextKeys.GuestAccess.EnterButton);
        }

        internal bool WasServerUnreachable { get; private set; }

        internal override void Open()
        {
            _aliasErrorLabel.Visible = false;
        }

        internal override void Update(GameTime gameTime)
        {
            GuestAccessOutcome? outcome = _guestAccessFlow.TakeOutcome();
            if (outcome is not null)
            {
                Show(outcome);
            }

            _enterButton.Enabled = !_guestAccessFlow.IsWaiting;
        }

        internal void ClearRequests()
        {
            WasServerUnreachable = false;
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = PanelFactory.CreateColumns();
            columns.Widgets.Add(BuildCard());
            columns.Widgets.Add(PanelFactory.CreateAmbiencePanel());
            StackPanel.SetProportionType(columns.Widgets[1], ProportionType.Fill);

            return columns;
        }

        private VerticalStackPanel BuildCard()
        {
            VerticalStackPanel card = PanelFactory.CreateCard(Sizes.GuestCardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(PanelFactory.CreateCardHeader(TextKeys.GuestAccess.Title, TextKeys.GuestAccess.Hint));

            var nameField = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            nameField.Widgets.Add(_aliasField);
            nameField.Widgets.Add(_aliasErrorLabel);
            nameField.Widgets.Add(LabelFactory.CreateSuccessMessage(TextKeys.GuestAccess.RepeatableName));
            card.Widgets.Add(nameField);

            card.Widgets.Add(LabelFactory.CreateParagraph(TextKeys.GuestAccess.Limitations));

            LocalizedButton backButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.BackButton);
            _enterButton.Click += EnterButtonOnClick;
            backButton.Click += BackButtonOnClick;
            HorizontalStackPanel guestActions = PanelFactory.CreateButtonRow();
            guestActions.Widgets.Add(_enterButton);
            guestActions.Widgets.Add(backButton);
            card.Widgets.Add(guestActions);

            return card;
        }

        private void EnterButtonOnClick(object sender, MyraEventArgs e)
        {
            _aliasErrorLabel.Visible = false;
            string? errorKey = _guestAccessFlow.StartGuestAccess(_aliasField.Value);
            if (errorKey is not null)
            {
                ShowError(errorKey);
            }
        }

        private void Show(GuestAccessOutcome outcome)
        {
            if (outcome.IsServerUnreachable)
            {
                WasServerUnreachable = true;
            }
            else if (outcome.ErrorKey is string errorKey)
            {
                ShowError(errorKey);
            }
            else if (outcome.NextScreen is ScreenId nextScreen)
            {
                _aliasField.Box.Text = string.Empty;
                RequestedScreen = nextScreen;
            }
        }

        private void ShowError(string textKey)
        {
            _aliasErrorLabel.TextKey = textKey;
            _aliasErrorLabel.Visible = true;
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Login;
        }
    }
}
