using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class PasswordRecoveryScreen : Screen
    {
        private readonly VerticalStackPanel _emailStep = CreateCard(Sizes.RecoveryCardWidth);
        private readonly VerticalStackPanel _codeStep = CreateCard(Sizes.RecoveryCardWidth);
        private readonly VerticalStackPanel _passwordStep = CreateCard(Sizes.RecoveryCardWidth);
        private readonly LabeledTextBox _emailField = new LabeledTextBox(TextKeys.PasswordRecovery.EmailLabel, false);
        private readonly LabeledTextBox _codeField = new LabeledTextBox(TextKeys.PasswordRecovery.CodeLabel, false);
        private readonly LocalizedLabel _emailError = CreateErrorMessage(TextKeys.PasswordRecovery.InvalidEmail);
        private readonly LocalizedLabel _codeError = CreateErrorMessage(TextKeys.PasswordRecovery.InvalidCode);

        internal PasswordRecoveryScreen()
            : base(TextKeys.PasswordRecovery.HeaderLabel, true)
        {
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
            var emailFieldGroup = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            emailFieldGroup.Widgets.Add(_emailField);
            emailFieldGroup.Widgets.Add(_emailError);

            LocalizedButton sendCodeButton = CreatePrimaryButton(TextKeys.PasswordRecovery.SendCodeButton);
            sendCodeButton.Click += SendCodeButtonOnClick;
            sendCodeButton.HorizontalAlignment = HorizontalAlignment.Left;
            VerticalStackPanel stepActions = CreateWrappedButtonRow();
            stepActions.Widgets.Add(sendCodeButton);
            stepActions.Widgets.Add(BackToLoginButton());

            _emailStep.HorizontalAlignment = HorizontalAlignment.Left;
            _emailStep.Widgets.Add(CreateCardHeader(TextKeys.PasswordRecovery.Step1Title, TextKeys.PasswordRecovery.Step1Hint));
            _emailStep.Widgets.Add(emailFieldGroup);
            _emailStep.Widgets.Add(stepActions);

            return _emailStep;
        }

        private VerticalStackPanel BuildCodeStep()
        {
            var codeFieldGroup = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            codeFieldGroup.Widgets.Add(_codeField);
            codeFieldGroup.Widgets.Add(_codeError);

            LocalizedButton verifyButton = CreatePrimaryButton(TextKeys.PasswordRecovery.VerifyButton);
            verifyButton.Click += VerifyButtonOnClick;
            HorizontalStackPanel stepActions = CreateButtonRow();
            stepActions.Widgets.Add(verifyButton);

            _codeStep.HorizontalAlignment = HorizontalAlignment.Left;
            _codeStep.Visible = false;
            _codeStep.Widgets.Add(CreateCardHeader(TextKeys.PasswordRecovery.Step2Title, TextKeys.PasswordRecovery.Step2Hint));
            _codeStep.Widgets.Add(codeFieldGroup);
            _codeStep.Widgets.Add(stepActions);

            return _codeStep;
        }

        private VerticalStackPanel BuildPasswordStep()
        {
            LocalizedButton saveButton = CreatePrimaryButton(TextKeys.PasswordRecovery.SaveButton);
            saveButton.HorizontalAlignment = HorizontalAlignment.Left;
            VerticalStackPanel stepActions = CreateWrappedButtonRow();
            stepActions.Widgets.Add(saveButton);
            stepActions.Widgets.Add(BackToLoginButton());

            _passwordStep.HorizontalAlignment = HorizontalAlignment.Left;
            _passwordStep.Visible = false;
            _passwordStep.Widgets.Add(CreateCardHeader(TextKeys.PasswordRecovery.Step3Title, TextKeys.PasswordRecovery.Step3Hint));
            _passwordStep.Widgets.Add(new LabeledTextBox(TextKeys.PasswordRecovery.NewPasswordLabel, true));
            _passwordStep.Widgets.Add(new LabeledTextBox(TextKeys.PasswordRecovery.RepeatPasswordLabel, true));
            _passwordStep.Widgets.Add(stepActions);

            return _passwordStep;
        }

        private LocalizedButton BackToLoginButton()
        {
            LocalizedButton backButton = CreateSecondaryButton(TextKeys.PasswordRecovery.BackToLoginButton);
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

        private void SendCodeButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            bool isValid = _emailField.Value.Contains('@');
            _emailError.Visible = !isValid;
            if (isValid)
            {
                ShowStep(_codeStep);
            }
        }

        private void VerifyButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            bool isValid = _codeField.Value.Length > 0;
            _codeError.Visible = !isValid;
            if (isValid)
            {
                ShowStep(_passwordStep);
            }
        }

        private void BackButtonOnClick(object sender, Myra.Events.MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Login;
        }
    }
}
