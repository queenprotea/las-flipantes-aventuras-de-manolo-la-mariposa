using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class RegisterScreen : Screen
    {
        private readonly RegisterFlow _registerFlow;
        private readonly VerticalStackPanel _detailsStep = PanelFactory.CreateCard(Sizes.RegisterCardWidth);
        private readonly VerticalStackPanel _codeStep = PanelFactory.CreateCard(Sizes.RegisterCardWidth);
        private readonly LabeledTextBox _usernameField = new LabeledTextBox(TextKeys.Register.UsernameLabel, false, AccountLimits.LongestUsername);
        private readonly LabeledTextBox _emailField = new LabeledTextBox(TextKeys.Register.EmailLabel, false, AccountLimits.LongestEmail);
        private readonly LabeledTextBox _passwordField = new LabeledTextBox(TextKeys.Register.PasswordLabel, true, AccountLimits.LongestPassword);
        private readonly LabeledTextBox _codeField = new LabeledTextBox(TextKeys.Register.CodeLabel, false, AccountLimits.VerificationCodeLength);
        private readonly LocalizedLabel _usernameMessage = LabelFactory.CreateErrorMessage(TextKeys.Common.RequiredField);
        private readonly LocalizedLabel _emailMessage = LabelFactory.CreateErrorMessage(TextKeys.Common.RequiredField);
        private readonly LocalizedLabel _passwordMessage = LabelFactory.CreateErrorMessage(TextKeys.Common.RequiredField);
        private readonly LocalizedLabel _serverMessage = LabelFactory.CreateErrorMessage(TextKeys.Common.ServerErrorTitle);
        private readonly LocalizedLabel _codeMessage = LabelFactory.CreateErrorMessage(TextKeys.Register.InvalidCode);
        private readonly LocalizedButton _createAccountButton = ButtonFactory.CreatePrimaryButton(TextKeys.Register.CreateAccountButton);
        private readonly LocalizedButton _confirmButton = ButtonFactory.CreatePrimaryButton(TextKeys.Register.ConfirmButton);

        internal RegisterScreen(RegisterFlow registerFlow)
            : base(TextKeys.Register.HeaderLabel, true)
        {
            _registerFlow = registerFlow;
        }

        internal override void Open()
        {
            ShowStep(_detailsStep);
        }

        internal override void Update(GameTime gameTime)
        {
            RegisterOutcome? outcome = _registerFlow.TakeOutcome();
            if (outcome is not null)
            {
                Show(outcome);
            }

            _createAccountButton.Enabled = !_registerFlow.IsRequestingCode;
            _confirmButton.Enabled = !_registerFlow.IsConfirming;
        }

        protected override Widget Build()
        {
            var steps = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            steps.Widgets.Add(BuildDetailsStep());
            steps.Widgets.Add(BuildCodeStep());

            HorizontalStackPanel columns = PanelFactory.CreateColumns();
            columns.Widgets.Add(steps);
            columns.Widgets.Add(PanelFactory.CreateAmbiencePanel());
            StackPanel.SetProportionType(columns.Widgets[1], ProportionType.Fill);
            ShowStep(_detailsStep);

            return columns;
        }

        private VerticalStackPanel BuildDetailsStep()
        {
            _detailsStep.Widgets.Add(PanelFactory.CreateCardHeader(TextKeys.Register.Title, TextKeys.Register.Hint));
            _detailsStep.Widgets.Add(BuildField(_usernameField, _usernameMessage));
            _detailsStep.Widgets.Add(BuildField(_emailField, _emailMessage));
            _detailsStep.Widgets.Add(BuildField(_passwordField, _passwordMessage));
            _detailsStep.Widgets.Add(_serverMessage);

            LocalizedButton backButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.BackButton);
            _createAccountButton.Click += CreateAccountButtonOnClick;
            backButton.Click += BackButtonOnClick;
            HorizontalStackPanel formActions = PanelFactory.CreateButtonRow();
            formActions.Widgets.Add(_createAccountButton);
            formActions.Widgets.Add(backButton);
            _detailsStep.Widgets.Add(formActions);

            return _detailsStep;
        }

        private VerticalStackPanel BuildCodeStep()
        {
            _codeStep.Widgets.Add(PanelFactory.CreateCardHeader(TextKeys.Register.CodeStepTitle, TextKeys.Register.CodeStepHint));
            _codeStep.Widgets.Add(BuildField(_codeField, _codeMessage));

            LocalizedButton codeBackButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.BackButton);
            _confirmButton.Click += ConfirmButtonOnClick;
            codeBackButton.Click += CodeBackButtonOnClick;
            HorizontalStackPanel codeActions = PanelFactory.CreateButtonRow();
            codeActions.Widgets.Add(_confirmButton);
            codeActions.Widgets.Add(codeBackButton);
            _codeStep.Widgets.Add(codeActions);

            return _codeStep;
        }

        private static VerticalStackPanel BuildField(LabeledTextBox textBox, LocalizedLabel messageLabel)
        {
            var fieldGroup = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            fieldGroup.Widgets.Add(textBox);
            fieldGroup.Widgets.Add(messageLabel);

            return fieldGroup;
        }

        private void ShowStep(VerticalStackPanel visibleStep)
        {
            _detailsStep.Visible = visibleStep == _detailsStep;
            _codeStep.Visible = visibleStep == _codeStep;
        }

        private void CreateAccountButtonOnClick(object sender, MyraEventArgs e)
        {
            _serverMessage.Visible = false;
            IReadOnlyList<RegisterField> missingFields = _registerFlow.StartCodeRequest(_usernameField.Value, _emailField.Value, _passwordField.Value);
            ShowRequired(_usernameMessage, missingFields.Contains(RegisterField.Username));
            ShowRequired(_emailMessage, missingFields.Contains(RegisterField.Email));
            ShowRequired(_passwordMessage, missingFields.Contains(RegisterField.Password));
        }

        private void ConfirmButtonOnClick(object sender, MyraEventArgs e)
        {
            bool isCodeMissing = _registerFlow.StartConfirmation(_codeField.Value);
            ShowRequired(_codeMessage, isCodeMissing);
        }

        private static void ShowRequired(LocalizedLabel messageLabel, bool isMissing)
        {
            messageLabel.TextKey = TextKeys.Common.RequiredField;
            messageLabel.Visible = isMissing;
        }

        private void Show(RegisterOutcome outcome)
        {
            if (outcome.Step is RegisterStep step)
            {
                ShowStep(step);
            }

            if ((outcome.MessageField is RegisterField messageField) && (outcome.MessageKey is string messageKey))
            {
                ShowMessage(FindMessageLabel(messageField), messageKey);
            }

            if (outcome.NextScreen is ScreenId nextScreen)
            {
                _passwordField.Box.Text = string.Empty;
                _codeField.Box.Text = string.Empty;
                RequestedScreen = nextScreen;
            }
        }

        private void ShowStep(RegisterStep step)
        {
            if (step == RegisterStep.Code)
            {
                _codeField.Box.Text = string.Empty;
                _codeMessage.Visible = false;
                ShowStep(_codeStep);
            }
            else
            {
                ShowStep(_detailsStep);
            }
        }

        private LocalizedLabel FindMessageLabel(RegisterField messageField)
        {
            LocalizedLabel messageLabel = messageField switch
            {
                RegisterField.Username => _usernameMessage,
                RegisterField.Email => _emailMessage,
                RegisterField.Password => _passwordMessage,
                RegisterField.Server => _serverMessage,
                RegisterField.Code => _codeMessage,
                _ => throw new ArgumentOutOfRangeException(nameof(messageField), messageField, null),
            };

            return messageLabel;
        }

        private static void ShowMessage(LocalizedLabel messageLabel, string textKey)
        {
            messageLabel.TextKey = textKey;
            messageLabel.Visible = true;
        }

        private void CodeBackButtonOnClick(object sender, MyraEventArgs e)
        {
            ShowStep(_detailsStep);
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = RegisterFlow.FindScreenToCancel();
        }
    }
}
