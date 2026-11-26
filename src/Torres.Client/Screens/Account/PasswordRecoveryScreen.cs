using System;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class PasswordRecoveryScreen : Screen
    {
        private readonly PasswordRecoveryFlow _recoveryFlow;
        private readonly VerticalStackPanel _emailStep = PanelFactory.CreateCard(Sizes.RecoveryCardWidth);
        private readonly VerticalStackPanel _codeStep = PanelFactory.CreateCard(Sizes.RecoveryCardWidth);
        private readonly VerticalStackPanel _passwordStep = PanelFactory.CreateCard(Sizes.RecoveryCardWidth);
        private readonly LabeledTextBox _emailField = new LabeledTextBox(TextKeys.PasswordRecovery.EmailLabel, false, AccountLimits.LongestEmail);
        private readonly LabeledTextBox _codeField = new LabeledTextBox(TextKeys.PasswordRecovery.CodeLabel, false, AccountLimits.VerificationCodeLength);
        private readonly LabeledTextBox _newPasswordField = new LabeledTextBox(TextKeys.PasswordRecovery.NewPasswordLabel, true, AccountLimits.LongestPassword);
        private readonly LabeledTextBox _repeatPasswordField = new LabeledTextBox(TextKeys.PasswordRecovery.RepeatPasswordLabel, true, AccountLimits.LongestPassword);
        private readonly LocalizedLabel _emailError = LabelFactory.CreateErrorMessage(TextKeys.PasswordRecovery.InvalidEmail);
        private readonly LocalizedLabel _codeError = LabelFactory.CreateErrorMessage(TextKeys.PasswordRecovery.InvalidCode);
        private readonly LocalizedLabel _passwordError = LabelFactory.CreateErrorMessage(TextKeys.PasswordRecovery.PasswordsDoNotMatch);
        private readonly LocalizedButton _sendCodeButton = ButtonFactory.CreatePrimaryButton(TextKeys.PasswordRecovery.SendCodeButton);
        private readonly LocalizedButton _verifyButton = ButtonFactory.CreatePrimaryButton(TextKeys.PasswordRecovery.VerifyButton);
        private readonly LocalizedButton _saveButton = ButtonFactory.CreatePrimaryButton(TextKeys.PasswordRecovery.SaveButton);

        internal PasswordRecoveryScreen(PasswordRecoveryFlow recoveryFlow)
            : base(TextKeys.PasswordRecovery.HeaderLabel, true)
        {
            _recoveryFlow = recoveryFlow;
        }

        internal bool WasServerUnreachable { get; private set; }

        internal bool WasPasswordReset { get; private set; }

        internal void ClearRequests()
        {
            WasServerUnreachable = false;
            WasPasswordReset = false;
        }

        internal override void Open()
        {
            _codeField.Box.Text = string.Empty;
            _newPasswordField.Box.Text = string.Empty;
            _repeatPasswordField.Box.Text = string.Empty;
            _emailError.Visible = false;
            ShowStep(_emailStep);
        }

        internal override void Update(GameTime gameTime)
        {
            PasswordRecoveryOutcome? outcome = _recoveryFlow.TakeOutcome();
            if (outcome is not null)
            {
                Show(outcome);
            }

            _sendCodeButton.Enabled = !_recoveryFlow.IsSendingCode;
            _verifyButton.Enabled = !_recoveryFlow.IsVerifying;
            _saveButton.Enabled = !_recoveryFlow.IsSaving;
        }

        protected override Widget Build()
        {
            var screenLayout = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            };
            screenLayout.Widgets.Add(BuildEmailStep());
            screenLayout.Widgets.Add(BuildCodeStep());
            screenLayout.Widgets.Add(BuildPasswordStep());

            ShowStep(_emailStep);

            return screenLayout;
        }

        private VerticalStackPanel BuildEmailStep()
        {
            _sendCodeButton.Click += SendCodeButtonOnClick;
            _sendCodeButton.HorizontalAlignment = HorizontalAlignment.Left;
            VerticalStackPanel stepActions = PanelFactory.CreateWrappedButtonRow();
            stepActions.Widgets.Add(_sendCodeButton);
            stepActions.Widgets.Add(BackToLoginButton());

            _emailStep.HorizontalAlignment = HorizontalAlignment.Left;
            _emailStep.Widgets.Add(PanelFactory.CreateCardHeader(TextKeys.PasswordRecovery.Step1Title, TextKeys.PasswordRecovery.Step1Hint));
            _emailStep.Widgets.Add(BuildField(_emailField, _emailError));
            _emailStep.Widgets.Add(stepActions);

            return _emailStep;
        }

        private VerticalStackPanel BuildCodeStep()
        {
            LocalizedButton newCodeButton = ButtonFactory.CreateSecondaryButton(TextKeys.Common.BackButton);
            _verifyButton.Click += VerifyButtonOnClick;
            newCodeButton.Click += NewCodeButtonOnClick;
            HorizontalStackPanel stepActions = PanelFactory.CreateButtonRow();
            stepActions.Widgets.Add(_verifyButton);
            stepActions.Widgets.Add(newCodeButton);

            _codeStep.HorizontalAlignment = HorizontalAlignment.Left;
            _codeStep.Visible = false;
            _codeStep.Widgets.Add(PanelFactory.CreateCardHeader(TextKeys.PasswordRecovery.Step2Title, TextKeys.PasswordRecovery.Step2Hint));
            _codeStep.Widgets.Add(BuildField(_codeField, _codeError));
            _codeStep.Widgets.Add(stepActions);

            return _codeStep;
        }

        private VerticalStackPanel BuildPasswordStep()
        {
            _saveButton.Click += SaveButtonOnClick;
            _saveButton.HorizontalAlignment = HorizontalAlignment.Left;
            VerticalStackPanel stepActions = PanelFactory.CreateWrappedButtonRow();
            stepActions.Widgets.Add(_saveButton);
            stepActions.Widgets.Add(BackToLoginButton());

            _passwordStep.HorizontalAlignment = HorizontalAlignment.Left;
            _passwordStep.Visible = false;
            _passwordStep.Widgets.Add(PanelFactory.CreateCardHeader(TextKeys.PasswordRecovery.Step3Title, TextKeys.PasswordRecovery.Step3Hint));
            _passwordStep.Widgets.Add(_newPasswordField);
            _passwordStep.Widgets.Add(BuildField(_repeatPasswordField, _passwordError));
            _passwordStep.Widgets.Add(stepActions);

            return _passwordStep;
        }

        private static VerticalStackPanel BuildField(LabeledTextBox textBox, LocalizedLabel messageLabel)
        {
            var fieldGroup = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            fieldGroup.Widgets.Add(textBox);
            fieldGroup.Widgets.Add(messageLabel);

            return fieldGroup;
        }

        private LocalizedButton BackToLoginButton()
        {
            LocalizedButton backButton = ButtonFactory.CreateSecondaryButton(TextKeys.PasswordRecovery.BackToLoginButton);
            backButton.HorizontalAlignment = HorizontalAlignment.Left;
            backButton.Click += BackButtonOnClick;

            return backButton;
        }

        private void ShowStep(VerticalStackPanel visibleStep)
        {
            _emailStep.Visible = visibleStep == _emailStep;
            _codeStep.Visible = visibleStep == _codeStep;
            _passwordStep.Visible = visibleStep == _passwordStep;
        }

        private void SendCodeButtonOnClick(object sender, MyraEventArgs e)
        {
            bool isValid = _recoveryFlow.StartCodeRequest(_emailField.Value);
            ShowMessage(_emailError, TextKeys.PasswordRecovery.InvalidEmail, !isValid);
        }

        private void VerifyButtonOnClick(object sender, MyraEventArgs e)
        {
            bool isTyped = _recoveryFlow.StartVerification(_codeField.Value);
            ShowMessage(_codeError, TextKeys.PasswordRecovery.InvalidCode, !isTyped);
        }

        private void SaveButtonOnClick(object sender, MyraEventArgs e)
        {
            bool isRepeated = _recoveryFlow.StartReset(_newPasswordField.Value, _repeatPasswordField.Value);
            ShowMessage(_passwordError, TextKeys.PasswordRecovery.PasswordsDoNotMatch, !isRepeated);
        }

        private void Show(PasswordRecoveryOutcome outcome)
        {
            if (outcome.Step is PasswordRecoveryStep step)
            {
                ShowRecoveryStep(step);
            }

            if ((outcome.MessageStep is PasswordRecoveryStep messageStep) && (outcome.MessageKey is string messageKey))
            {
                ShowMessage(FindErrorLabel(messageStep), messageKey, true);
            }

            WasServerUnreachable |= outcome.Ending == PasswordRecoveryEnding.ServerUnreachable;
            WasPasswordReset |= outcome.Ending == PasswordRecoveryEnding.PasswordReset;
        }

        private void ShowRecoveryStep(PasswordRecoveryStep step)
        {
            if (step == PasswordRecoveryStep.Code)
            {
                _codeField.Box.Text = string.Empty;
            }

            FindErrorLabel(step).Visible = false;
            ShowStep(FindStepPanel(step));
        }

        private VerticalStackPanel FindStepPanel(PasswordRecoveryStep step)
        {
            VerticalStackPanel stepPanel = step switch
            {
                PasswordRecoveryStep.Email => _emailStep,
                PasswordRecoveryStep.Code => _codeStep,
                PasswordRecoveryStep.Password => _passwordStep,
                _ => throw new ArgumentOutOfRangeException(nameof(step), step, null),
            };

            return stepPanel;
        }

        private LocalizedLabel FindErrorLabel(PasswordRecoveryStep step)
        {
            LocalizedLabel errorLabel = step switch
            {
                PasswordRecoveryStep.Email => _emailError,
                PasswordRecoveryStep.Code => _codeError,
                PasswordRecoveryStep.Password => _passwordError,
                _ => throw new ArgumentOutOfRangeException(nameof(step), step, null),
            };

            return errorLabel;
        }

        private static void ShowMessage(LocalizedLabel messageLabel, string textKey, bool isVisible)
        {
            messageLabel.TextKey = textKey;
            messageLabel.Visible = isVisible;
        }

        private void NewCodeButtonOnClick(object sender, MyraEventArgs e)
        {
            ShowStep(_emailStep);
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Login;
        }
    }
}
